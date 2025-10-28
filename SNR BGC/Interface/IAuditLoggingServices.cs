using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SNR_BGC.Interface
{
    public interface IAuditLoggingServices
    {
        Dictionary<string, (string, string)> GetChangedFields<T>(T oldObj, T newObj);
        Task LogChanges(int userId, int performedById, string module, string action, Dictionary<string, (string OldValue, string NewValue)> changes);
    }
}
