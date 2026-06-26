using Microsoft.Win32;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class InstallRegistryService
    {
        public const string Mode = "hkcu-uninstall-key";

        public static bool Write(InstallRegistration registration)
        {
            if (registration == null || string.IsNullOrEmpty(registration.RegistryKey))
            {
                return false;
            }

            RegistryKey key = Registry.CurrentUser.CreateSubKey(registration.RegistryKey);
            if (key == null)
            {
                return false;
            }

            try
            {
                key.SetValue("DisplayName", registration.DisplayName ?? string.Empty);
                key.SetValue("Publisher", registration.Publisher ?? string.Empty);
                key.SetValue("InstallLocation", registration.InstallLocation ?? string.Empty);
                key.SetValue("DisplayIcon", registration.DisplayIcon ?? string.Empty);
                key.SetValue("UninstallString", registration.UninstallString ?? string.Empty);
                return true;
            }
            finally
            {
                key.Close();
            }
        }

        public static bool Remove(string registryKey)
        {
            if (string.IsNullOrEmpty(registryKey))
            {
                return false;
            }

            RegistryKey existing = Registry.CurrentUser.OpenSubKey(registryKey);
            if (existing == null)
            {
                return false;
            }

            existing.Close();
            Registry.CurrentUser.DeleteSubKeyTree(registryKey);
            return true;
        }
    }
}
