using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;

namespace WeChatPlus.Core.Models
{
    public sealed class LicenseActivationResponse
    {
        public bool Ok { get; set; }

        public string Message { get; set; }

        public string LicenseKeyMasked { get; set; }

        public string Plan { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime OfflineGraceUntilUtc { get; set; }

        public static LicenseActivationResponse Parse(string json)
        {
            LicenseActivationResponse response = new LicenseActivationResponse();
            response.Message = string.Empty;
            response.LicenseKeyMasked = string.Empty;
            response.Plan = string.Empty;

            if (string.IsNullOrWhiteSpace(json))
            {
                return response;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> payload = serializer.Deserialize<Dictionary<string, object>>(json);
            if (payload == null)
            {
                return response;
            }

            response.Ok = ConvertToBool(payload, "ok");
            response.Message = ConvertToString(payload, "message");
            response.LicenseKeyMasked = ConvertToString(payload, "licenseKeyMasked");
            response.Plan = ConvertToString(payload, "plan");
            response.ExpiresAtUtc = ConvertToUtc(payload, "expiresAtUtc");
            response.OfflineGraceUntilUtc = ConvertToUtc(payload, "offlineGraceUntilUtc");
            return response;
        }

        private static string ConvertToString(Dictionary<string, object> payload, string key)
        {
            if (!payload.ContainsKey(key) || payload[key] == null)
            {
                return string.Empty;
            }

            return Convert.ToString(payload[key], CultureInfo.InvariantCulture);
        }

        private static bool ConvertToBool(Dictionary<string, object> payload, string key)
        {
            bool parsed;
            return bool.TryParse(ConvertToString(payload, key), out parsed) && parsed;
        }

        private static DateTime ConvertToUtc(Dictionary<string, object> payload, string key)
        {
            DateTime parsed;
            if (!DateTime.TryParse(
                ConvertToString(payload, key),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out parsed))
            {
                return DateTime.MinValue;
            }

            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }
    }
}
