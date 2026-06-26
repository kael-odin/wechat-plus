using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class SettingsSummaryService
    {
        public static SettingsSummary Create(string dataRoot, string runtimeRoot)
        {
            string data = string.IsNullOrWhiteSpace(dataRoot) ? AppPaths.GetDefaultDataRoot() : dataRoot;
            string runtime = string.IsNullOrWhiteSpace(runtimeRoot) ? string.Empty : runtimeRoot;

            SettingsSummary summary = new SettingsSummary();
            summary.DataRoot = data;
            summary.RuntimeRoot = runtime;
            summary.HelperPath = Path.Combine(runtime, "WeChatPlus.OpenHelper.exe");
            summary.UpdateManifestPath = Path.Combine(runtime, "update-manifest.json");
            summary.AccountsPath = Path.Combine(data, "accounts.json");
            summary.QuickRepliesPath = Path.Combine(data, "quick_replies.json");
            summary.LicenseStatePath = Path.Combine(data, "license_state.json");
            summary.PrivacyLockPath = Path.Combine(data, "privacy_lock.json");
            summary.ComponentsPath = Path.Combine(data, "components.json");
            summary.DiagnosticsPath = Path.Combine(data, "diagnostics.log");
            return summary;
        }
    }
}
