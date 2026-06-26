namespace WeChatPlus.Core.Models
{
    public sealed class UpdateManifestResult
    {
        public bool Loaded { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public UpdateManifest Manifest { get; set; }
    }
}
