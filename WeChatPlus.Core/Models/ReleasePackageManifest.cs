namespace WeChatPlus.Core.Models
{
    public sealed class ReleasePackageManifest
    {
        public string Name { get; set; }

        public ReleasePackageFile[] Files { get; set; }

        public static ReleasePackageManifest CreateDefault()
        {
            ReleasePackageManifest manifest = new ReleasePackageManifest();
            manifest.Name = "WeChat Plus MVP runtime package";
            manifest.Files = new[]
            {
                File("WeChatPlus.Shell.exe", "ClosedSourceShell"),
                File("WeChatPlus.Core.dll", "NeutralCore"),
                File("WeChatPlus.OpenHelper.exe", "OpenHelper"),
                File("LICENSE", "OpenSourceLicense"),
                File("README.md", "RuntimeGuide"),
                File("components.json", "OpenSourceNotice"),
                File("update-manifest.json", "UpdateManifest")
            };
            return manifest;
        }

        private static ReleasePackageFile File(string path, string role)
        {
            ReleasePackageFile file = new ReleasePackageFile();
            file.Path = path;
            file.Role = role;
            file.Required = true;
            return file;
        }
    }
}
