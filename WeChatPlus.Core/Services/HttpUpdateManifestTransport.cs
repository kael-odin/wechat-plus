using System.Net;
using System.Text;

namespace WeChatPlus.Core.Services
{
    public sealed class HttpUpdateManifestTransport : IUpdateManifestTransport
    {
        public string DownloadString(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                return client.DownloadString(url);
            }
        }
    }
}
