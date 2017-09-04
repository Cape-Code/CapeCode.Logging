using System;
using System.Configuration;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging.Configuration {
    public class EMailLogWriterConfigurationSection : ConfigurationSection, ILogWriterConfiguration {

        public Type LogWriterType { get { return typeof( EMailLogWriter ); } }

        [ConfigurationProperty( "MinimumLogLevel", DefaultValue = "Error", IsKey = true, IsRequired = false )]
        public LogLevel MinimumLogLevel {
            get { return ( ( LogLevel ) ( base[ "MinimumLogLevel" ] ) ); }
            //get { return ( (LogLevel)Enum.Parse(typeof(LogLevel), ((string) base[ "MinimumLogLevel" ] ), true) ); }
            set { base[ "MinimumLogLevel" ] = value; }
        }

    }
}
