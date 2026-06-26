using System;
using System.IO;

namespace WeChatPlus.Core.Services
{
    public static class AppPaths
    {
        public static string GetDefaultDataRoot()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "WeChatPlus");
        }

        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
