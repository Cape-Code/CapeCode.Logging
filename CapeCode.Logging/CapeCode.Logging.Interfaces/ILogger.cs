using System;
using System.Runtime.CompilerServices;

namespace CapeCode.Logging.Interfaces {
    public interface ILogger {

        Type ParentType { get; set; }

        [Obsolete]
        void Log( string message, string method, LogLevel level = LogLevel.Trace, Type source = null, params object[] formatArgs );

        [Obsolete]
        void LogError( string method, string message, params object[] args );
        [Obsolete]
        void LogDebug( string method, string message, params object[] args );
        [Obsolete]
        void LogInfo( string method, string message, params object[] args );
        [Obsolete]
        void LogTrace( string method, string message, params object[] args );
        [Obsolete]
        void LogWarning( string method, string message, params object[] args );

        void Log( string message, LogLevel level = LogLevel.Trace, Type source = null, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", params object[] formatArgs );

        void LogError( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" );
        void LogDebug( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" );
        void LogInfo( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" );
        void LogTrace( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" );
        void LogWarning( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" );
    }
}
