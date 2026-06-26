using System;
using System.IO;
using System.Text;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class DiagnosticsPackageService
    {
        public static DiagnosticsPackageResult Create(
            string dataRoot,
            string runtimeRoot,
            string packageDirectory,
            ReleasePackageManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(packageDirectory))
            {
                throw new ArgumentException("Missing diagnostics package directory.", "packageDirectory");
            }

            AppPaths.EnsureDirectory(packageDirectory);

            SettingsSummary settings = SettingsSummaryService.Create(dataRoot, runtimeRoot);
            ReleasePackageValidationResult package = ReleasePackageValidator.Validate(runtimeRoot, manifest);
            LicenseState license = new TrialLicenseService(dataRoot).GetOrCreateTrial();

            string reportPath = Path.Combine(packageDirectory, "support-report.txt");
            string logPath = Path.Combine(packageDirectory, "diagnostics.log");
            string sourceLogPath = settings.DiagnosticsPath;
            if (!File.Exists(sourceLogPath))
            {
                File.WriteAllText(sourceLogPath, string.Empty);
            }

            File.Copy(sourceLogPath, logPath, true);
            File.WriteAllText(reportPath, BuildReport(settings, package, license));

            return new DiagnosticsPackageResult
            {
                Created = true,
                PackageDirectory = packageDirectory,
                ReportPath = reportPath,
                LogPath = logPath
            };
        }

        private static string BuildReport(
            SettingsSummary settings,
            ReleasePackageValidationResult package,
            LicenseState license)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("WeChat Plus Support Report");
            builder.AppendLine("GeneratedAtUtc: " + DateTime.UtcNow.ToString("o"));
            builder.AppendLine();
            builder.AppendLine("[Runtime]");
            builder.AppendLine("DataRoot: " + Safe(settings.DataRoot));
            builder.AppendLine("RuntimeRoot: " + Safe(settings.RuntimeRoot));
            builder.AppendLine("HelperPath: " + Safe(settings.HelperPath));
            builder.AppendLine("UpdateManifestPath: " + Safe(settings.UpdateManifestPath));
            builder.AppendLine("DiagnosticsPath: diagnostics.log");
            builder.AppendLine();
            builder.AppendLine("[Package]");
            builder.AppendLine("Summary: " + Safe(package.SummaryText));
            builder.AppendLine("MissingFiles: " + (package.MissingFiles == null ? 0 : package.MissingFiles.Length));
            if (package.MissingFiles != null)
            {
                for (int i = 0; i < package.MissingFiles.Length; i++)
                {
                    builder.AppendLine("- " + Safe(package.MissingFiles[i].Path) + " (" + Safe(package.MissingFiles[i].Role) + ")");
                }
            }

            builder.AppendLine();
            builder.AppendLine("[DataFiles]");
            builder.AppendLine("AccountsExists: " + File.Exists(settings.AccountsPath));
            builder.AppendLine("QuickRepliesExists: " + File.Exists(settings.QuickRepliesPath));
            builder.AppendLine("LicenseStateExists: " + File.Exists(settings.LicenseStatePath));
            builder.AppendLine("PrivacyLockExists: " + File.Exists(settings.PrivacyLockPath));
            builder.AppendLine("ComponentsExists: " + File.Exists(settings.ComponentsPath));
            builder.AppendLine();
            builder.AppendLine("[License]");
            builder.AppendLine("Plan: " + Safe(license.Plan));
            builder.AppendLine("LicenseKeyMasked: " + Safe(license.LicenseKeyMasked));
            builder.AppendLine("ExpiresAtUtc: " + license.ExpiresAtUtc.ToString("o"));
            builder.AppendLine("OfflineGraceUntilUtc: " + license.OfflineGraceUntilUtc.ToString("o"));
            return builder.ToString();
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
