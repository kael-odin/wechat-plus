using System.Collections.Generic;
using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class ReleasePackageValidator
    {
        public static ReleasePackageValidationResult Validate(string runtimeRoot, ReleasePackageManifest manifest)
        {
            string root = string.IsNullOrWhiteSpace(runtimeRoot) ? string.Empty : runtimeRoot;
            ReleasePackageManifest packageManifest = manifest ?? ReleasePackageManifest.CreateDefault();
            List<ReleasePackageFile> missing = new List<ReleasePackageFile>();

            if (packageManifest.Files != null)
            {
                for (int i = 0; i < packageManifest.Files.Length; i++)
                {
                    ReleasePackageFile file = packageManifest.Files[i];
                    if (file == null || !file.Required)
                    {
                        continue;
                    }

                    string path = Path.Combine(root, file.Path ?? string.Empty);
                    if (!File.Exists(path))
                    {
                        missing.Add(file);
                    }
                }
            }

            ReleasePackageValidationResult result = new ReleasePackageValidationResult();
            result.RuntimeRoot = root;
            result.MissingFiles = missing.ToArray();
            result.IsComplete = result.MissingFiles.Length == 0;
            result.SummaryText = result.IsComplete
                ? "运行包完整，必需文件均存在。"
                : "运行包缺少 " + result.MissingFiles.Length + " 个必需文件。";
            return result;
        }
    }
}
