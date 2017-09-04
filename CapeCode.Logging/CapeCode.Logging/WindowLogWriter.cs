using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CapeCode.Logging.Interfaces;

namespace CapeCode.Logging {
    public class WindowLogWriter : FormattingLogWriter<string> {

        private Form _outputWindow;
        private TextBox _outputTextBox;
        private readonly Thread _windowThread;

        public delegate void BeforeQuit();
        private readonly BeforeQuit _beforeQuit;

        public WindowLogWriter( Func<ILogEvent, string> logFormatter, Func<ILogEvent, bool> logFilter, BeforeQuit beforeQuit = null )
            : base( logFormatter, logFilter ) {

            _beforeQuit = beforeQuit;

            _windowThread = new Thread( new ThreadStart( WindowThread ) );
            _windowThread.SetApartmentState( ApartmentState.STA );
            _windowThread.Start();

            while ( _outputWindow == null || !_outputWindow.Created ) {
                // wait until the window has been created, otherwise no message can be logged to the window
                Application.DoEvents();
            }
        }

        public void WindowThread() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            _outputWindow = new Form { WindowState = FormWindowState.Maximized, Text = Assembly.GetEntryAssembly().FullName };

            _outputTextBox = new TextBox { Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false };
            _outputWindow.Controls.Add( _outputTextBox );

            _outputWindow.Resize += new EventHandler( OutputWindowResize );
            _outputWindow.Closed += new EventHandler( OutputWindowClosed );

            Application.Run( _outputWindow );
        }

        void OutputWindowResize( object sender, EventArgs e ) {
            _outputTextBox.Width = _outputWindow.Width - 20;
            _outputTextBox.Height = _outputWindow.Height - 40;
        }

        void OutputWindowClosed( object sender, EventArgs e ) {
            if ( _beforeQuit != null ) {
                _beforeQuit.DynamicInvoke();
            }
            Environment.Exit( 0 );
        }

        #region FormattingLogWriter Members

        public override void InitSettings( ILogWriterConfiguration configuration ) {

        }

        public override void WriteFormattedToMedium( string message ) {
            try {
                _outputWindow.Invoke( new MethodInvoker( () => this._outputTextBox.AppendText( message + "\n" ) ) );
            } catch ( System.ObjectDisposedException ) {
                // A logger should not crash the system. This catches the exception which is been thrown when the console has already been closed.
            }
        }

        public override void Dispose() {
            if ( _outputWindow != null ) {
                _outputWindow.Invoke( new MethodInvoker( Application.ExitThread ) );
            }
        }

        #endregion
    }
}
