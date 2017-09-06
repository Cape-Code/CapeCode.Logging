
namespace CapeCode.Logging.Emails.Interfaces {
    public interface IEmailSender {
        void SendEncryptedEmail( EncryptedEmail encryptedEmail );
        void SendEmail( Email email );
        void SendSystemEmail( SystemEmail systemEmail );
        void SendPublicAccouncementSystemEmail( PublicAnnouncementSystemEmail publicAnnouncementEmail );
    }
}
