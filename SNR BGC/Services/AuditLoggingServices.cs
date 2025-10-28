using Microsoft.Extensions.Configuration;
using Polly;
using SNR_BGC.Interface;
using SNR_BGC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SNR_BGC.Services
{
    public class AuditLoggingServices : IAuditLoggingServices
    {
        private readonly IConfiguration _configuration;
        private readonly UserClass _userInfoConn;

        public AuditLoggingServices(IConfiguration configuration, UserClass userInfoConn)
        {
            _configuration = configuration;
            _userInfoConn = userInfoConn;
        }

        public Dictionary<string, (string, string)> GetChangedFields<T>(T oldObj, T newObj)
        {
            var changes = new Dictionary<string, (string, string)>();
            var ignoreProps = new[] { "lastEditDate", "lastEditUser", "lastEditColumn", "lastEditValue", "failedAttempts", "newUser" };

            var props = typeof(T).GetProperties()
                .Where(p => !ignoreProps.Contains(p.Name));

            foreach (var prop in props)
            {
                var oldValue = oldObj == null ? null : prop.GetValue(oldObj);
                var newValue = prop.GetValue(newObj);

                string oldVal, newVal;

                if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                {
                    oldVal = oldValue == null ? null : ((bool)oldValue ? "Yes" : "No");
                    newVal = newValue == null ? null : ((bool)newValue ? "Yes" : "No");
                }
                else
                {
                    oldVal = oldValue?.ToString();
                    newVal = newValue?.ToString();
                }

                if (oldVal != newVal)
                {
                    changes.Add(prop.Name, (oldVal, newVal));
                }
            }

            return changes;
        }

        public async Task LogChanges(int userId, int performedById, string module, string action, Dictionary<string, (string OldValue, string NewValue)> changes)
        {
            foreach (var change in changes)
            {
                var auditLog = new AuditLogs
                {
                    PerformedById = performedById,
                    UserId = userId,
                    Module = module,
                    Action = action,
                    Field = change.Key,
                    OldValue = change.Value.OldValue,
                    NewValue = change.Value.NewValue,
                    DateCreated = DateTime.Now
                };

                await _userInfoConn.AuditLogs.AddAsync(auditLog);
            }
        }
    }
}
