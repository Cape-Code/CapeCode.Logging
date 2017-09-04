using System;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging {
    public abstract class FilteringLogWriter : ILogWriter {

        public Func<ILogEvent, bool> LogFilter { get; set; }

        public FilteringLogWriter( Func<ILogEvent, bool> logFilter ) {
            LogFilter = logFilter;
        }

        #region ILogWriter Members

        public void WriteLogEvent( ILogEvent logEvent ) {
            if ( LogFilter( logEvent ) ) {
                WriteToMedium( logEvent );
            }
        }

        public abstract void InitSettings( ILogWriterConfiguration configuration );

        #endregion

        public abstract void WriteToMedium( ILogEvent logEvent );

        #region IDisposable Members

        public abstract void Dispose();

        #endregion
    }
}
