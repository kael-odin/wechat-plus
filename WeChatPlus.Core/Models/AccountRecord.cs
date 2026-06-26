using System;

namespace WeChatPlus.Core.Models
{
    public sealed class AccountRecord
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string AvatarPath { get; set; }

        public int ProcessId { get; set; }

        public string WindowHandle { get; set; }

        public string Status { get; set; }

        public int SortOrder { get; set; }

        public DateTime LastActiveAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
