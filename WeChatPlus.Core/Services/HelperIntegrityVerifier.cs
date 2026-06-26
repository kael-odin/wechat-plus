using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class HelperIntegrityVerifier
    {
        public static HelperIntegrityStatus Verify(string helperPath, UpdateManifest manifest)
        {
            HelperIntegrityStatus status = new HelperIntegrityStatus();
            status.ExpectedSha256 = manifest == null ? string.Empty : CleanHash(manifest.HelperSha256);
            status.HashProvided = !string.IsNullOrWhiteSpace(status.ExpectedSha256);
            status.FileExists = !string.IsNullOrWhiteSpace(helperPath) && File.Exists(helperPath);

            if (!status.FileExists)
            {
                status.StatusText = "助手组件校验：未找到助手组件文件。";
                return status;
            }

            if (!status.HashProvided)
            {
                status.StatusText = "助手组件校验：更新清单未提供 SHA-256，已跳过校验。";
                return status;
            }

            status.ActualSha256 = ComputeSha256(helperPath);
            status.HashMatches = string.Equals(status.ExpectedSha256, status.ActualSha256, StringComparison.OrdinalIgnoreCase);
            status.StatusText = status.HashMatches
                ? "助手组件校验通过。"
                : "助手组件校验失败：SHA-256 不匹配。";
            return status;
        }

        private static string ComputeSha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(stream);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static string CleanHash(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Replace(" ", string.Empty).Trim();
        }
    }
}
