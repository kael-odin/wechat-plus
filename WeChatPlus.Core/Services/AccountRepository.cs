using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class AccountRepository
    {
        private readonly string _accountsPath;
        private readonly JavaScriptSerializer _serializer;

        public AccountRepository(string dataRoot)
        {
            AppPaths.EnsureDirectory(dataRoot);
            _accountsPath = Path.Combine(dataRoot, "accounts.json");
            _serializer = new JavaScriptSerializer();
        }

        public AccountRecord[] GetAll()
        {
            if (!File.Exists(_accountsPath))
            {
                return new AccountRecord[0];
            }

            string json = File.ReadAllText(_accountsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new AccountRecord[0];
            }

            AccountRecord[] accounts = _serializer.Deserialize<AccountRecord[]>(json);
            return accounts == null
                ? new AccountRecord[0]
                : accounts.OrderBy(x => x.SortOrder).ThenBy(x => x.DisplayName).ToArray();
        }

        public AccountRecord UpsertFromProcess(int processId, string displayName, string status)
        {
            List<AccountRecord> accounts = new List<AccountRecord>(GetAll());
            AccountRecord record = accounts.FirstOrDefault(x => x.ProcessId == processId && processId > 0);
            DateTime now = DateTime.UtcNow;

            if (record == null)
            {
                record = new AccountRecord
                {
                    Id = Guid.NewGuid().ToString("N"),
                    CreatedAtUtc = now,
                    SortOrder = accounts.Count + 1
                };
                accounts.Add(record);
            }

            if (string.IsNullOrWhiteSpace(record.DisplayName) || IsGeneratedDisplayName(record.DisplayName))
            {
                record.DisplayName = string.IsNullOrWhiteSpace(displayName) ? "微信账号" : displayName;
            }
            record.ProcessId = processId;
            record.Status = string.IsNullOrWhiteSpace(status) ? "Unknown" : status;
            record.LastActiveAtUtc = now;
            record.UpdatedAtUtc = now;

            Save(accounts.ToArray());
            return record;
        }

        public void UpdateStatus(string id, string status, int processId, string windowHandle)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            List<AccountRecord> accounts = new List<AccountRecord>(GetAll());
            AccountRecord record = accounts.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (record == null)
            {
                return;
            }

            record.Status = string.IsNullOrWhiteSpace(status) ? record.Status : status;
            record.ProcessId = processId;
            record.WindowHandle = windowHandle ?? string.Empty;
            record.UpdatedAtUtc = DateTime.UtcNow;
            Save(accounts.ToArray());
        }

        public bool UpdateDisplayName(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(displayName))
            {
                return false;
            }

            List<AccountRecord> accounts = new List<AccountRecord>(GetAll());
            AccountRecord record = accounts.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (record == null)
            {
                return false;
            }

            record.DisplayName = displayName.Trim();
            record.UpdatedAtUtc = DateTime.UtcNow;
            Save(accounts.ToArray());
            return true;
        }

        public void Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            AccountRecord[] remaining = GetAll()
                .Where(x => !string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Save(remaining);
        }

        private void Save(AccountRecord[] accounts)
        {
            File.WriteAllText(_accountsPath, _serializer.Serialize(accounts ?? new AccountRecord[0]));
        }

        private static bool IsGeneratedDisplayName(string displayName)
        {
            return string.Equals(displayName, "微信账号", StringComparison.OrdinalIgnoreCase)
                || displayName.StartsWith("微信窗口 ", StringComparison.OrdinalIgnoreCase)
                || displayName.StartsWith("微信实例 ", StringComparison.OrdinalIgnoreCase);
        }
    }
}
