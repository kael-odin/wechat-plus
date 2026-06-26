using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class LicenseApiClient
    {
        private readonly string _baseUrl;

        public LicenseApiClient(string baseUrl)
        {
            _baseUrl = (baseUrl ?? string.Empty).TrimEnd('/');
        }

        public LicenseActivationRequest BuildActivationRequest(string licenseKey, LicenseState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body["product"] = "wechat-plus";
            body["licenseKey"] = licenseKey ?? string.Empty;
            body["deviceIdHash"] = state.DeviceIdHash ?? string.Empty;
            body["currentPlan"] = state.Plan ?? string.Empty;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return new LicenseActivationRequest
            {
                Url = _baseUrl + "/licenses/activate",
                Method = "POST",
                BodyJson = serializer.Serialize(body)
            };
        }
    }
}
