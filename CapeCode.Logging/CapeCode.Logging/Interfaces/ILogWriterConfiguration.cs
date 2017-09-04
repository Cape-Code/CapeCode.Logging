using System;

namespace CapeCode.Logging.Interfaces {
    public interface ILogWriterConfiguration {

        Type LogWriterType { get; }
    }
}
