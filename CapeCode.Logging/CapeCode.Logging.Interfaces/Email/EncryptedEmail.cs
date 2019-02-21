using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Runtime.Serialization;
using CapeCode.ExtensionMethods;

namespace CapeCode.Logging.Emails.Interfaces {
    [DataContract( IsReference = true )]
    public class EncryptedEmail {
        [DataMember]
        public MailPriority MailPriority { get; set; }

        [DataMember]
        public DeliveryNotificationOptions DeliveryNotificationOptions { get; set; }

        [DataMember]
        public string From { get; private set; }

        [DataMember]
        public IList<SecureMailAddress> Recipients { get; private set; }

        [DataMember]
        public string Subject { get; private set; }

        [DataMember]
        public string Body { get; private set; }

        [DataMember]
        public IList<Attachment> Attachments { get; private set; }

        public EncryptedEmail( MailPriority mailPriority, DeliveryNotificationOptions deliveryNotificationOptions, string from, IList<SecureMailAddress> recipients, string subject, string body, params Attachment[] attachments ) {
            if ( string.IsNullOrEmpty( from ) ) {
                throw new ArgumentException( "NULL/string.Empty is not allowed", "from" );
            }

            if ( recipients == null || recipients.Count == 0 ) {
                throw new ArgumentException( "NULL/Zero is not allowed", "recipients" );
            }

            if ( string.IsNullOrEmpty( subject ) ) {
                throw new ArgumentException( "NULL/string.Empty is not allowed", "subject" );
            }

            if ( body == null ) {
                throw new ArgumentException( "NULL is not allowed", "body" );
            }

            this.MailPriority = mailPriority;
            this.DeliveryNotificationOptions = deliveryNotificationOptions;
            this.From = from;
            this.Recipients = recipients;
            this.Subject = subject;
            this.Body = body;
            this.Attachments = attachments;
        }

        public override string ToString() {
            var emailString = string.Format( "Email: \nMailPriority: {0} \nDeliveryNotificationOptions: {1} \nFrom: '{2}' \nTo: '{3}' \nSubject: '{4}' \nBody:\n{5}",
                    this.MailPriority,
                    this.DeliveryNotificationOptions,
                    this.From,
                    this.Recipients.ToSeparatedString( t => t.Address ),
                    this.Subject,
                    this.Body.Replace( @"<br\>", "\n" ).Replace( @"<br>", "\n" ).Replace( @"<br \>", "\n" ) );
            return emailString;
        }
    }
}