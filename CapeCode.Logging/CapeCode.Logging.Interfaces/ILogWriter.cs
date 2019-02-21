using System;

namespace CapeCode.Logging.Interfaces {
    public interface ILogWriter : IDisposable {

        void InitSettings( ILogWriterConfiguration configuration );
        void WriteLogEvent( ILogEvent logEvent );

    }
}
