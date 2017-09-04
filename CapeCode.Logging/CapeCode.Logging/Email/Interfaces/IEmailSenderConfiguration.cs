
using System.Collections.Generic;

namespace CapeCode.Logging.Emails.Interfaces {
    public interface IEmailSenderConfiguration {
        string SmtpServerAddress { get; }

        string SmtpServerUsername { get; }
        string SmtpServerPassword { get; }
        bool SmtpServerUsesSSL { get; }
        int MailServerRecipientLimit { get; }
        string SystemMailAddress { get; }
        bool InSimulationMode { get; }
        IList<string> AdministratorEmailAddresses { get; }
        bool IsDisabled { get; }
    }
}
