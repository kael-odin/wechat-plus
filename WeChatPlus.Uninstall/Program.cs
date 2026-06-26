using System;
using System.Collections.Generic;
using WeChatPlus.Core.Contracts;
using WeChatPlus.Core.Models;
using WeChatPlus.Core.Services;

namespace WeChatPlus.Uninstall
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            HelperCommandResult output;
            try
            {
                Dictionary<string, string> options = ParseOptions(args);
                InstallerManifest manifest = InstallerManifest.CreateDefault(ReleasePackageManifest.CreateDefault());
                string installRoot = GetOption(options, "install-root", AppDomain.CurrentDomain.BaseDirectory);
                string startMenuRoot = GetOption(options, "start-menu-root", manifest.StartMenuFolder);
                string dataRoot = GetOption(options, "data-root", AppPaths.GetDefaultDataRoot());
                bool removeUserData = HasFlag(args, "--remove-user-data");
                bool planOnly = HasFlag(args, "--plan");

                UninstallPlan plan = UninstallPlanner.Create(manifest, installRoot, startMenuRoot, dataRoot);
                output = HelperCommandResult.Success(planOnly ? "uninstall plan" : "uninstall execute");
                output.Data["productName"] = plan.ProductName;
                output.Data["installDirectory"] = plan.InstallDirectory;
                output.Data["userDataDirectory"] = plan.UserDataDirectory;
                output.Data["preserveUserData"] = plan.PreserveUserData && !removeUserData;
                output.Data["runtimeFileCount"] = plan.RuntimeFilePaths.Length;
                output.Data["shortcutCount"] = plan.ShortcutPaths.Length;

                if (!planOnly)
                {
                    UninstallResult result = UninstallService.Execute(plan, removeUserData);
                    output.Ok = result.Ok;
                    output.Message = result.SummaryText;
                    output.Data["removedRuntimeFiles"] = result.RemovedRuntimeFiles;
                    output.Data["removedShortcuts"] = result.RemovedShortcuts;
                    output.Data["removedUserData"] = result.RemovedUserData;
                    output.Data["errors"] = result.Errors;
                }
            }
            catch (Exception ex)
            {
                output = HelperCommandResult.Failure("uninstall", ex.Message);
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(JsonResultWriter.Serialize(output));
            return output.Ok ? 0 : 1;
        }

        private static Dictionary<string, string> ParseOptions(string[] args)
        {
            Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                string item = args[i] ?? string.Empty;
                if (item.StartsWith("--") && i + 1 < args.Length && !(args[i + 1] ?? string.Empty).StartsWith("--"))
                {
                    options[item.Substring(2)] = args[i + 1];
                    i++;
                }
            }

            return options;
        }

        private static string GetOption(Dictionary<string, string> options, string name, string fallback)
        {
            string value;
            return options.TryGetValue(name, out value) ? value : fallback;
        }

        private static bool HasFlag(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
