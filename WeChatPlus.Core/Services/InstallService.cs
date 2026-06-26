using System;
using System.Collections;
using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class InstallService
    {
        public static InstallResult Execute(InstallPlan plan)
        {
            ArrayList errors = new ArrayList();
            int copied = CopyFiles(plan == null ? null : plan.FileCopies, errors);
            bool shortcut = CreateShortcutPlaceholder(plan, errors);

            InstallResult result = new InstallResult();
            result.Ok = errors.Count == 0;
            result.CopiedFiles = copied;
            result.CreatedShortcut = shortcut;
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

        private static bool CreateShortcutPlaceholder(InstallPlan plan, ArrayList errors)
        {
            if (plan == null || string.IsNullOrEmpty(plan.ShortcutPath))
            {
                return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(plan.ShortcutPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(plan.ShortcutPath, plan.ShortcutTargetPath ?? string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                errors.Add(plan.ShortcutPath + ": " + ex.Message);
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
            return "installed " + result.CopiedFiles + " files; shortcut " + (result.CreatedShortcut ? "created" : "not created");
        }
    }
}
