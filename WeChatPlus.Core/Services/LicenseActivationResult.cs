using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class LicenseActivationResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public LicenseState State { get; set; }
    }
}
