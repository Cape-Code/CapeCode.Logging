using System;
using System.Diagnostics;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging {
    /// <summary>
    /// DebugLogWriter is intended for UnitTests to have live console output
    /// </summary>
    public class DebugLogWriter : FormattingLogWriter<string> {

        public DebugLogWriter( Func<ILogEvent, string> logFormatter, Func<ILogEvent, bool> logFilter )
            : base( logFormatter, logFilter ) {
        }

        public override void WriteFormattedToMedium( string message ) {
            Debug.WriteLine( message );
        }

        public override void InitSettings( ILogWriterConfiguration configuration ) {
        }

        #region IDisposable Members

        public override void Dispose() {
        }

        #endregion
    }
}
