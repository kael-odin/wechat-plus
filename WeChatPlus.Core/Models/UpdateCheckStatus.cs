namespace WeChatPlus.Core.Models
{
    public sealed class UpdateCheckStatus
    {
        public bool ProductUpdateAvailable { get; set; }

        public bool HelperUpdateAvailable { get; set; }

        public string StatusText { get; set; }
    }
}
