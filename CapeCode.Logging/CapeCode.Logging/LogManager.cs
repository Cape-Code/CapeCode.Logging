using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using CapeCode.DependencyInjection.Interfaces;
using CapeCode.Logging.Interfaces;
using Microsoft.Practices.Unity;

namespace CapeCode.Logging {

    [InjectAsGlobalSingleton( typeof( ILogManager ) )]
    public class LogManager : ILogManager, IDisposable {

        private readonly IList<ILogWriter> _logWriters = new List<ILogWriter>();
        protected readonly IDependencyResolver DependencyResolver;

        public LogManager( IDependencyResolver dependencyResolver, System.Configuration.Configuration configuration ) {
            this.DependencyResolver = dependencyResolver;
            LoadConfiguration( configuration );
        }

        [InjectionConstructor]
        public LogManager( IDependencyResolver dependencyResolver ) {
            this.DependencyResolver = dependencyResolver;

            //Open app.config or web.config file 
            System.Configuration.Configuration configuration;
            if ( HttpContext.Current != null ) {
                configuration = WebConfigurationManager.OpenWebConfiguration( "~" );
            } else {
                configuration = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
            }

            LoadConfiguration( configuration );
        }

        private void LoadConfiguration( System.Configuration.Configuration configuration ) {
            foreach ( ConfigurationSection section in configuration.Sections ) {
                if ( typeof( ILogWriterConfiguration ).IsAssignableFrom( section.GetType() ) ) {
                    ILogWriterConfiguration logWriterConfiguration = ( ILogWriterConfiguration ) section;
                    ILogWriter logWriter = ( ILogWriter ) this.DependencyResolver.Resolve( logWriterConfiguration.LogWriterType );
                    logWriter.InitSettings( logWriterConfiguration );
                    RegisterLogWriter( logWriter );
                }
            }
        }

        #region ILogManager Members

        public virtual void DistributeLogEvent( ILogEvent logEvent ) {
            IList<Exception> exceptions = new List<Exception>();
            foreach ( ILogWriter writer in _logWriters ) {
                try {
                    writer.WriteLogEvent( logEvent );
                } catch ( Exception ex ) {
                    // if an exception occurs, try to write at lease to the other writers
                    exceptions.Add( ex );
                }
            }
            if ( exceptions.Count > 0 ) {
                throw new Exception( string.Format( "DistributeLogEvent failed for some logWriters:  {0}", exceptions.Aggregate( string.Empty, ( result, value ) => result += value + "\n\n" ) ) );
            }
        }

        public void RegisterLogWriter( ILogWriter logWriter ) {
            _logWriters.Add( logWriter );
        }

        public void UnregisterLogWriter( ILogWriter logWriter ) {
            _logWriters.Remove( logWriter );
        }

        public IList<ILogWriter> RegisterDefaultLogWriter( LogLevel logLevel = LogLevel.Trace ) {
            IList<ILogWriter> logWriters = new List<ILogWriter>();

            if ( Environment.UserInteractive ) {
                if ( Environment.CommandLine.Contains( "QTAgent" ) ) {
                    // Unit Test
                    logWriters.Add( new TraceLogWriter( logLevel ) );
                } else if ( Debugger.IsAttached ) {
                    // Visual Studio with Debugger
                    logWriters.Add( new VisualStudioLogWriter( le => le.ToString(), le => le.Level >= logLevel ) );
                } else {
                    // Standalone
                    logWriters.Add( new WindowLogWriter( le => le.ToString(), le => le.Level >= logLevel ) );
                    logWriters.Add( new TraceLogWriter( logLevel ) );
                }
            } else if ( Environment.CommandLine.Contains( "QTAgent" ) ) {
                // Unit Test on TFS
                logWriters.Add( new DebugLogWriter( le => le.ToString(), le => le.Level >= logLevel ) );
            } else {
                logWriters.Add( new TraceLogWriter( logLevel ) );
            }
            foreach ( var logWriter in logWriters ) {
                this.RegisterLogWriter( logWriter );
            }

            return logWriters;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            foreach ( ILogWriter logWriter in _logWriters.ToList() ) {
                _logWriters.Remove( logWriter );
                logWriter.Dispose();
            }
        }

        #endregion
    }
}
