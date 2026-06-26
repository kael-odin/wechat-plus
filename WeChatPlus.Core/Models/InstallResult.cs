namespace WeChatPlus.Core.Models
{
    public sealed class InstallResult
    {
        public bool Ok { get; set; }

        public int CopiedFiles { get; set; }

        public bool CreatedShortcut { get; set; }

        public string[] Errors { get; set; }

        public string SummaryText { get; set; }
    }
}
