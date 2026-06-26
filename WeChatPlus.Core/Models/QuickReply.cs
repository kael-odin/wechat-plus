using System;

namespace WeChatPlus.Core.Models
{
    public sealed class QuickReply
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string CategoryId { get; set; }

        public string Tags { get; set; }

        public int SortOrder { get; set; }

        public bool IsFavorite { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
