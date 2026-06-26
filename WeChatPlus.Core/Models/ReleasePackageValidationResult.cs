namespace WeChatPlus.Core.Models
{
    public sealed class ReleasePackageValidationResult
    {
        public string RuntimeRoot { get; set; }

        public bool IsComplete { get; set; }

        public ReleasePackageFile[] MissingFiles { get; set; }

        public string SummaryText { get; set; }
    }
}
