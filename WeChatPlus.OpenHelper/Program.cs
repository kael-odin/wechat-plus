using System;
using WeChatPlus.Core.Contracts;
using WeChatPlus.OpenHelper.MultiInstance;

namespace WeChatPlus.OpenHelper
{
    internal static class Program
    {
        private const string Version = "0.1.0";
        private static readonly WeChatProcessService WeChatService = new WeChatProcessService();

        private static int Main(string[] args)
        {
            HelperCommandResult result;
            try
            {
                HelperCommand command = HelperCommandParser.Parse(args);
                result = Execute(command);
            }
            catch (Exception ex)
            {
                result = HelperCommandResult.Failure("unknown", ex.Message);
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(JsonResultWriter.Serialize(result));
            return result.Ok ? 0 : 1;
        }

        private static HelperCommandResult Execute(HelperCommand command)
        {
            if (string.Equals(command.Area, "version", StringComparison.OrdinalIgnoreCase))
            {
                HelperCommandResult result = HelperCommandResult.Success("version");
                result.Data["version"] = Version;
                result.Data["license"] = "GPLv3-compatible open helper boundary";
                return result;
            }

            if (string.Equals(command.Area, "multi-instance", StringComparison.OrdinalIgnoreCase))
            {
                return ExecuteMultiInstance(command);
            }

            if (string.Equals(command.Area, "patch", StringComparison.OrdinalIgnoreCase))
            {
                HelperCommandResult result = HelperCommandResult.Success("patch " + command.Action);
                result.Data["supported"] = false;
                result.Data["reason"] = "Patch operations are reserved for the open helper implementation.";
                result.Data["app"] = command.GetOption("app");
                return result;
            }

            return HelperCommandResult.Failure(command.Area, "Unsupported helper command.");
        }

        private static HelperCommandResult ExecuteMultiInstance(HelperCommand command)
        {
            if (string.Equals(command.Action, "status", StringComparison.OrdinalIgnoreCase))
            {
                HelperCommandResult result = HelperCommandResult.Success("multi-instance status");
                System.Diagnostics.Process[] processes = WeChatService.GetProcesses();
                result.Data["processCount"] = processes.Length;
                result.Data["installPath"] = WeChatService.FindInstallPath();
                return result;
            }

            if (string.Equals(command.Action, "windows", StringComparison.OrdinalIgnoreCase))
            {
                HelperCommandResult result = HelperCommandResult.Success("multi-instance windows");
                System.Diagnostics.Process[] processes = WeChatService.GetProcesses();
                System.Collections.ArrayList windows = new System.Collections.ArrayList();
                for (int i = 0; i < processes.Length; i++)
                {
                    System.Collections.Generic.Dictionary<string, object> item = new System.Collections.Generic.Dictionary<string, object>();
                    item["processId"] = processes[i].Id;
                    item["windowHandle"] = processes[i].MainWindowHandle.ToInt64().ToString();
                    item["title"] = processes[i].MainWindowTitle ?? string.Empty;
                    item["hasWindow"] = processes[i].MainWindowHandle != IntPtr.Zero;
                    windows.Add(item);
                }
                result.Data["windows"] = windows;
                result.Data["processCount"] = processes.Length;
                return result;
            }

            if (string.Equals(command.Action, "start", StringComparison.OrdinalIgnoreCase))
            {
                HelperCommandResult result = HelperCommandResult.Success("multi-instance start");
                result.Data["processId"] = WeChatService.StartWeChat();
                return result;
            }

            if (string.Equals(command.Action, "close-mutex", StringComparison.OrdinalIgnoreCase))
            {
                int processId;
                if (!int.TryParse(command.GetOption("pid"), out processId))
                {
                    return HelperCommandResult.Failure("multi-instance close-mutex", "Missing or invalid --pid.");
                }

                bool closed = WeChatService.CloseMutex(processId);
                HelperCommandResult result = HelperCommandResult.Success("multi-instance close-mutex");
                result.Data["processId"] = processId;
                result.Data["closed"] = closed;
                return result;
            }

            if (string.Equals(command.Action, "close-all-mutex", StringComparison.OrdinalIgnoreCase))
            {
                int closed = WeChatService.CloseAllMutexes();
                HelperCommandResult result = HelperCommandResult.Success("multi-instance close-all-mutex");
                result.Data["closedCount"] = closed;
                result.Data["processCount"] = WeChatService.CountProcesses();
                return result;
            }

            if (string.Equals(command.Action, "focus", StringComparison.OrdinalIgnoreCase))
            {
                long handleValue;
                if (!long.TryParse(command.GetOption("handle"), out handleValue))
                {
                    return HelperCommandResult.Failure("multi-instance focus", "Missing or invalid --handle.");
                }

                bool focused = WeChatService.FocusWindow(new IntPtr(handleValue));
                HelperCommandResult result = HelperCommandResult.Success("multi-instance focus");
                result.Data["focused"] = focused;
                result.Data["windowHandle"] = handleValue.ToString();
                return result;
            }

            if (string.Equals(command.Action, "close-all", StringComparison.OrdinalIgnoreCase))
            {
                int closed = WeChatService.CloseAllProcesses();
                HelperCommandResult result = HelperCommandResult.Success("multi-instance close-all");
                result.Data["closedCount"] = closed;
                return result;
            }

            if (string.Equals(command.Action, "close", StringComparison.OrdinalIgnoreCase))
            {
                int processId;
                if (!int.TryParse(command.GetOption("pid"), out processId))
                {
                    return HelperCommandResult.Failure("multi-instance close", "Missing or invalid --pid.");
                }

                bool closed = WeChatService.CloseProcess(processId);
                HelperCommandResult result = HelperCommandResult.Success("multi-instance close");
                result.Data["processId"] = processId;
                result.Data["closed"] = closed;
                return result;
            }

            return HelperCommandResult.Failure("multi-instance", "Unsupported multi-instance action.");
        }
    }
}
