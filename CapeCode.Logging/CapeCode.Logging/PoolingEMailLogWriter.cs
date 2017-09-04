using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CapeCode.Logging.Emails.Interfaces;
using CapeCode.Logging.Interfaces;
using CapeCode.Logging.Configuration;

namespace CapeCode.Logging {
    public class PoolingEMailLogWriter : ILogWriter {

        private readonly IEmailSender _emailSender;
        private readonly IEmailSenderConfiguration _emailSenderConfiguration;
        private LogLevel _minLogLevel = LogLevel.Error;
        private int _initialPoolingSeconds = 30;
        private int _incrementPoolingSeconds = 60;

        private readonly IDictionary<string, PooledLogEventQueue> _pooledLogEventQueueByMethodName = new Dictionary<string, PooledLogEventQueue>();
        private readonly object _pooledLogEventQueueByMethodNameLock = new object();

        public PoolingEMailLogWriter( IEmailSender emailSender, IEmailSenderConfiguration emailSenderConfiguration ) {
            _emailSender = emailSender;
            _emailSenderConfiguration = emailSenderConfiguration;
        }

        #region ILogWriter Members

        public void WriteLogEvent( ILogEvent logEvent ) {
            if ( logEvent.Level >= _minLogLevel ) {
                lock ( _pooledLogEventQueueByMethodNameLock ) {
                    var methodName = logEvent.CorrespondingType != null ? ( logEvent.CorrespondingType.Name + "." + logEvent.Method ) : logEvent.Method;
                    if ( _pooledLogEventQueueByMethodName.ContainsKey( methodName ) ) {
                        _pooledLogEventQueueByMethodName[ methodName ].PoolLogEvent( logEvent, _incrementPoolingSeconds );
                    } else {
                        SendLogEventEmail( logEvent );
                        _pooledLogEventQueueByMethodName[ methodName ] = new PooledLogEventQueue( logEvent, _initialPoolingSeconds );
                        Task.Factory.StartNew( obj => HandlePooledLogEventQueue( methodName ), "AIMOPoolingEMailLogWriter(" + methodName + ")" );
                    }
                }
            }
        }

        public void InitSettings( ILogWriterConfiguration configuration ) {
            var eMailWriterConfiguration = ( PoolingEMailLogWriterConfigurationSection ) configuration;

            _minLogLevel = eMailWriterConfiguration.MinimumLogLevel;
            _initialPoolingSeconds = eMailWriterConfiguration.InitialPoolingSeconds;
            _incrementPoolingSeconds = eMailWriterConfiguration.IncrementPoolingSeconds;
        }

        #endregion

        private void HandlePooledLogEventQueue( string methodName ) {
            DateTime? wait;
            var waitHandle = new EventWaitHandle( false, mode: EventResetMode.AutoReset );
            do {
                wait = null;
                lock ( _pooledLogEventQueueByMethodNameLock ) {
                    if ( _pooledLogEventQueueByMethodName.ContainsKey( methodName ) ) {
                        var pooledLogEventQueue = _pooledLogEventQueueByMethodName[ methodName ];
                        if ( pooledLogEventQueue.FinishPoolingDateTime < DateTime.Now ) {
                            if ( pooledLogEventQueue.Count > 1 ) {
                                SendPooledLogEventEmail( pooledLogEventQueue );
                            }
                            _pooledLogEventQueueByMethodName.Remove( methodName );
                        } else {
                            wait = pooledLogEventQueue.FinishPoolingDateTime;
                        }
                    }
                }
                if ( wait.HasValue ) {
                    waitHandle.WaitOne( wait.Value.Subtract( DateTime.Now ) );
                }
            } while ( wait.HasValue );
        }

        public void SendLogEventEmail( ILogEvent logEvent ) {
            string message = "Event Details\n\n";
            message += "Time:" + logEvent.Time + "\n";
            message += "Level:" + logEvent.Level + "\n";
            message += "Class:" + logEvent.CorrespondingType + "\n";
            message += "Method:" + logEvent.Method + "\n\n";
            message += "Message:" + logEvent.Message + "\n";
            var systemEmail = new SystemEmail( to: _emailSenderConfiguration.AdministratorEmailAddresses, cc: null, subject: string.Format( "[{0}]", logEvent.Level ), body: message );
            _emailSender.SendSystemEmail( systemEmail );
        }

        public void SendPooledLogEventEmail( PooledLogEventQueue pooledLogEventQueue ) {
            var pooledLogEvents = pooledLogEventQueue.GetPooledLogEvents();
            string message = "Event Details\n\n";
            message += "Time:" + pooledLogEventQueue.FirstLogEventDateTime + " to " + pooledLogEventQueue.LastLogEventDateTime + "\n";
            message += "Level:" + pooledLogEvents.First().Level + "\n";
            message += "Class:" + pooledLogEvents.First().CorrespondingType + "\n";
            message += "Method:" + pooledLogEvents.First().Method + "\n";
            message += "Count:" + pooledLogEvents.Count() + "\n\n";
            var maxMessages = Math.Min( 10, pooledLogEvents.Count() );
            for ( int i = 0; i < maxMessages; i++ ) {
                message += "Message" + i + ":" + pooledLogEvents[ i ].Message + "\n\n";
            }
            var systemEmail = new SystemEmail( to: _emailSenderConfiguration.AdministratorEmailAddresses, cc: null, subject: string.Format( "[{0}]", pooledLogEvents.First().Level ), body: message );
            _emailSender.SendSystemEmail( systemEmail );
        }

        public void Dispose() {
        }
    }

    public class PooledLogEventQueue {
        private readonly Queue<ILogEvent> _logEvents = new Queue<ILogEvent>();
        private DateTime _finishPoolingDateTime;
        public DateTime FinishPoolingDateTime {
            get {
                return _finishPoolingDateTime;
            }
        }
        private readonly DateTime _firstLogEventDateTime;
        public DateTime FirstLogEventDateTime {
            get {
                return _firstLogEventDateTime;
            }
        }
        private DateTime _lastLogEventDateTime;
        public DateTime LastLogEventDateTime {
            get {
                return _lastLogEventDateTime;
            }
        }
        public int Count { get; private set; }

        public PooledLogEventQueue( ILogEvent firstLogEvent, int initialPoolingSeconds = 30 ) {
            var initialPoolingTimeSpan = new TimeSpan( 0, 0, 0, initialPoolingSeconds );
            _firstLogEventDateTime = DateTime.Now;
            _lastLogEventDateTime = _firstLogEventDateTime;
            _finishPoolingDateTime = _firstLogEventDateTime.Add( initialPoolingTimeSpan );
            _logEvents.Enqueue( firstLogEvent );
            Count = 1;
        }

        public void PoolLogEvent( ILogEvent logEvent, int incrementPoolingSeconds = 60 ) {
            var incrementPoolingTimeSpan = new TimeSpan( 0, 0, 0, incrementPoolingSeconds );
            _lastLogEventDateTime = DateTime.Now;
            _finishPoolingDateTime = _lastLogEventDateTime.Add( incrementPoolingTimeSpan );
            _logEvents.Enqueue( logEvent );
            Count++;
        }

        public IList<ILogEvent> GetPooledLogEvents() {
            return _logEvents.ToList();
        }

    }
}
