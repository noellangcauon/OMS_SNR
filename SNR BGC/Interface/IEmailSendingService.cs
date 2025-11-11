using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SNR_BGC.Interface
{
    public interface IEmailSendingService
    {
        void SendEmailForTempPassword(string toAddress, string toDisplayName, string subject, string userName, string tempPassword);
    }
}
