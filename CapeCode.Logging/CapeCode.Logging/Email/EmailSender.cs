using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography.Pkcs;
using System.Text;
using CapeCode.DependencyInjection.Interfaces;
using CapeCode.Logging.Emails.Interfaces;
using CapeCode.Logging.Interfaces;
using CapeCode.ExtensionMethods;
using System.ComponentModel;

namespace CapeCode.Logging.Emails {
    [InjectAsNewInstancePerResolve]
    public class EmailSender : IEmailSender {
        public IEmailSenderConfiguration EmailSenderConfiguration { get; private set; }

        private readonly IDependencyResolver _dependencyResolver;
        protected ILogger Logger {
            get {
                if ( _logger == null ) {
                    _logger = _dependencyResolver.Resolve<ILogger>();
                    _logger.ParentType = this.GetType();
                }
                return _logger;
            }
        }

        private ILogger _logger;

        public EmailSender( IEmailSenderConfiguration emailSenderConfiguration, IDependencyResolver dependencyResolver ) {
            this.EmailSenderConfiguration = emailSenderConfiguration;
            this._dependencyResolver = dependencyResolver;
        }

        public void SendSystemEmail( SystemEmail systemEmail ) {
            var email = new Email( from: EmailSenderConfiguration.SystemMailAddress, to: systemEmail.To, cc: systemEmail.CC, bcc: systemEmail.BCC, subject: systemEmail.Subject, body: systemEmail.Body, attachments: systemEmail.Attachments.ToArray() ) {
                MailPriority = systemEmail.MailPriority,
                DeliveryNotificationOptions = systemEmail.DeliveryNotificationOptions
            };
            SendEmail( email );
        }

        public void SendPublicAccouncementSystemEmail( PublicAnnouncementSystemEmail publicAnnouncementEmail ) {
            var email = new Email( from: EmailSenderConfiguration.SystemMailAddress, to: new List<string>() { EmailSenderConfiguration.SystemMailAddress }, cc: publicAnnouncementEmail.CC, bcc: publicAnnouncementEmail.BCC, subject: publicAnnouncementEmail.Subject, body: publicAnnouncementEmail.Body, attachments: publicAnnouncementEmail.Attachments.ToArray() ) {
                MailPriority = publicAnnouncementEmail.MailPriority,
                DeliveryNotificationOptions = publicAnnouncementEmail.DeliveryNotificationOptions
            };
            SendEmail( email );
        }

        public void SendEncryptedEmail( EncryptedEmail encryptedEmail ) {
            Logger.LogTrace( $"Sending {encryptedEmail.ToString()}" );

            if ( EmailSenderConfiguration.IsDisabled ) {
                // used in unit-tests to prevent mass email sending...
                return;
            }

            IList<MailMessage> mailMessages;
            if ( EmailSenderConfiguration.InSimulationMode ) {
                Logger.LogWarning( $"InSimulation Mode -> Changing Recipients: {EmailSenderConfiguration.AdministratorEmailAddresses.ToSeparatedString( a => a )}" );

                var simulatedEmailBody = "<div style='font-family: Arial, Verdana; font-size: 10px; font-weight: bold'>" +
                                        "This mail would have been sent to:" +
                                        "<br/>" + encryptedEmail.Recipients.ToSeparatedString( t => t.Address ) +
                                        "<br /><br /></div>" +
                                         "-- The encrypted mail body is hidden in a simulated, unencrypted mail --";

                var simulatedEmail = new Email( from: encryptedEmail.From, to: EmailSenderConfiguration.AdministratorEmailAddresses, cc: null, bcc: null, subject: encryptedEmail.Subject, body: simulatedEmailBody, attachments: encryptedEmail.Attachments != null ? encryptedEmail.Attachments.ToArray() : null );

                mailMessages = ConvertEmailToMailMessages( simulatedEmail );

            } else {
                mailMessages = ConvertEncryptedEmailToMailMessages( encryptedEmail );
            }

            this.SendMailMessages( mailMessages );
        }

