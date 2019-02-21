using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CapeCode.Logging.Emails.Interfaces {
    [DataContract( IsReference = true )]
    public class SystemEmail : Email {
        public SystemEmail( IList<string> to, IList<string> cc, IList<string> bcc, string subject, string body, params Attachment[] attachments )
            : base( from: "$SYSTEMADDRESS", to: to, cc: cc, bcc: bcc, subject: subject, body: body, attachments: attachments ) {

        }

        public SystemEmail( IList<string> to, IList<string> cc, string subject, string body, params Attachment[] attachments )
            : base( from: "$SYSTEMADDRESS", to: to, cc: cc, bcc: null, subject: subject, body: body, attachments: attachments ) {

        }

        public SystemEmail( IList<string> to, string subject, string body, params Attachment[] attachments )
            : base( from: "$SYSTEMADDRESS", to: to, cc: null, bcc: null, subject: subject, body: body, attachments: attachments ) {

        }
    }
}
