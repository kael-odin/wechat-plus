using System;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class UpdateCheckService
    {
        public static UpdateCheckStatus Evaluate(UpdateManifest manifest, string currentProductVersion, string currentHelperVersion)
        {
            UpdateCheckStatus status = new UpdateCheckStatus();
            if (manifest == null)
            {
                status.StatusText = "更新清单不可用。";
                return status;
            }

            status.ProductUpdateAvailable = IsNewer(manifest.ProductVersion, currentProductVersion);
            status.HelperUpdateAvailable = IsNewer(manifest.HelperVersion, currentHelperVersion);

            if (!status.ProductUpdateAvailable && !status.HelperUpdateAvailable)
            {
                status.StatusText = "已是最新版本。";
                return status;
            }

            string text = string.Empty;
            if (status.ProductUpdateAvailable)
            {
                text += "主程序 " + manifest.ProductVersion + " 可用。";
            }
            if (status.HelperUpdateAvailable)
            {
                if (text.Length > 0)
                {
                    text += " ";
                }
                text += "助手组件 " + manifest.HelperVersion + " 可用。";
            }
            if (!string.IsNullOrWhiteSpace(manifest.ReleaseNotes))
            {
                text += " 更新说明：" + manifest.ReleaseNotes;
            }

            status.StatusText = text;
            return status;
        }

        private static bool IsNewer(string availableVersion, string currentVersion)
        {
            Version available;
            Version current;
            if (!Version.TryParse(NormalizeVersion(availableVersion), out available))
            {
                return false;
            }
            if (!Version.TryParse(NormalizeVersion(currentVersion), out current))
            {
                return true;
            }

            return available.CompareTo(current) > 0;
        }

        private static string NormalizeVersion(string version)
        {
            return string.IsNullOrWhiteSpace(version) ? "0.0.0" : version.Trim();
        }
    }
}
