using System;
using System.Configuration;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging.Configuration {

    public class PoolingEMailLogWriterConfigurationSection : ConfigurationSection, ILogWriterConfiguration {

        public Type LogWriterType { get { return typeof( PoolingEMailLogWriter ); } }

        [ConfigurationProperty( "MinimumLogLevel", DefaultValue = "Error", IsKey = true, IsRequired = false )]
        public LogLevel MinimumLogLevel {
            get { return ( ( LogLevel ) ( base[ "MinimumLogLevel" ] ) ); }
            set { base[ "MinimumLogLevel" ] = value; }
        }

        [ConfigurationProperty( "InitialPoolingSeconds", DefaultValue = "30", IsKey = true, IsRequired = false )]
        public int InitialPoolingSeconds {
            get { return ( ( int ) ( base[ "InitialPoolingSeconds" ] ) ); }
            set { base[ "InitialPoolingSeconds" ] = value; }
        }

        [ConfigurationProperty( "IncrementPoolingSeconds", DefaultValue = "60", IsKey = true, IsRequired = false )]
        public int IncrementPoolingSeconds {
            get { return ( ( int ) ( base[ "IncrementPoolingSeconds" ] ) ); }
            set { base[ "IncrementPoolingSeconds" ] = value; }
        }

    }
}
