using System;

namespace WeChatPlus.Core.Models
{
    public sealed class InstallerManifest
    {
        public string ProductName { get; set; }

        public string Publisher { get; set; }

        public string DefaultInstallDirectory { get; set; }

        public string StartMenuFolder { get; set; }

        public string ShortcutName { get; set; }

        public string ShortcutTarget { get; set; }

        public string UninstallCommand { get; set; }

        public string OpenHelperSourceUrl { get; set; }

        public ReleasePackageManifest RuntimePackage { get; set; }

        public ReleasePackageFile[] Files { get; set; }

        public static InstallerManifest CreateDefault(ReleasePackageManifest runtimePackage)
        {
            ReleasePackageManifest package = runtimePackage ?? ReleasePackageManifest.CreateDefault();
            InstallerManifest manifest = new InstallerManifest();
            manifest.ProductName = "WeChat Plus";
            manifest.Publisher = "Kael Odin";
            manifest.DefaultInstallDirectory = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\WeChat Plus");
            manifest.StartMenuFolder = "WeChat Plus";
            manifest.ShortcutName = "WeChat Plus.lnk";
            manifest.ShortcutTarget = @"WeChatPlus.Shell.exe";
            manifest.UninstallCommand = @"WeChatPlus.Uninstall.exe";
            manifest.OpenHelperSourceUrl = "https://github.com/huiyadanli/RevokeMsgPatcher";
            manifest.RuntimePackage = package;
            manifest.Files = package.Files ?? new ReleasePackageFile[0];
            return manifest;
        }
    }
}