        public void SendEmail( Email email ) {

            Logger.LogTrace( $"Sending {email.ToString()}" );

            if ( EmailSenderConfiguration.IsDisabled ) {
                // used in unit-tests to prevent mass email sending...
                return;
            }

            IList<MailMessage> mailMessages;
            if ( EmailSenderConfiguration.InSimulationMode ) {
                Logger.LogWarning( $"InSimulation Mode -> Changing To: {EmailSenderConfiguration.AdministratorEmailAddresses.ToSeparatedString( a => a )} / CC: null / BCC: null" );

                var simulatedEmailBody = "<div style='font-family: Arial, Verdana; font-size: 10px; font-weight: bold'>" +
                                        "This mail would have been sent to:" +
                                        "<br/>" + email.To.ToSeparatedString( t => t ) +
                                        ( email.CC != null ? "<br/> cc: " + email.CC.ToSeparatedString( c => c ) : string.Empty ) +
                                        ( email.BCC != null ? "<br/> bcc: " + email.BCC.ToSeparatedString( c => c ) : string.Empty ) +
                                        "<br /><br /></div>" +
                                        email.Body;

                var simulatedEmail = new Email( from: email.From, to: EmailSenderConfiguration.AdministratorEmailAddresses, cc: null, bcc: null, subject: email.Subject, body: simulatedEmailBody, attachments: email.Attachments != null ? email.Attachments.ToArray() : null );

                mailMessages = ConvertEmailToMailMessages( simulatedEmail );

            } else {
                mailMessages = ConvertEmailToMailMessages( email );
            }

            this.SendMailMessages( mailMessages );
        }

        private void SendMailMessages( IEnumerable<MailMessage> mailMessages ) {
            if ( !TrySendMailMessages( mailMessages ) ) {
                System.Threading.Thread.Sleep( 1000 );
                TrySendMailMessages( mailMessages, true );
            }
        }

        private bool TrySendMailMessages( IEnumerable<MailMessage> mailMessages, bool isRetry = false ) {
            if ( string.IsNullOrEmpty( this.EmailSenderConfiguration.SmtpServerAddress ) ) {
                throw new ArgumentException( "SmtpServerAddress is not set", "EmailSenderConfiguration.SmtpServerAddress" );
            }

            try {
                var client = new SmtpClient( this.EmailSenderConfiguration.SmtpServerAddress );
                if ( !string.IsNullOrEmpty( this.EmailSenderConfiguration.SmtpServerUsername ) ) {
                    client.Credentials = new System.Net.NetworkCredential( this.EmailSenderConfiguration.SmtpServerUsername, this.EmailSenderConfiguration.SmtpServerPassword );
                }
                if ( this.EmailSenderConfiguration.SmtpServerUsesSSL ) {
                    client.EnableSsl = true;
                }

                foreach ( var mailMessage in mailMessages ) {
                    this.SendSmtpMailMessages( client, mailMessage );
                }
                this.Logger.LogTrace( "Successfully finished" );
            } catch ( Exception e ) {
                Exception innerMostException = e;
                while ( innerMostException.InnerException != null ) {
                    innerMostException = innerMostException.InnerException;
                }

                if ( innerMostException is Win32Exception && !isRetry ) {
                    return false;
                } else {
                    throw new EmailSenderException( "Email delivery failed because of an SMTP server error: " + innerMostException.Message, e );
                }
            }
            return true;
        }

        protected virtual void SendSmtpMailMessages( SmtpClient client, MailMessage mailMessage ) {
            client.Send( mailMessage );
        }

