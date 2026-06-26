using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace WeChatPlus.Core.Contracts
{
    public static class JsonResultWriter
    {
        public static string Serialize(HelperCommandResult result)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload["ok"] = result != null && result.Ok;
            payload["command"] = result == null ? string.Empty : result.Command ?? string.Empty;
            payload["message"] = result == null ? string.Empty : result.Message ?? string.Empty;
            payload["data"] = result == null ? new Dictionary<string, object>() : result.Data;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(payload);
        }
    }
}
