using System;
using System.Linq;
using System.Threading;
using CapeCode.Logging.Interfaces;
using CapeCode.ExtensionMethods;

namespace CapeCode.Logging {
    public class LogEvent : ILogEvent {

        #region ILogEvent Members

        public string Message { get; set; }

        public LogLevel Level { get; set; }

        public Type CorrespondingType { get; set; }

        public string Method { get; set; }

        public string FilePath { get; set; }

        public int LineNumber { get; set; }


        public DateTime Time { get; set; }

        #endregion

        public override string ToString() {
            return "[" + Time.ToString( "dd.MM.yy H:mm:ss" ) + "][" + Level.ToString() + "][Thread:" + Thread.CurrentThread.ManagedThreadId + "]" + FormatType( CorrespondingType ) + ( !string.IsNullOrEmpty( Method ) ? "." + Method : string.Empty ) + ": " + Message;
        }

        private string FormatType( Type type ) {
            if ( type != null ) {
                if ( type.GetGenericArguments().Count() > 0 ) {
                    return type.Name + ( CorrespondingType.GetGenericArguments().Aggregate( "[", ( result, input ) => result += FormatType( input ) + "|", result => result.RemoveLast( 1 ) + "]" ) );
                } else {
                    return type.Name;
                }
            } else {
                return string.Empty;
            }
        }
    }
}
