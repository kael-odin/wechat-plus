namespace WeChatPlus.Core.Models
{
    public sealed class UninstallPlan
    {
        public string ProductName { get; set; }

        public string InstallDirectory { get; set; }

        public string StartMenuDirectory { get; set; }

        public string UserDataDirectory { get; set; }

        public bool PreserveUserData { get; set; }

        public string RegistryKey { get; set; }

        public string[] RuntimeFilePaths { get; set; }

        public string[] ShortcutPaths { get; set; }
    }
}
