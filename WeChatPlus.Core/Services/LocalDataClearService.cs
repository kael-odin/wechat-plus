using System;
using System.Collections;
using System.IO;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class LocalDataClearService
    {
        public static LocalDataClearResult Clear(string dataRoot)
        {
            ArrayList errors = new ArrayList();
            int removed = 0;
            string root = string.IsNullOrWhiteSpace(dataRoot) ? AppPaths.GetDefaultDataRoot() : dataRoot;

            if (Directory.Exists(root))
            {
                string[] files = Directory.GetFiles(root);
                for (int i = 0; i < files.Length; i++)
                {
                    if (DeleteFile(files[i], errors))
                    {
                        removed++;
                    }
                }

                string[] directories = Directory.GetDirectories(root);
                for (int i = 0; i < directories.Length; i++)
                {
                    if (DeleteDirectory(directories[i], errors))
                    {
                        removed++;
                    }
                }
            }
            else
            {
                AppPaths.EnsureDirectory(root);
            }

            LocalDataClearResult result = new LocalDataClearResult();
            result.Ok = errors.Count == 0;
            result.RemovedEntries = removed;
            result.Errors = ToStringArray(errors);
            result.SummaryText = BuildSummary(result);
            return result;
        }

        public static string GetPrivacyNoticeText()
        {
            return "WeChat Plus 默认不读取微信聊天数据库，不采集联系人或会话内容，不上传微信账号、联系人或聊天内容。授权流程只使用设备授权所需的最小信息。本地账号备注、快捷话术、授权状态、隐私锁状态、开源组件声明和诊断日志保存在本机数据目录；用户可在设置页清理这些本地数据。";
        }

        private static bool DeleteFile(string path, ArrayList errors)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                errors.Add(path + ": " + ex.Message);
                return false;
            }
        }

        private static bool DeleteDirectory(string path, ArrayList errors)
        {
            try
            {
                Directory.Delete(path, true);
                return true;
            }
            catch (Exception ex)
            {
                errors.Add(path + ": " + ex.Message);
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

        private static string BuildSummary(LocalDataClearResult result)
        {
            string status = result.Ok ? "ok" : "with errors";
            return "removed " + result.RemovedEntries + " local data entries; " + status;
        }
    }
}
