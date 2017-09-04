using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CapeCode.Logging.Emails.Interfaces {
    [DataContract( IsReference = true )]
    public class PublicAnnouncementSystemEmail : Email {
        public PublicAnnouncementSystemEmail( IList<string> cc, IList<string> bcc, string subject, string body, params Attachment[] attachments )
            : base( from: "$SYSTEMADDRESS", to: new List<string>() { "$SYSTEMADDRESS" }, cc: cc, bcc: bcc, subject: subject, body: body, attachments: attachments ) {

        }
    }
}
