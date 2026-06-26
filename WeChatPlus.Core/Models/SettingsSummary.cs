namespace WeChatPlus.Core.Models
{
    public sealed class SettingsSummary
    {
        public string DataRoot { get; set; }

        public string RuntimeRoot { get; set; }

        public string HelperPath { get; set; }

        public string UpdateManifestPath { get; set; }

        public string AccountsPath { get; set; }

        public string QuickRepliesPath { get; set; }

        public string LicenseStatePath { get; set; }

        public string PrivacyLockPath { get; set; }

        public string PrivacyNoticePath { get; set; }

        public string ComponentsPath { get; set; }

        public string DiagnosticsPath { get; set; }
    }
}
