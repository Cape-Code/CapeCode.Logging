using System;

namespace CapeCode.Logging.Emails.Interfaces {
    public class EmailSenderException : Exception {
        public EmailSenderException( string msg, Exception ex )
            : base( msg, ex ) {

        }

        public EmailSenderException( string msg )
            : base( msg ) {

        }
    }
}
