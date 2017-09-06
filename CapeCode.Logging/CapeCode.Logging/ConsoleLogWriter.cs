using System;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging {
    public class ConsoleLogWriter : FormattingLogWriter<string> {

        public ConsoleLogWriter( Func<ILogEvent, string> logFormatter, Func<ILogEvent, bool> logFilter )
            : base( logFormatter, logFilter ) {
        }

        public override void WriteFormattedToMedium( string message ) {
            try {
                Console.WriteLine( message );
            } catch ( System.ObjectDisposedException ) {
                // A logger should not crash the system. This catches the exception which is been thrown when the console has already been closed.
            }
        }

        public override void InitSettings( ILogWriterConfiguration configuration ) {
        }

        #region IDisposable Members

        public override void Dispose() {
        }

        #endregion
    }
}
