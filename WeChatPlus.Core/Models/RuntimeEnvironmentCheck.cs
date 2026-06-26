namespace WeChatPlus.Core.Models
{
    public sealed class RuntimeEnvironmentCheck
    {
        public bool IsAdministrator { get; set; }

        public bool HelperAvailable { get; set; }

        public bool WeChatInstallPathFound { get; set; }

        public int WeChatProcessCount { get; set; }

        public string WeChatInstallPath { get; set; }

        public bool IsReady { get; set; }

        public string SummaryText { get; set; }
    }
}
