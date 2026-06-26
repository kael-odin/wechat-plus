namespace WeChatPlus.Core.Models
{
    public sealed class HelperIntegrityStatus
    {
        public bool FileExists { get; set; }

        public bool HashProvided { get; set; }

        public bool HashMatches { get; set; }

        public string ExpectedSha256 { get; set; }

        public string ActualSha256 { get; set; }

        public string StatusText { get; set; }
    }
}
