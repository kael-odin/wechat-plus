using WeChatPlus.Core.Contracts;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class RuntimeEnvironmentChecker
    {
        public static RuntimeEnvironmentCheck Create(bool isAdministrator, bool helperAvailable, HelperRuntimeStatus helperStatus)
        {
            HelperRuntimeStatus status = helperStatus ?? new HelperRuntimeStatus();
            string installPath = string.IsNullOrWhiteSpace(status.InstallPath) ? string.Empty : status.InstallPath;

            RuntimeEnvironmentCheck check = new RuntimeEnvironmentCheck();
            check.IsAdministrator = isAdministrator;
            check.HelperAvailable = helperAvailable && status.HelperOk;
            check.WeChatInstallPath = installPath;
            check.WeChatInstallPathFound = !string.IsNullOrWhiteSpace(installPath);
            check.WeChatProcessCount = status.ProcessCount;
            check.IsReady = check.IsAdministrator && check.HelperAvailable && check.WeChatInstallPathFound;
            check.SummaryText =
                "管理员权限：" + (check.IsAdministrator ? "已启用" : "未启用") + System.Environment.NewLine +
                "助手组件：" + (check.HelperAvailable ? "可用" : "不可用") + System.Environment.NewLine +
                "微信安装路径：" + (check.WeChatInstallPathFound ? check.WeChatInstallPath : "未检测到") + System.Environment.NewLine +
                "微信进程：" + check.WeChatProcessCount;
            return check;
        }
    }
}
