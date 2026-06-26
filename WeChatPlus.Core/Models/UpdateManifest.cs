using System.Web.Script.Serialization;

namespace WeChatPlus.Core.Models
{
    public sealed class UpdateManifest
    {
        public string ProductVersion { get; set; }

        public string HelperVersion { get; set; }

        public string DownloadUrl { get; set; }

        public string HelperSha256 { get; set; }

        public string ReleaseNotes { get; set; }

        public static UpdateManifest Parse(string json)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            UpdateManifest manifest = serializer.Deserialize<UpdateManifest>(json);
            return manifest == null ? new UpdateManifest() : manifest;
        }
    }
}
