using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class PrivacyLockService
    {
        public const string DefaultPin = "1234";

        private readonly string _dataRoot;
        private readonly string _statePath;
        private readonly JavaScriptSerializer _serializer;

        public PrivacyLockService(string dataRoot)
        {
            _dataRoot = dataRoot;
            _statePath = Path.Combine(_dataRoot, "privacy_lock.json");
            _serializer = new JavaScriptSerializer();
        }

        public PrivacyLockState GetOrCreate()
        {
            AppPaths.EnsureDirectory(_dataRoot);
            if (File.Exists(_statePath))
            {
                PrivacyLockState existing = _serializer.Deserialize<PrivacyLockState>(File.ReadAllText(_statePath));
                if (existing != null && !string.IsNullOrWhiteSpace(existing.PinHash))
                {
                    return existing;
                }
            }

            PrivacyLockState state = new PrivacyLockState();
            state.IsLocked = false;
            state.PinHash = HashPin(DefaultPin);
            state.UpdatedAtUtc = DateTime.UtcNow;
            Save(state);
            return state;
        }

        public PrivacyLockState Lock()
        {
            PrivacyLockState state = GetOrCreate();
            state.IsLocked = true;
            state.UpdatedAtUtc = DateTime.UtcNow;
            Save(state);
            return state;
        }

        public bool TryUnlock(string pin)
        {
            PrivacyLockState state = GetOrCreate();
            if (!string.Equals(state.PinHash, HashPin(pin), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            state.IsLocked = false;
            state.UpdatedAtUtc = DateTime.UtcNow;
            Save(state);
            return true;
        }

        public bool ChangePin(string currentPin, string newPin)
        {
            if (string.IsNullOrWhiteSpace(newPin))
            {
                return false;
            }

            PrivacyLockState state = GetOrCreate();
            if (!string.Equals(state.PinHash, HashPin(currentPin), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            state.PinHash = HashPin(newPin);
            state.UpdatedAtUtc = DateTime.UtcNow;
            Save(state);
            return true;
        }

        public bool IsUsingDefaultPin()
        {
            PrivacyLockState state = GetOrCreate();
            return string.Equals(state.PinHash, HashPin(DefaultPin), StringComparison.OrdinalIgnoreCase);
        }

        private void Save(PrivacyLockState state)
        {
            AppPaths.EnsureDirectory(_dataRoot);
            File.WriteAllText(_statePath, _serializer.Serialize(state));
        }

        private static string HashPin(string pin)
        {
            string normalized = string.IsNullOrEmpty(pin) ? string.Empty : pin.Trim();
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes("wechat-plus-privacy-lock|" + normalized);
                byte[] hash = sha.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
