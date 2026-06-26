namespace WeChatPlus.Core.Models
{
    public sealed class InstallRegistration
    {
        public string RegistryKey { get; set; }

        public string DisplayName { get; set; }

        public string Publisher { get; set; }

        public string InstallLocation { get; set; }

        public string DisplayIcon { get; set; }

        public string UninstallString { get; set; }
    }
}
