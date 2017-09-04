using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Runtime.Serialization;
using CapeCode.ExtensionMethods;

namespace CapeCode.Logging.Emails.Interfaces {
    [DataContract( IsReference = true )]
    public class Email {
        [DataMember]
        public MailPriority MailPriority { get; set; }

        [DataMember]
        public DeliveryNotificationOptions DeliveryNotificationOptions { get; set; }

        [DataMember]
        public string From { get; private set; }
        [DataMember]
        public IList<string> To { get; private set; }
        [DataMember]
        public IList<string> CC { get; private set; }
        [DataMember]
        public IList<string> BCC { get; private set; }
        [DataMember]
        public string Subject { get; private set; }
        [DataMember]
        public string Body { get; private set; }
        [DataMember]
        public IList<Attachment> Attachments { get; private set; }

        public Email( string from, IList<string> to, IList<string> cc, IList<string> bcc, string subject, string body, params Attachment[] attachments ) {
            MailPriority = MailPriority.Normal;
            DeliveryNotificationOptions = DeliveryNotificationOptions.None;

            if ( string.IsNullOrEmpty( from ) ) {
                throw new ArgumentException( "NULL/string.Empty is not allowed", "from" );
            }

            if ( to == null || to.Count == 0 ) {
                throw new ArgumentException( "NULL/Zero is not allowed", "to" );
            }

            if ( string.IsNullOrEmpty( subject ) ) {
                throw new ArgumentException( "NULL/string.Empty is not allowed", "subject" );
            }

            if ( body == null ) {
                throw new ArgumentException( "NULL is not allowed", "body" );
            }

            From = from;
            To = to;
            CC = cc ?? new List<string>();
            BCC = bcc ?? new List<string>();
            Subject = subject;
            Body = body;
            Attachments = new ReadOnlyCollection<Attachment>( attachments ?? new Attachment[] { } );
        }

        public override string ToString() {
            var emailString = string.Format( "Email: \nFrom: '{0}' \nTo: '{1}' \nCC: '{2}' \nBCC: '{3}' \nSubject: '{4}' \nBody:\n{5} \nAttachments: '{6}'.",
                From,
                To.ToSeparatedString( t => t ),
                CC.ToSeparatedString( c => c ),
                BCC.ToSeparatedString( b => b ),
                Subject,
                Body.Replace( @"<br\>", "\n" ).Replace( @"<br>", "\n" ).Replace( @"<br \>", "\n" ),
                Attachments != null ? Attachments.ToSeparatedString( a => a.FileName ) : string.Empty );
            return emailString;
        }

    }
}
