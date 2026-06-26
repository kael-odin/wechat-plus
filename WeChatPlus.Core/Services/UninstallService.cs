using System;
using System.Collections;
using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class UninstallService
    {
        public static UninstallResult Execute(UninstallPlan plan, bool removeUserData)
        {
            ArrayList errors = new ArrayList();
            int runtimeFiles = DeleteFiles(plan == null ? null : plan.RuntimeFilePaths, errors);
            int shortcuts = DeleteFiles(plan == null ? null : plan.ShortcutPaths, errors);
            bool removedUserData = false;

            if (plan != null && removeUserData && Directory.Exists(plan.UserDataDirectory))
            {
                try
                {
                    Directory.Delete(plan.UserDataDirectory, true);
                    removedUserData = true;
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            }

            RemoveDirectoryIfEmpty(plan == null ? string.Empty : plan.StartMenuDirectory, errors);
            RemoveDirectoryIfEmpty(plan == null ? string.Empty : plan.InstallDirectory, errors);

            UninstallResult result = new UninstallResult();
            result.Ok = errors.Count == 0;
            result.RemovedRuntimeFiles = runtimeFiles;
            result.RemovedShortcuts = shortcuts;
            result.RemovedUserData = removedUserData;
            result.PreservedUserData = plan != null && plan.PreserveUserData && !removeUserData;
            result.Errors = ToStringArray(errors);
            result.SummaryText = BuildSummary(result);
            return result;
        }

        private static int DeleteFiles(string[] paths, ArrayList errors)
        {
            if (paths == null)
            {
                return 0;
            }

            int removed = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i] ?? string.Empty;
                if (path.Length == 0 || !File.Exists(path))
                {
                    continue;
                }

                try
                {
                    File.Delete(path);
                    removed++;
                }
                catch (Exception ex)
                {
                    errors.Add(path + ": " + ex.Message);
                }
            }

            return removed;
        }

        private static void RemoveDirectoryIfEmpty(string path, ArrayList errors)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return;
            }

            try
            {
                if (Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0)
                {
                    Directory.Delete(path, false);
                }
            }
            catch (Exception ex)
            {
                errors.Add(path + ": " + ex.Message);
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

        private static string BuildSummary(UninstallResult result)
        {
            string dataText = result.RemovedUserData ? "user data removed" : "user data preserved";
            return "removed " + result.RemovedRuntimeFiles + " runtime files and " + result.RemovedShortcuts + " shortcuts; " + dataText;
        }
    }
}
