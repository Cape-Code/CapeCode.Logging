using System;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging {
    public abstract class FormattingLogWriter<T> : FilteringLogWriter {

        public Func<ILogEvent, T> LogFormatter { get; set; }

        public FormattingLogWriter( Func<ILogEvent, T> logFormatter, Func<ILogEvent, bool> logFilter )
            : base( logFilter ) {
            LogFormatter = logFormatter;
        }

        public override void WriteToMedium( ILogEvent logEvent ) {
            WriteFormattedToMedium( LogFormatter( logEvent ) );
        }

        public abstract void WriteFormattedToMedium( T message );
    }
}
