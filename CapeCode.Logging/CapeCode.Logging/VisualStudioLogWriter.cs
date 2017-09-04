using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using CapeCode.Logging.Interfaces;
using CapeCode.ExtensionMethods;

namespace CapeCode.Logging {
    public class VisualStudioLogWriter : FormattingLogWriter<string> {

        public VisualStudioLogWriter( Func<ILogEvent, string> logFormatter, Func<ILogEvent, bool> logFilter )
            : base( logFormatter, logFilter ) {
        }

        #region FormattingLogWriter Members

        public override void InitSettings( ILogWriterConfiguration configuration ) {

        }

        public override void WriteFormattedToMedium( string message ) {
            try {
                TraceLog( message );
            } catch ( System.ObjectDisposedException ) {
                // A logger should not crash the system. This catches the exception which is been thrown when the console has already been closed.
            }
        }

        public override void Dispose() {

        }

        #endregion

        /// 
        /// Format debug output the way VS likes
        /// 
        protected static void TraceLog( string message ) {

            var st = new StackTrace( true );
            StackFrame sf = st.GetFrame( 7 );
            if ( sf.GetFileName().EndsWith( @"\Logger.cs" ) ) {
                sf = st.GetFrame( 8 );
            }

            if ( message.Contains( "Exception" ) ) {
                // output the stack with clickable trace lines
                var messageLines = message.Split( "\n" );
                Trace.WriteLine( string.Format( "{0}({1},{2}): {3}", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber(), messageLines[ 0 ].RemoveLast( 1 ) ) );

                var regexForStackMethod = new Regex( @"(   at )(([a-zA-Z0-9äüöÄÜÖ._/\(\)\<\>\\-]|`[0-9]*[ ]{0,1}|[a-zA-Z0-9]* [a-zA-Z0-9]*, |, | [a-zA-Z0-9]*[\)])*)( in )(([a-zA-Z0-9äüöÄÜÖ.:_/\\-])*)(:line )([0-9]*)" );

                foreach ( var messageLine in messageLines.Skip( 1 ) ) {
                    var match = regexForStackMethod.Match( messageLine );

                    if ( match.Success ) {
                        var method = match.Groups[ 2 ];
                        var file = match.Groups[ 5 ];
                        var line = match.Groups[ 8 ];

                        Trace.WriteLine( string.Format( "{0}({1},{2}): {3}", file, line, 0, method ) );
                    } else {
                        Trace.WriteLine( string.Format( "{0}", messageLine.RemoveLast( 1 ) ) );
                    }
                }

            } else {
                Trace.WriteLine( string.Format( "{0}({1},{2}): {3}", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber(), message ) );
            }
        }
    }
}
