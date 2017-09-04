
using System;
using System.Collections.Generic;

namespace CapeCode.Logging.Interfaces {
    public interface ILogManager : IDisposable {

        void DistributeLogEvent( ILogEvent logEvent );
        void RegisterLogWriter( ILogWriter logWriter );
        void UnregisterLogWriter( ILogWriter logWriter );
        IList<ILogWriter> RegisterDefaultLogWriter( LogLevel logLevel = LogLevel.Trace );
    }
}
