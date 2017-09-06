using System;
using System.Runtime.CompilerServices;
using CapeCode.DependencyInjection.Interfaces;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging {

    [InjectAsNewInstance( typeof( ILogger ) )]
    public class Logger : ILogger {

        protected readonly ILogManager _logManager;

        public Type ParentType { get; set; }

        public Logger( ILogManager logManager ) {
            _logManager = logManager;
        }

        #region ILogger Members

        public virtual void Log( string message, string method, LogLevel level = LogLevel.Trace, Type source = null, params object[] formatArgs ) {
            this.LogEvent( message, level, source, 0, string.Empty, method, formatArgs );
        }

        public virtual void Log( string message, LogLevel level = LogLevel.Trace, Type source = null, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", params object[] formatArgs ) {
            this.LogEvent( message, level, source, lineNumber, sourceFilePath, memberName, formatArgs );
        }



        public void LogError( string method, string message, params object[] args ) {
            LogEvent( message, LogLevel.Error, lineNumber: 0, sourceFilePath: string.Empty, memberName: method, formatArgs: args );
        }

        public void LogDebug( string method, string message, params object[] args ) {
            LogEvent( message, LogLevel.Debug, lineNumber: 0, sourceFilePath: string.Empty, memberName: method, formatArgs: args );
        }

        public void LogInfo( string method, string message, params object[] args ) {
            LogEvent( message, LogLevel.Info, lineNumber: 0, sourceFilePath: string.Empty, memberName: method, formatArgs: args );
        }

        public void LogTrace( string method, string message, params object[] args ) {
            LogEvent( message, LogLevel.Trace, lineNumber: 0, sourceFilePath: string.Empty, memberName: method, formatArgs: args );
        }

        public void LogWarning( string method, string message, params object[] args ) {
            LogEvent( message, LogLevel.Warning, lineNumber: 0, sourceFilePath: string.Empty, memberName: method, formatArgs: args );
        }



        public void LogError( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" ) {
            LogEvent( message, LogLevel.Error, lineNumber: lineNumber, sourceFilePath: sourceFilePath, memberName: memberName );
        }

        public void LogDebug( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" ) {
            LogEvent( message, LogLevel.Debug, lineNumber: lineNumber, sourceFilePath: sourceFilePath, memberName: memberName );
        }

        public void LogInfo( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" ) {
            LogEvent( message, LogLevel.Info, lineNumber: lineNumber, sourceFilePath: sourceFilePath, memberName: memberName );
        }

        public void LogTrace( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" ) {
            LogEvent( message, LogLevel.Trace, lineNumber: lineNumber, sourceFilePath: sourceFilePath, memberName: memberName );
        }

        public void LogWarning( string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" ) {
            LogEvent( message, LogLevel.Warning, lineNumber: lineNumber, sourceFilePath: sourceFilePath, memberName: memberName );
        }

        #endregion

        protected virtual void LogEvent( string message, LogLevel level = LogLevel.Trace, Type source = null, int lineNumber = 0, string sourceFilePath = "", string memberName = "", params object[] formatArgs ) {
            if ( source == null ) {
                source = ParentType;
            }
            string formattedMessage = message;
            if ( formatArgs.Length > 0 ) {
                try {
                    formattedMessage = string.Format( message, formatArgs );
                } catch ( Exception e ) {
                    DistributeLogEvent( CreateLogEvent( GetType(), LogLevel.Warning, 0, "", "Log", "string.Format threw exception. " + e ) );
                    formattedMessage = message;
                }
            }
            DistributeLogEvent( CreateLogEvent( source, level, lineNumber, sourceFilePath, memberName, formattedMessage ) );
        }

        protected virtual ILogEvent CreateLogEvent( Type source, LogLevel level, int lineNumber, string filePath, string method, string message ) {
            return new LogEvent() { Message = message, Method = method, Level = level, CorrespondingType = source, FilePath = filePath, LineNumber = lineNumber, Time = DateTime.Now };
        }

        protected virtual void DistributeLogEvent( ILogEvent logEvent ) {
            _logManager.DistributeLogEvent( logEvent );
        }

    }
}
