using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class TrialLicenseService
    {
        private readonly string _dataRoot;
        private readonly string _licensePath;
        private readonly JavaScriptSerializer _serializer;

        public TrialLicenseService(string dataRoot)
        {
            _dataRoot = dataRoot;
            _licensePath = Path.Combine(_dataRoot, "license_state.json");
            _serializer = new JavaScriptSerializer();
        }

        public LicenseState GetOrCreateTrial()
        {
            AppPaths.EnsureDirectory(_dataRoot);
            if (File.Exists(_licensePath))
            {
                string json = File.ReadAllText(_licensePath);
                LicenseState existing = _serializer.Deserialize<LicenseState>(json);
                if (existing != null)
                {
                    return existing;
                }
            }

            DateTime now = DateTime.UtcNow;
            LicenseState state = new LicenseState
            {
                Id = Guid.NewGuid().ToString("N"),
                LicenseKeyMasked = string.Empty,
                Plan = "trial",
                DeviceIdHash = ComputeDeviceHash(),
                ExpiresAtUtc = now.AddDays(7),
                LastVerifiedAtUtc = now,
                OfflineGraceUntilUtc = now.AddDays(14)
            };

            File.WriteAllText(_licensePath, _serializer.Serialize(state));
            return state;
        }

        private static string ComputeDeviceHash()
        {
            string machine = Environment.MachineName ?? string.Empty;
            string user = Environment.UserName ?? string.Empty;
            string source = machine + "|" + user;
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            using (SHA256 sha = SHA256.Create())
            {
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
