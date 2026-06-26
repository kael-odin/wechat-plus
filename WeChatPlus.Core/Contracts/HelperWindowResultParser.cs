using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace WeChatPlus.Core.Contracts
{
    public static class HelperWindowResultParser
    {
        public static HelperWindowInfo[] ParseWindows(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new HelperWindowInfo[0];
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(json);
            if (payload == null || !payload.ContainsKey("data"))
            {
                return new HelperWindowInfo[0];
            }

            Dictionary<string, object> data = payload["data"] as Dictionary<string, object>;
            if (data == null || !data.ContainsKey("windows"))
            {
                return new HelperWindowInfo[0];
            }

            ArrayList windows = data["windows"] as ArrayList;
            if (windows == null)
            {
                return new HelperWindowInfo[0];
            }

            List<HelperWindowInfo> results = new List<HelperWindowInfo>();
            for (int i = 0; i < windows.Count; i++)
            {
                Dictionary<string, object> item = windows[i] as Dictionary<string, object>;
                if (item == null)
                {
                    continue;
                }

                HelperWindowInfo window = new HelperWindowInfo();
                window.ProcessId = ConvertToInt(item, "processId");
                window.WindowHandle = ConvertToString(item, "windowHandle");
                window.Title = ConvertToString(item, "title");
                window.HasWindow = ConvertToBool(item, "hasWindow");
                results.Add(window);
            }

            return results.ToArray();
        }

        private static int ConvertToInt(Dictionary<string, object> item, string key)
        {
            if (!item.ContainsKey(key) || item[key] == null)
            {
                return 0;
            }

            int parsed;
            return int.TryParse(Convert.ToString(item[key]), out parsed) ? parsed : 0;
        }

        private static string ConvertToString(Dictionary<string, object> item, string key)
        {
            if (!item.ContainsKey(key) || item[key] == null)
            {
                return string.Empty;
            }

            return Convert.ToString(item[key]);
        }

        private static bool ConvertToBool(Dictionary<string, object> item, string key)
        {
            if (!item.ContainsKey(key) || item[key] == null)
            {
                return false;
            }

            bool parsed;
            return bool.TryParse(Convert.ToString(item[key]), out parsed) && parsed;
        }
    }
}
