using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public static class WorkspaceStatusFormatter
    {
        public static string FormatFocusMode(AccountRecord account, bool focusAttempted, string helperOutput)
        {
            return FormatFocusMode(account, focusAttempted ? "成功" : "未执行", helperOutput);
        }

        public static string FormatFocusMode(AccountRecord account, string focusResult, string helperOutput)
        {
            if (account == null)
            {
                return "非嵌入聚焦模式：请先选择微信账号。";
            }

            string name = string.IsNullOrWhiteSpace(account.DisplayName) ? "微信账号" : account.DisplayName;
            string status = string.IsNullOrWhiteSpace(account.Status) ? "Unknown" : account.Status;
            string handle = string.IsNullOrWhiteSpace(account.WindowHandle) ? "未检测" : account.WindowHandle;
            focusResult = string.IsNullOrWhiteSpace(focusResult) ? "未执行" : focusResult;

            string text = "非嵌入聚焦模式" + System.Environment.NewLine +
                "当前账号：" + name + System.Environment.NewLine +
                "状态：" + status + " / PID：" + account.ProcessId + " / 窗口句柄：" + handle + System.Environment.NewLine +
                "聚焦：" + focusResult;

            if (!string.IsNullOrWhiteSpace(helperOutput))
            {
                text += System.Environment.NewLine + "助手返回：" + Trim(helperOutput);
            }

            return text;
        }

        public static string FormatFocusFailure(AccountRecord account, string errorMessage)
        {
            string text = FormatFocusMode(account, false, string.Empty);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                text += System.Environment.NewLine + "聚焦失败：" + errorMessage;
            }

            return text;
        }

        private static string Trim(string value)
        {
            value = value.Replace(System.Environment.NewLine, " ").Trim();
            return value.Length > 120 ? value.Substring(0, 120) + "..." : value;
        }
    }
}
