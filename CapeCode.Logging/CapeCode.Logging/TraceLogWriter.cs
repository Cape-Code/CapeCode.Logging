using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using CapeCode.Logging.Interfaces;
using CapeCode.ExtensionMethods;

namespace CapeCode.Logging {
    public class TraceLogWriter : ILogWriter, IDisposable {
        private TraceSource globalTraceSource;
        private IDictionary<Assembly, TraceSource> traceSourceByAssembly;

        public TraceLogWriter( LogLevel logLevel, IList<Assembly> assemblies = null ) {
            globalTraceSource = new TraceSource( "Global", ConvertLogLevelToSourceLevels( logLevel ) );
            if ( assemblies != null ) {
                traceSourceByAssembly = assemblies.ToDictionary( k => k, v => new TraceSource( v.ManifestModule.Name.Replace( ".dll", "" ).Replace( ".exe", "" ), ConvertLogLevelToSourceLevels( logLevel ) ) );
            } else {
                traceSourceByAssembly = new Dictionary<Assembly, TraceSource>();
            }
        }

        #region ILogWriter Members

        public void WriteLogEvent( ILogEvent logEvent ) {
            WriteToMedium( logEvent );
        }

        #endregion

        public void WriteToMedium( ILogEvent logEvent ) {
            if ( OperationContext.Current != null && OperationContext.Current.IncomingMessageHeaders != null && OperationContext.Current.IncomingMessageHeaders.MessageId != null && OperationContext.Current.IncomingMessageHeaders.MessageId.IsGuid ) {
                Guid messageGuid;
                OperationContext.Current.IncomingMessageHeaders.MessageId.TryGetGuid( out messageGuid );
                using ( new ActivityTracerScope( globalTraceSource, logEvent.Method, messageGuid ) ) {
                    globalTraceSource.TraceEvent( ConvertLogLevelToTraceEventType( logEvent.Level ), 10, logEvent.ToString() );
                }
            } else {
                globalTraceSource.TraceEvent( ConvertLogLevelToTraceEventType( logEvent.Level ), 10, logEvent.ToString() );
            }
        }

        private TraceEventType ConvertLogLevelToTraceEventType( LogLevel logLevel ) {
            TraceEventType traceEventType;
            switch ( logLevel ) {
                case LogLevel.Trace:
                    traceEventType = TraceEventType.Verbose;
                    break;
                case LogLevel.Debug:
                    traceEventType = TraceEventType.Information;
                    break;
                case LogLevel.Info:
                    traceEventType = TraceEventType.Information;
                    break;
                case LogLevel.Warning:
                    traceEventType = TraceEventType.Warning;
                    break;
                case LogLevel.Error:
                    traceEventType = TraceEventType.Error;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return traceEventType;
        }

        private SourceLevels ConvertLogLevelToSourceLevels( LogLevel logLevel ) {
            SourceLevels sourceLevel;
            switch ( logLevel ) {
                case LogLevel.Trace:
                    sourceLevel = SourceLevels.Verbose;
                    break;
                case LogLevel.Debug:
                    sourceLevel = SourceLevels.Information;
                    break;
                case LogLevel.Info:
                    sourceLevel = SourceLevels.Information;
                    break;
                case LogLevel.Warning:
                    sourceLevel = SourceLevels.Warning;
                    break;
                case LogLevel.Error:
                    sourceLevel = SourceLevels.Error;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return sourceLevel;
        }

        public void InitSettings( ILogWriterConfiguration configuration ) {
        }

        #region IDisposable Members

        public void Dispose() {
            globalTraceSource.Close();
            traceSourceByAssembly.ForEach( ta => ta.Value.Close() );
        }

        #endregion

        public class ActivityTracerScope : IDisposable {
            private readonly Guid oldActivityId;
            private readonly Guid newActivityId;
            private readonly TraceSource ts;
            private readonly string activityName;

            public ActivityTracerScope( TraceSource ts, string activityName, Guid guid ) {
                this.ts = ts;
                this.oldActivityId = Trace.CorrelationManager.ActivityId;
                this.activityName = activityName;

                this.newActivityId = guid;

                if ( this.oldActivityId != Guid.Empty ) {
                    ts.TraceTransfer( 0, "Transferring to new activity...", newActivityId );
                }
                Trace.CorrelationManager.ActivityId = newActivityId;
                ts.TraceEvent( TraceEventType.Start, 0, activityName );
            }
            public void Dispose() {
                if ( this.oldActivityId != Guid.Empty ) {
                    ts.TraceTransfer( 0, "Transferring back to old activity...", oldActivityId );
                }
                ts.TraceEvent( TraceEventType.Stop, 0, activityName );
                Trace.CorrelationManager.ActivityId = oldActivityId;
            }
        }

    }
}
