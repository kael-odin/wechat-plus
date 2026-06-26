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
                result.Data["processCount"] = WeChatService.CountProcesses();
                result.Data["installPath"] = WeChatService.FindInstallPath();
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

            return HelperCommandResult.Failure("multi-instance", "Unsupported multi-instance action.");
        }
    }
}
