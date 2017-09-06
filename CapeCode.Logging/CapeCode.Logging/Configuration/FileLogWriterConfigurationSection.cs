using System;
using System.Configuration;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging.Configuration {
    public class FileLogWriterConfigurationSection : ConfigurationSection, ILogWriterConfiguration {

        public Type LogWriterType { get { return typeof( FileLogWriter ); } }

        [ConfigurationProperty( "FileName", DefaultValue = "", IsKey = false, IsRequired = true )]
        public string FileName {
            get { return ( ( string ) ( base[ "FileName" ] ) ); }
            set { base[ "FileName" ] = value; }
        }

        [ConfigurationProperty( "MinimumLogLevel", DefaultValue = "", IsKey = false, IsRequired = false )]
        public LogLevel MinimumLogLevel {
            get { return ( ( LogLevel ) ( base[ "LogWriterType" ] ) ); }
            //get { return ( (LogLevel)Enum.Parse(typeof(LogLevel), ((string) base[ "MinimumLogLevel" ] ), true) ); }
            set { base[ "MinimumLogLevel" ] = value; }
        }
    }
}
