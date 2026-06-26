using System.Net;
using System.Text;

namespace WeChatPlus.Core.Services
{
    public sealed class HttpLicenseActivationTransport : ILicenseActivationTransport
    {
        public string Send(LicenseActivationRequest request)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                return client.UploadString(request.Url, request.Method, request.BodyJson ?? string.Empty);
            }
        }
    }
}
