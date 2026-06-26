namespace WeChatPlus.Core.Models
{
    public sealed class InstallResult
    {
        public bool Ok { get; set; }

        public int CopiedFiles { get; set; }

        public bool CreatedShortcut { get; set; }

        public string ShortcutPath { get; set; }

        public string ShortcutMode { get; set; }

        public bool WroteRegistration { get; set; }

        public string RegistrationPath { get; set; }

        public bool WroteRegistry { get; set; }

        public string RegistryMode { get; set; }

        public string[] Errors { get; set; }

        public string SummaryText { get; set; }
    }
}
