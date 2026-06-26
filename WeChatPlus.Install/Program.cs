using System;
using System.Collections.Generic;
using WeChatPlus.Core.Contracts;
using WeChatPlus.Core.Models;
using WeChatPlus.Core.Services;

namespace WeChatPlus.Install
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
                string packageRoot = GetOption(options, "package-root", AppDomain.CurrentDomain.BaseDirectory);
                string installRoot = GetOption(options, "install-root", manifest.DefaultInstallDirectory);
                string startMenuRoot = GetOption(options, "start-menu-root", manifest.StartMenuFolder);
                bool planOnly = HasFlag(args, "--plan");
                bool writeRegistry = HasFlag(args, "--write-registry");
                bool rollbackOnFailure = HasFlag(args, "--rollback-on-failure");

                InstallPlan plan = InstallPlanner.Create(manifest, packageRoot, installRoot, startMenuRoot);
                output = HelperCommandResult.Success(planOnly ? "install plan" : "install execute");
                output.Data["productName"] = plan.ProductName;
                output.Data["packageDirectory"] = plan.PackageDirectory;
                output.Data["installDirectory"] = plan.InstallDirectory;
                output.Data["shortcutPath"] = plan.ShortcutPath;
                output.Data["fileCount"] = plan.FileCopies.Length;
                output.Data["uninstallCommand"] = plan.UninstallCommand;
                output.Data["registryKey"] = plan.RegistryKey;
                output.Data["writeRegistry"] = writeRegistry;
                output.Data["rollbackOnFailure"] = rollbackOnFailure;

                if (!planOnly)
                {
                    InstallResult result = InstallService.Execute(plan, writeRegistry, rollbackOnFailure);
                    output.Ok = result.Ok;
                    output.Message = result.SummaryText;
                    output.Data["copiedFiles"] = result.CopiedFiles;
                    output.Data["createdShortcut"] = result.CreatedShortcut;
                    output.Data["shortcutPath"] = result.ShortcutPath;
                    output.Data["shortcutMode"] = result.ShortcutMode;
                    output.Data["wroteRegistration"] = result.WroteRegistration;
                    output.Data["registrationPath"] = result.RegistrationPath;
                    output.Data["wroteRegistry"] = result.WroteRegistry;
                    output.Data["registryMode"] = result.RegistryMode;
                    output.Data["rolledBack"] = result.RolledBack;
                    output.Data["rolledBackFiles"] = result.RolledBackFiles;
                    output.Data["rolledBackShortcut"] = result.RolledBackShortcut;
                    output.Data["rolledBackRegistration"] = result.RolledBackRegistration;
                    output.Data["rolledBackRegistry"] = result.RolledBackRegistry;
                    output.Data["errors"] = result.Errors;
                }
            }
            catch (Exception ex)
            {
                output = HelperCommandResult.Failure("install", ex.Message);
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
