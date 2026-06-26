using System;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class LicenseFeaturePolicy
    {
        public const int TrialAccountLimit = 2;
        public const int TrialReplyLimit = 50;

        public static bool CanAddAccount(LicenseState state, int currentAccountCount)
        {
            if (!IsActive(state))
            {
                return false;
            }

            if (IsTrial(state))
            {
                return currentAccountCount < TrialAccountLimit;
            }

            return true;
        }

        public static bool CanImportOrExportReplies(LicenseState state)
        {
            if (!IsActive(state))
            {
                return false;
            }

            return !IsTrial(state);
        }

        public static bool CanCreateReply(LicenseState state, int currentReplyCount)
        {
            if (!IsActive(state))
            {
                return false;
            }

            if (IsTrial(state))
            {
                return currentReplyCount < TrialReplyLimit;
            }

            return true;
        }

        public static string GetAccountLimitMessage(LicenseState state)
        {
            if (!IsActive(state))
            {
                return "授权已过期，请激活会员后继续管理微信账号。";
            }

            if (IsTrial(state))
            {
                return "试用版最多可管理 " + TrialAccountLimit + " 个微信账号，请激活会员后继续添加。";
            }

            return string.Empty;
        }

        public static string GetImportExportLimitMessage(LicenseState state)
        {
            if (!IsActive(state))
            {
                return "授权已过期，请激活会员后继续导入或导出话术库。";
            }

            if (IsTrial(state))
            {
                return "试用版不支持导入或导出话术库，请激活会员后使用。";
            }

            return string.Empty;
        }

        public static string GetReplyLimitMessage(LicenseState state)
        {
            if (!IsActive(state))
            {
                return "授权已过期，请激活会员后继续新增话术。";
            }

            if (IsTrial(state))
            {
                return "试用版最多可保存 " + TrialReplyLimit + " 条话术，请激活会员后继续新增。";
            }

            return string.Empty;
        }

        private static bool IsActive(LicenseState state)
        {
            return state != null && state.ExpiresAtUtc > DateTime.UtcNow;
        }

        private static bool IsTrial(LicenseState state)
        {
            return string.Equals(state.Plan, "trial", StringComparison.OrdinalIgnoreCase);
        }
    }
}
