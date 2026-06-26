using System;
using System.Collections;
using System.IO;
using System.Web.Script.Serialization;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class InstallService
    {
        public static InstallResult Execute(InstallPlan plan)
        {
            ArrayList errors = new ArrayList();
            int copied = CopyFiles(plan == null ? null : plan.FileCopies, errors);
            string shortcutPath;
            string shortcutMode;
            bool shortcut = CreateShortcut(plan, errors, out shortcutPath, out shortcutMode);
            string registrationPath;
            bool registration = WriteRegistration(plan, errors, out registrationPath);

            InstallResult result = new InstallResult();
            result.Ok = errors.Count == 0;
            result.CopiedFiles = copied;
            result.CreatedShortcut = shortcut;
            result.ShortcutPath = shortcutPath;
            result.ShortcutMode = shortcutMode;
            result.WroteRegistration = registration;
            result.RegistrationPath = registrationPath;
            result.Errors = ToStringArray(errors);
            result.SummaryText = BuildSummary(result);
            return result;
        }

        private static int CopyFiles(InstallFileCopy[] copies, ArrayList errors)
        {
            if (copies == null)
            {
                return 0;
            }

            int copied = 0;
            for (int i = 0; i < copies.Length; i++)
            {
                InstallFileCopy copy = copies[i];
                if (copy == null)
                {
                    continue;
                }

                try
                {
                    string directory = Path.GetDirectoryName(copy.DestinationPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Copy(copy.SourcePath, copy.DestinationPath, true);
                    copied++;
                }
                catch (Exception ex)
                {
                    errors.Add((copy.SourcePath ?? string.Empty) + ": " + ex.Message);
                }
            }

            return copied;
        }

        private static bool CreateShortcut(InstallPlan plan, ArrayList errors, out string shortcutPath, out string shortcutMode)
        {
            shortcutPath = string.Empty;
            shortcutMode = "not-created";
            if (plan == null || string.IsNullOrEmpty(plan.ShortcutPath))
            {
                return false;
            }

            shortcutPath = plan.ShortcutPath;
            try
            {
                string directory = Path.GetDirectoryName(plan.ShortcutPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (TryCreateWindowsShellLink(plan))
                {
                    shortcutMode = "windows-shell-link";
                    return true;
                }

                File.WriteAllText(plan.ShortcutPath, plan.ShortcutTargetPath ?? string.Empty);
                shortcutMode = "fallback-target-file";
                return true;
            }
            catch (Exception ex)
            {
                errors.Add(plan.ShortcutPath + ": " + ex.Message);
                return false;
            }
        }

        private static bool TryCreateWindowsShellLink(InstallPlan plan)
        {
            object shell = null;
            object shortcut = null;

            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    return false;
                }

                shell = Activator.CreateInstance(shellType);
                shortcut = shellType.InvokeMember(
                    "CreateShortcut",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    shell,
                    new object[] { plan.ShortcutPath });

                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { plan.ShortcutTargetPath ?? string.Empty });
                shortcutType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { plan.InstallDirectory ?? string.Empty });
                shortcutType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { plan.ProductName ?? string.Empty });
                shortcutType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, new object[0]);
                return File.Exists(plan.ShortcutPath);
            }
            catch
            {
                return false;
            }
            finally
            {
                ReleaseComObject(shortcut);
                ReleaseComObject(shell);
            }
        }

        private static void ReleaseComObject(object value)
        {
            if (value != null && System.Runtime.InteropServices.Marshal.IsComObject(value))
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(value);
            }
        }

        private static bool WriteRegistration(InstallPlan plan, ArrayList errors, out string registrationPath)
        {
            registrationPath = string.Empty;
            if (plan == null || plan.Registration == null || string.IsNullOrEmpty(plan.InstallDirectory))
            {
                return false;
            }

            try
            {
                if (!Directory.Exists(plan.InstallDirectory))
                {
                    Directory.CreateDirectory(plan.InstallDirectory);
                }

                registrationPath = Path.Combine(plan.InstallDirectory, "install-registration.json");
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                File.WriteAllText(registrationPath, serializer.Serialize(plan.Registration));
                return true;
            }
            catch (Exception ex)
            {
                errors.Add("install-registration.json: " + ex.Message);
                return false;
            }
        }

        private static string[] ToStringArray(ArrayList values)
        {
            string[] result = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                result[i] = values[i] == null ? string.Empty : values[i].ToString();
            }

            return result;
        }

        private static string BuildSummary(InstallResult result)
        {
            return "installed " + result.CopiedFiles + " files; shortcut " + (result.CreatedShortcut ? result.ShortcutMode : "not created") + "; registration " + (result.WroteRegistration ? "written" : "not written");
        }
    }
}