        private IList<MailMessage> ConvertEncryptedEmailToMailMessages( EncryptedEmail encryptedEmail ) {
            var mailMessages = new List<MailMessage>();

            var body = encryptedEmail.Body.Replace( "\r", "" ).Replace( "\n", "<br />" );

            var sb = new StringBuilder();
            if ( encryptedEmail.Attachments != null && encryptedEmail.Attachments.Count > 0 ) {

                sb.Append( "MIME-Version: 1.0\r\n" );
                sb.Append( "Content-Type: multipart/mixed; boundary=unique-boundary-1\r\n" );
                sb.Append( "\r\n" );
                sb.Append( "This is a multi-part message in MIME format.\r\n" );
                sb.Append( "--unique-boundary-1\r\n" );
                sb.Append( "Content-Type: text/html; charset=\"utf-8\"\r\n" );
                sb.Append( "Content-Transfer-Encoding: 7Bit\r\n\r\n" );
                sb.Append( body );
                if ( !body.EndsWith( "\r\n" ) )
                    sb.Append( "\r\n" );
                sb.Append( "\r\n\r\n" );

                foreach ( var attachment in encryptedEmail.Attachments ) {
                    sb.Append( "--unique-boundary-1\r\n" );
                    sb.Append( "Content-Type: application/octet-stream; file=" + attachment.FileName + "\r\n" );
                    sb.Append( "Content-Transfer-Encoding: base64\r\n" );
                    sb.Append( "Content-Disposition: attachment; filename=" + attachment.FileName + "\r\n" );
                    sb.Append( "\r\n" );

                    byte[] binaryData = attachment.Data;
                    string base64Value = Convert.ToBase64String( binaryData, 0, binaryData.Length );
                    int position = 0;

                    while ( position < base64Value.Length ) {
                        int chunkSize = 100;
                        if ( base64Value.Length - ( position + chunkSize ) < 0 )
                            chunkSize = base64Value.Length - position;
                        sb.Append( base64Value.Substring( position, chunkSize ) );
                        sb.Append( "\r\n" );
                        position += chunkSize;
                    }
                    sb.Append( "\r\n" );
                }
            } else {
                sb.AppendLine( "Content-Type: text/html; charset=\"utf-8\"" );
                sb.AppendLine( "Content-Transfer-Encoding: 7bit" );
                sb.AppendLine();
                sb.AppendLine( body );
            }

            var bodyData = Encoding.UTF8.GetBytes( sb.ToString() );

            foreach ( var recipient in encryptedEmail.Recipients ) {
                var envelopedCms = new EnvelopedCms( new ContentInfo( bodyData ) );
                var cmsRecipient = new CmsRecipient( SubjectIdentifierType.IssuerAndSerialNumber, recipient.Certificate );
                envelopedCms.Encrypt( cmsRecipient );
                var encryptedBytes = envelopedCms.Encode();

                var mailMessage = new MailMessage( encryptedEmail.From, recipient.Address ) {
                    Priority = encryptedEmail.MailPriority,
                    DeliveryNotificationOptions = encryptedEmail.DeliveryNotificationOptions,
                    Subject = encryptedEmail.Subject,
                    IsBodyHtml = true
                };
                mailMessage.AlternateViews.Add( new AlternateView( new MemoryStream( encryptedBytes ), "application/pkcs7-mime; smime-type=signed-data; name=smime.p7m" ) );

                if ( !string.IsNullOrEmpty( mailMessage.Subject ) ) {
                    while ( ( "=?utf-8?B?" + Convert.ToBase64String( mailMessage.Subject.ToByte( System.Text.Encoding.UTF8 ) ) + "?=" ).Length > 255 ) {
                        // Bugfix for some mailservers/clients which cannot handle UTF8-encoded and to Base64 converted strings which are longer than 255 characters (including control characters)
                        mailMessage.Subject = mailMessage.Subject.RemoveLast( 10 ) + "...";
                    }
                }

                mailMessages.Add( mailMessage );
            }

            return mailMessages;
        }

