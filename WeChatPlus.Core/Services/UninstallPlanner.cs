using System;
using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class UninstallPlanner
    {
        public static UninstallPlan Create(InstallerManifest manifest, string installDirectory, string startMenuDirectory, string dataRoot)
        {
            InstallerManifest source = manifest ?? InstallerManifest.CreateDefault(ReleasePackageManifest.CreateDefault());
            UninstallPlan plan = new UninstallPlan();
            plan.ProductName = source.ProductName ?? string.Empty;
            plan.InstallDirectory = RequirePath(installDirectory, source.DefaultInstallDirectory);
            plan.StartMenuDirectory = RequirePath(startMenuDirectory, source.StartMenuFolder);
            plan.UserDataDirectory = RequirePath(dataRoot, AppPaths.GetDefaultDataRoot());
            plan.PreserveUserData = source.PreserveUserDataOnUninstall;
            plan.RegistryKey = source.UninstallRegistryKey ?? string.Empty;
            plan.RuntimeFilePaths = CombinePaths(plan.InstallDirectory, source.RuntimeFilesToRemove);
            plan.ShortcutPaths = CombinePaths(plan.StartMenuDirectory, source.ShortcutsToRemove);
            return plan;
        }

        private static string RequirePath(string preferred, string fallback)
        {
            if (!string.IsNullOrEmpty(preferred))
            {
                return preferred;
            }

            return fallback ?? string.Empty;
        }

        private static string[] CombinePaths(string root, string[] relativePaths)
        {
            if (relativePaths == null)
            {
                return new string[0];
            }

            string[] paths = new string[relativePaths.Length];
            for (int i = 0; i < relativePaths.Length; i++)
            {
                string relativePath = relativePaths[i] ?? string.Empty;
                paths[i] = Path.Combine(root ?? string.Empty, relativePath);
            }

            return paths;
        }
    }
}
