using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace WeChatPlus.Core.Contracts
{
    public static class HelperRuntimeStatusParser
    {
        public static HelperRuntimeStatus Parse(string json)
        {
            HelperRuntimeStatus status = new HelperRuntimeStatus();
            status.Command = string.Empty;
            status.Message = string.Empty;
            status.InstallPath = string.Empty;

            if (string.IsNullOrWhiteSpace(json))
            {
                return status;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(json);
            if (payload == null)
            {
                return status;
            }

            status.HelperOk = ConvertToBool(payload, "ok");
            status.Command = ConvertToString(payload, "command");
            status.Message = ConvertToString(payload, "message");

            Dictionary<string, object> data = payload.ContainsKey("data")
                ? payload["data"] as Dictionary<string, object>
                : null;
            if (data != null)
            {
                status.ProcessCount = ConvertToInt(data, "processCount");
                status.InstallPath = ConvertToString(data, "installPath");
            }

            return status;
        }

        private static string ConvertToString(Dictionary<string, object> item, string key)
        {
            if (!item.ContainsKey(key) || item[key] == null)
            {
                return string.Empty;
            }

            return Convert.ToString(item[key]);
        }

        private static int ConvertToInt(Dictionary<string, object> item, string key)
        {
            int parsed;
            return int.TryParse(ConvertToString(item, key), out parsed) ? parsed : 0;
        }

        private static bool ConvertToBool(Dictionary<string, object> item, string key)
        {
            bool parsed;
            return bool.TryParse(ConvertToString(item, key), out parsed) && parsed;
        }
    }
}
