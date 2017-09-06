using System.IO;
using CapeCode.Logging.Interfaces;
using CapeCode.Logging.Configuration;
using Microsoft.Practices.Unity;

namespace CapeCode.Logging {
    public class FileLogWriter : ILogWriter {

        private TextWriter _textWriter;
        private LogLevel _minLogLevel = LogLevel.Error;

        [InjectionConstructor]
        public FileLogWriter() {
        }

        public FileLogWriter( string filename, LogLevel minimumLogLevel ) {
            _textWriter = new StreamWriter( filename );
            _minLogLevel = minimumLogLevel;
        }

        #region ILogWriter Members

        public void WriteLogEvent( ILogEvent logEvent ) {
            if ( logEvent.Level >= _minLogLevel ) {
                _textWriter.WriteLine( logEvent.ToString() );
                _textWriter.Flush();
            }
        }

        public void InitSettings( ILogWriterConfiguration configuration ) {
            FileLogWriterConfigurationSection fileWriterConfiguration = ( FileLogWriterConfigurationSection ) configuration;
            _textWriter = new StreamWriter( fileWriterConfiguration.FileName );
            _minLogLevel = fileWriterConfiguration.MinimumLogLevel;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            _textWriter.Close();
        }

        #endregion
    }
}
