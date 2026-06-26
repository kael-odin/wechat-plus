using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class InstallPlanner
    {
        public static InstallPlan Create(InstallerManifest manifest, string packageDirectory, string installDirectory, string startMenuDirectory)
        {
            InstallerManifest source = manifest ?? InstallerManifest.CreateDefault(ReleasePackageManifest.CreateDefault());
            InstallPlan plan = new InstallPlan();
            plan.ProductName = source.ProductName ?? string.Empty;
            plan.PackageDirectory = RequirePath(packageDirectory, ".");
            plan.InstallDirectory = RequirePath(installDirectory, source.DefaultInstallDirectory);
            plan.StartMenuDirectory = RequirePath(startMenuDirectory, source.StartMenuFolder);
            plan.ShortcutPath = Path.Combine(plan.StartMenuDirectory, source.ShortcutName ?? string.Empty);
            plan.ShortcutTargetPath = Path.Combine(plan.InstallDirectory, source.ShortcutTarget ?? string.Empty);
            plan.UninstallCommand = Path.Combine(plan.InstallDirectory, source.UninstallCommand ?? string.Empty);
            plan.RegistryKey = source.UninstallRegistryKey ?? string.Empty;
            plan.FileCopies = CreateCopies(source.Files, plan.PackageDirectory, plan.InstallDirectory);
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

        private static InstallFileCopy[] CreateCopies(ReleasePackageFile[] files, string packageDirectory, string installDirectory)
        {
            if (files == null)
            {
                return new InstallFileCopy[0];
            }

            InstallFileCopy[] copies = new InstallFileCopy[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                ReleasePackageFile file = files[i] ?? new ReleasePackageFile();
                InstallFileCopy copy = new InstallFileCopy();
                copy.SourcePath = Path.Combine(packageDirectory ?? string.Empty, file.Path ?? string.Empty);
                copy.DestinationPath = Path.Combine(installDirectory ?? string.Empty, file.Path ?? string.Empty);
                copy.Role = file.Role ?? string.Empty;
                copies[i] = copy;
            }

            return copies;
        }
    }
}
