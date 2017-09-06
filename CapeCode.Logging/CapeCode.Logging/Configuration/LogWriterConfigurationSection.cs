using System;
using System.Configuration;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging.Configuration {
    public class LogWriterConfigurationSection : ConfigurationSection, ILogWriterConfiguration {

        public Type LogWriterType {
            get {
                if ( LogWriterTypeString != null ) {
                    return Type.GetType( LogWriterTypeString );
                } else {
                    return null;
                }
            }
            set {
                LogWriterTypeString = value.AssemblyQualifiedName;
            }
        }

        [ConfigurationProperty( "LogWriterTypeString", DefaultValue = null, IsKey = false, IsRequired = true )]
        public string LogWriterTypeString {
            get {
                return ( string ) base[ "LogWriterTypeString" ];
            }
            set { base[ "LogWriterTypeString" ] = value; }
        }
    }
}
