using CapeCode.Logging.Configuration;
using CapeCode.Logging.Emails.Interfaces;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging {
    public class EMailLogWriter : ILogWriter {

        private IEmailSender _emailSender;
        private LogLevel _minLogLevel = LogLevel.Error;
        private IEmailSenderConfiguration _emailSenderConfiguration;

        public EMailLogWriter( IEmailSender emailSender, IEmailSenderConfiguration emailSenderConfiguration ) {
            this._emailSender = emailSender;
            this._emailSenderConfiguration = emailSenderConfiguration;
        }

        #region ILogWriter Members

        public void WriteLogEvent( ILogEvent logEvent ) {
            if ( logEvent.Level >= _minLogLevel ) {
                string message = "Event Details\n\n";
                message += "Time:" + logEvent.Time + "\n";
                message += "Level:" + logEvent.Level + "\n";
                message += "Class:" + logEvent.CorrespondingType + "\n";
                message += "Method:" + logEvent.Method + "\n\n";
                message += "Message:" + logEvent.Message + "\n";
                var systemEmail = new SystemEmail( to: _emailSenderConfiguration.AdministratorEmailAddresses, cc: null, subject: string.Format( "[{0}]", logEvent.Level ), body: message );
                _emailSender.SendSystemEmail( systemEmail );
            }
        }

        public void InitSettings( ILogWriterConfiguration configuration ) {
            var eMailWriterConfiguration = ( EMailLogWriterConfigurationSection ) configuration;

            this._minLogLevel = eMailWriterConfiguration.MinimumLogLevel;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion

    }
}
