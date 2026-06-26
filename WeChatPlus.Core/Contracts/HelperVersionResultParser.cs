using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace WeChatPlus.Core.Contracts
{
    public static class HelperVersionResultParser
    {
        public static string ParseVersion(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(json);
            if (payload == null || !payload.ContainsKey("data"))
            {
                return string.Empty;
            }

            Dictionary<string, object> data = payload["data"] as Dictionary<string, object>;
            if (data == null || !data.ContainsKey("version") || data["version"] == null)
            {
                return string.Empty;
            }

            return Convert.ToString(data["version"]);
        }
    }
}
