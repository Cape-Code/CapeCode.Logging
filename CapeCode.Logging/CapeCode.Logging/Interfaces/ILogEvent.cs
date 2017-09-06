using System;

namespace CapeCode.Logging.Interfaces {
    public interface ILogEvent {

        string Message { get; set; }

        LogLevel Level { get; set; }

        Type CorrespondingType { get; set; }

        string Method { get; set; }

        string FilePath { get; set; }

        int LineNumber { get; set; }

        DateTime Time { get; set; }

    }
}
