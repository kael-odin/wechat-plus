using System;

namespace WeChatPlus.Core.Models
{
    public sealed class PrivacyLockState
    {
        public bool IsLocked { get; set; }

        public string PinHash { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
