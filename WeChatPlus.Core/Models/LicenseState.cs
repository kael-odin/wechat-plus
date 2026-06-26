using System;

namespace WeChatPlus.Core.Models
{
    public sealed class LicenseState
    {
        public string Id { get; set; }

        public string LicenseKeyMasked { get; set; }

        public string Plan { get; set; }

        public string DeviceIdHash { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime LastVerifiedAtUtc { get; set; }

        public DateTime OfflineGraceUntilUtc { get; set; }
    }
}
