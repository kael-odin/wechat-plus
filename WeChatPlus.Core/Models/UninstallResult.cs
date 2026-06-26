namespace WeChatPlus.Core.Models
{
    public sealed class UninstallResult
    {
        public bool Ok { get; set; }

        public int RemovedRuntimeFiles { get; set; }

        public int RemovedShortcuts { get; set; }

        public bool RemovedUserData { get; set; }

        public bool PreservedUserData { get; set; }

        public string[] Errors { get; set; }

        public string SummaryText { get; set; }
    }
}
