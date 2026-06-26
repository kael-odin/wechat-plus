namespace WeChatPlus.Core.Models
{
    public sealed class InstallPlan
    {
        public string ProductName { get; set; }

        public string PackageDirectory { get; set; }

        public string InstallDirectory { get; set; }

        public string StartMenuDirectory { get; set; }

        public string ShortcutPath { get; set; }

        public string ShortcutTargetPath { get; set; }

        public string UninstallCommand { get; set; }

        public string RegistryKey { get; set; }

        public InstallRegistration Registration { get; set; }

        public InstallFileCopy[] FileCopies { get; set; }
    }
}