        private IList<MailMessage> ConvertEmailToMailMessages( Email email ) {
            var mailMessages = new List<MailMessage>();

            var mailServerRecipientLimit = EmailSenderConfiguration.MailServerRecipientLimit;

            if ( ( email.To.Count > mailServerRecipientLimit && ( email.CC.Count > mailServerRecipientLimit || email.BCC.Count > mailServerRecipientLimit ) ) ||
                 ( email.CC.Count > mailServerRecipientLimit && email.BCC.Count > mailServerRecipientLimit ) ) {
                throw new InvalidOperationException( string.Format( "The mail server recipient limit is {0} recipients. Only either To,CC or BCC might be larger than this limit!", mailServerRecipientLimit ) );
            }

            if ( email.To.Count > mailServerRecipientLimit ) {
                for ( int skip = 0; skip < email.To.Count; skip += mailServerRecipientLimit ) {
                    var toSubset = email.To.Skip( skip ).Take( skip + mailServerRecipientLimit ).ToList();
                    var emailWithToSubset = new Email( from: email.From, to: toSubset, cc: email.CC, bcc: email.BCC, subject: email.Subject, body: email.Body, attachments: email.Attachments.ToArray() );
                    var mailMessage = ConvertEmailToSingleMailMessage( emailWithToSubset );
                    mailMessages.Add( mailMessage );
                }
            } else if ( email.CC.Count > mailServerRecipientLimit ) {
                for ( int skip = 0; skip < email.CC.Count; skip += mailServerRecipientLimit ) {
                    var ccSubset = email.CC.Skip( skip ).Take( skip + mailServerRecipientLimit ).ToList();
                    var emailWithCCSubset = new Email( from: email.From, to: email.To, cc: ccSubset, bcc: email.BCC, subject: email.Subject, body: email.Body, attachments: email.Attachments.ToArray() );
                    var mailMessage = ConvertEmailToSingleMailMessage( emailWithCCSubset );
                    mailMessages.Add( mailMessage );
                }
            } else if ( email.BCC.Count > mailServerRecipientLimit ) {
                for ( int skip = 0; skip < email.BCC.Count; skip += mailServerRecipientLimit ) {
                    var bccSubset = email.BCC.Skip( skip ).Take( skip + mailServerRecipientLimit ).ToList();
                    var emailWithBCCSubset = new Email( from: email.From, to: email.To, cc: email.CC, bcc: bccSubset, subject: email.Subject, body: email.Body, attachments: email.Attachments.ToArray() );
                    var mailMessage = ConvertEmailToSingleMailMessage( emailWithBCCSubset );
                    mailMessages.Add( mailMessage );
                }
            } else {
                var mailMessage = ConvertEmailToSingleMailMessage( email );
                mailMessages.Add( mailMessage );
            }

            return mailMessages;
        }

        private MailMessage ConvertEmailToSingleMailMessage( Email email ) {
            var mailMessage = new MailMessage( email.From, email.To.ToSeparatedString( separator: "," ), email.Subject, email.Body.Replace( "\r", "" ).Replace( "\n", "<br />" ) );
            mailMessage.Priority = email.MailPriority;
            mailMessage.DeliveryNotificationOptions = email.DeliveryNotificationOptions;
            mailMessage.IsBodyHtml = true;
            if ( email.CC != null && email.CC.Count > 0 ) {
                mailMessage.CC.Add( email.CC.ToSeparatedString( separator: "," ) );
            }
            if ( email.BCC != null && email.BCC.Count > 0 ) {
                mailMessage.Bcc.Add( email.BCC.ToSeparatedString( separator: "," ) );
            }
            if ( email.Attachments != null ) {
                foreach ( var attachment in email.Attachments ) {
                    mailMessage.Attachments.Add( new System.Net.Mail.Attachment( new MemoryStream( attachment.Data ), attachment.FileName, MediaTypeNames.Application.Octet ) );
                }
            }
            if ( !string.IsNullOrEmpty( mailMessage.Subject ) ) {
                while ( ( "=?utf-8?B?" + Convert.ToBase64String( mailMessage.Subject.ToByte( System.Text.Encoding.UTF8 ) ) + "?=" ).Length > 255 ) {
                    // Bugfix for some mailservers/clients which cannot handle UTF8-encoded and to Base64 converted strings which are longer than 255 characters (including control characters)
                    mailMessage.Subject = mailMessage.Subject.RemoveLast( 10 ) + "...";
                }
            }
            return mailMessage;
        }

    }
}
