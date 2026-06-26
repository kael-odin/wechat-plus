namespace WeChatPlus.Core.Services
{
    public sealed class LicenseActivationRequest
    {
        public string Url { get; set; }

        public string Method { get; set; }

        public string BodyJson { get; set; }
    }
}
