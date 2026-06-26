using System;
using System.IO;
using WeChatPlus.Core.Contracts;
using WeChatPlus.Core.Models;
using WeChatPlus.Core.Services;

namespace WeChatPlus.Tests
{
    internal static class Program
    {
        private static int _passed;

        private static int Main(string[] args)
        {
            try
            {
                Run("parses helper commands", ParsesHelperCommands);
                Run("serializes helper result", SerializesHelperResult);
                Run("parses helper version result", ParsesHelperVersionResult);
                Run("parses helper runtime status result", ParsesHelperRuntimeStatusResult);
                Run("parses helper window results", ParsesHelperWindowResults);
                Run("seeds and searches quick replies", SeedsAndSearchesQuickReplies);
                Run("returns shortcut quick replies", ReturnsShortcutQuickReplies);
                Run("updates quick replies by id", UpdatesQuickRepliesById);
                Run("deletes quick replies by id", DeletesQuickRepliesById);
                Run("imports exported quick reply json", ImportsExportedQuickReplyJson);
                Run("imports quick reply csv", ImportsQuickReplyCsv);
                Run("persists account records", PersistsAccountRecords);
                Run("renames and deletes account records", RenamesAndDeletesAccountRecords);
                Run("preserves account remarks on process refresh", PreservesAccountRemarksOnProcessRefresh);
                Run("marks missing account processes offline", MarksMissingAccountProcessesOffline);
                Run("reorders account records", ReordersAccountRecords);
                Run("formats fallback workspace focus status", FormatsFallbackWorkspaceFocusStatus);
                Run("defines release package manifest", DefinesReleasePackageManifest);
                Run("defines installer manifest", DefinesInstallerManifest);
                Run("validates release package files", ValidatesReleasePackageFiles);
                Run("seeds open source component declarations", SeedsOpenSourceComponentDeclarations);
                Run("copies packaged open source component declarations", CopiesPackagedOpenSourceComponentDeclarations);
                Run("writes and exports diagnostics log", WritesAndExportsDiagnosticsLog);
                Run("builds settings summary", BuildsSettingsSummary);
                Run("formats runtime environment checks", FormatsRuntimeEnvironmentChecks);
                Run("persists privacy lock state", PersistsPrivacyLockState);
                Run("creates trial license state", CreatesTrialLicenseState);
                Run("applies local license activation", AppliesLocalLicenseActivation);
                Run("enforces trial feature limits", EnforcesTrialFeatureLimits);
                Run("builds license activation request", BuildsLicenseActivationRequest);
                Run("parses update manifest and formats status", ParsesUpdateManifestAndFormatsStatus);
                Run("verifies helper sha256 from update manifest", VerifiesHelperSha256FromUpdateManifest);
                Console.WriteLine("All tests passed: " + _passed);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static void Run(string name, Action test)
        {
            test();
            _passed++;
            Console.WriteLine("PASS " + name);
        }

        private static void ParsesHelperCommands()
        {
            HelperCommand version = HelperCommandParser.Parse(new[] { "version", "--json" });
            AssertEqual("version", version.Area, "version area");
            AssertEqual("", version.Action, "version action");
            AssertTrue(version.Json, "version json flag");

            HelperCommand start = HelperCommandParser.Parse(new[] { "multi-instance", "start" });
            AssertEqual("multi-instance", start.Area, "start area");
            AssertEqual("start", start.Action, "start action");

            HelperCommand close = HelperCommandParser.Parse(new[] { "multi-instance", "close-mutex", "--pid", "1234" });
            AssertEqual("multi-instance", close.Area, "close area");
            AssertEqual("close-mutex", close.Action, "close action");
            AssertEqual("1234", close.GetOption("pid"), "pid option");

            HelperCommand closeProcess = HelperCommandParser.Parse(new[] { "multi-instance", "close", "--pid", "5678" });
            AssertEqual("multi-instance", closeProcess.Area, "close process area");
            AssertEqual("close", closeProcess.Action, "close process action");
            AssertEqual("5678", closeProcess.GetOption("pid"), "close process pid option");

            HelperCommand embed = HelperCommandParser.Parse(new[] { "multi-instance", "embed", "--handle", "100", "--parent", "200" });
            AssertEqual("multi-instance", embed.Area, "embed area");
            AssertEqual("embed", embed.Action, "embed action");
            AssertEqual("100", embed.GetOption("handle"), "embed handle option");
            AssertEqual("200", embed.GetOption("parent"), "embed parent option");

            HelperCommand detach = HelperCommandParser.Parse(new[] { "multi-instance", "detach", "--handle", "100" });
            AssertEqual("multi-instance", detach.Area, "detach area");
            AssertEqual("detach", detach.Action, "detach action");
            AssertEqual("100", detach.GetOption("handle"), "detach handle option");
        }

        private static void SerializesHelperResult()
        {
            HelperCommandResult result = HelperCommandResult.Success("version");
            result.Data["version"] = "0.1.0";

            string json = JsonResultWriter.Serialize(result);

            AssertContains(json, "\"ok\":true", "ok json");
            AssertContains(json, "\"command\":\"version\"", "command json");
            AssertContains(json, "\"version\":\"0.1.0\"", "data json");
        }

        private static void ParsesHelperVersionResult()
        {
            string json = "{\"ok\":true,\"command\":\"version\",\"message\":\"\",\"data\":{\"version\":\"0.1.0\",\"license\":\"GPLv3-compatible open helper boundary\"}}";

            string version = HelperVersionResultParser.ParseVersion(json);

            AssertEqual("0.1.0", version, "helper version");
            AssertEqual("", HelperVersionResultParser.ParseVersion("{\"ok\":false}"), "missing helper version");
        }

        private static void ParsesHelperRuntimeStatusResult()
        {
            string json = "{\"ok\":true,\"command\":\"multi-instance status\",\"message\":\"\",\"data\":{\"processCount\":2,\"installPath\":\"C:\\\\Program Files\\\\Tencent\\\\WeChat\"}}";

            HelperRuntimeStatus status = HelperRuntimeStatusParser.Parse(json);

            AssertTrue(status.HelperOk, "runtime helper ok");
            AssertEqual("multi-instance status", status.Command, "runtime command");
            AssertTrue(status.ProcessCount == 2, "runtime process count");
            AssertEqual("C:\\Program Files\\Tencent\\WeChat", status.InstallPath, "runtime install path");

            HelperRuntimeStatus failed = HelperRuntimeStatusParser.Parse("{\"ok\":false,\"command\":\"multi-instance status\",\"message\":\"denied\"}");

            AssertTrue(!failed.HelperOk, "runtime helper not ok");
            AssertEqual("denied", failed.Message, "runtime helper message");
        }

        private static void ParsesHelperWindowResults()
        {
            string json = "{\"ok\":true,\"command\":\"multi-instance windows\",\"message\":\"\",\"data\":{\"windows\":[{\"processId\":1001,\"windowHandle\":\"123456\",\"title\":\"微信一号\",\"hasWindow\":true},{\"processId\":1002,\"windowHandle\":\"0\",\"title\":\"\",\"hasWindow\":false}],\"processCount\":2}}";

            HelperWindowInfo[] windows = HelperWindowResultParser.ParseWindows(json);

            AssertEqual("2", windows.Length.ToString(), "window count");
            AssertEqual("1001", windows[0].ProcessId.ToString(), "first pid");
            AssertEqual("123456", windows[0].WindowHandle, "first handle");
            AssertEqual("微信一号", windows[0].Title, "first title");
            AssertTrue(windows[0].HasWindow, "first has window");
            AssertEqual("1002", windows[1].ProcessId.ToString(), "second pid");
            AssertTrue(!windows[1].HasWindow, "second has window");
        }

        private static void SeedsAndSearchesQuickReplies()
        {
            string root = CreateTempRoot();
            QuickReplyRepository repository = new QuickReplyRepository(root);
            repository.EnsureSeedData();

            ReplyCategory[] categories = repository.GetCategories();
            QuickReply[] welcome = repository.Search("欢迎");

            AssertTrue(categories.Length >= 7, "default category count");
            AssertTrue(welcome.Length > 0, "welcome search count");
            AssertContains(welcome[0].Content, "欢迎", "welcome content");
        }

        private static void ReturnsShortcutQuickReplies()
        {
            QuickReplyRepository repository = new QuickReplyRepository(CreateTempRoot());
            repository.EnsureSeedData();

            QuickReply favorite = new QuickReply();
            favorite.Id = "favorite-shortcut";
            favorite.Title = "收藏短语";
            favorite.Content = "收藏内容";
            favorite.CategoryId = "favorite";
            favorite.Tags = "收藏";
            favorite.SortOrder = 999;
            favorite.IsFavorite = true;
            repository.SaveReply(favorite);

            QuickReply[] shortcuts = repository.GetShortcutReplies(3);

            AssertEqual("3", shortcuts.Length.ToString(), "shortcut count");
            AssertEqual("收藏短语", shortcuts[0].Title, "favorite first shortcut");
            AssertTrue(shortcuts[1].SortOrder <= shortcuts[2].SortOrder, "shortcut sorted by sort order");
        }

        private static void CreatesTrialLicenseState()
        {
            string root = CreateTempRoot();
            TrialLicenseService service = new TrialLicenseService(root);

            LicenseState state = service.GetOrCreateTrial();

            AssertEqual("trial", state.Plan, "trial plan");
            AssertTrue(!string.IsNullOrEmpty(state.DeviceIdHash), "device hash");
            AssertTrue(state.OfflineGraceUntilUtc > DateTime.UtcNow, "offline grace");
        }

        private static void AppliesLocalLicenseActivation()
        {
            string root = CreateTempRoot();
            TrialLicenseService service = new TrialLicenseService(root);

            LicenseState activated = service.ApplyActivation("ABC-123-SECRET", "personal", DateTime.UtcNow.AddDays(365));

            AssertEqual("personal", activated.Plan, "activated plan");
            AssertEqual("ABC...CRET", activated.LicenseKeyMasked, "masked license key");
            AssertTrue(activated.ExpiresAtUtc > DateTime.UtcNow.AddDays(300), "activated expiry");
            AssertTrue(activated.OfflineGraceUntilUtc > DateTime.UtcNow.AddDays(30), "activated offline grace");

            TrialLicenseService reloaded = new TrialLicenseService(root);
            LicenseState persisted = reloaded.GetOrCreateTrial();
            AssertEqual("personal", persisted.Plan, "persisted activated plan");
            AssertEqual("ABC...CRET", persisted.LicenseKeyMasked, "persisted masked key");
        }

        private static void EnforcesTrialFeatureLimits()
        {
            LicenseState trial = new LicenseState();
            trial.Plan = "trial";
            trial.ExpiresAtUtc = DateTime.UtcNow.AddDays(1);

            LicenseState personal = new LicenseState();
            personal.Plan = "personal";
            personal.ExpiresAtUtc = DateTime.UtcNow.AddDays(1);

            AssertTrue(LicenseFeaturePolicy.CanAddAccount(trial, 1), "trial can add second account");
            AssertTrue(!LicenseFeaturePolicy.CanAddAccount(trial, 2), "trial blocks third account");
            AssertTrue(!LicenseFeaturePolicy.CanImportOrExportReplies(trial), "trial blocks import export");
            AssertTrue(LicenseFeaturePolicy.CanAddAccount(personal, 12), "personal can add account");
            AssertTrue(LicenseFeaturePolicy.CanImportOrExportReplies(personal), "personal import export");
            AssertContains(LicenseFeaturePolicy.GetAccountLimitMessage(trial), "试用版最多可管理 2 个微信账号", "trial limit message");

            personal.ExpiresAtUtc = DateTime.UtcNow.AddDays(-1);
            AssertTrue(!LicenseFeaturePolicy.CanAddAccount(personal, 0), "expired personal blocks account");
            AssertTrue(!LicenseFeaturePolicy.CanImportOrExportReplies(personal), "expired personal blocks import export");
        }

        private static void PersistsAccountRecords()
        {
            string root = CreateTempRoot();
            AccountRepository repository = new AccountRepository(root);
            AccountRecord record = repository.UpsertFromProcess(2048, "微信客服号", "Ready");

            AccountRepository reloaded = new AccountRepository(root);
            AccountRecord[] accounts = reloaded.GetAll();

            AssertEqual("1", accounts.Length.ToString(), "account count");
            AssertEqual(record.Id, accounts[0].Id, "account id");
            AssertEqual("微信客服号", accounts[0].DisplayName, "account name");
            AssertEqual("Ready", accounts[0].Status, "account status");

            reloaded.UpdateStatus(record.Id, "Offline", 0, string.Empty);
            AccountRecord updated = reloaded.GetAll()[0];
            AssertEqual("Offline", updated.Status, "updated account status");
            AssertEqual("0", updated.ProcessId.ToString(), "updated process id");
        }

        private static void RenamesAndDeletesAccountRecords()
        {
            string root = CreateTempRoot();
            AccountRepository repository = new AccountRepository(root);
            AccountRecord first = repository.UpsertFromProcess(3001, "微信一号", "Ready");
            AccountRecord second = repository.UpsertFromProcess(3002, "微信二号", "Ready");

            bool renamed = repository.UpdateDisplayName(first.Id, "售前客服");
            repository.Delete(second.Id);

            AccountRepository reloaded = new AccountRepository(root);
            AccountRecord[] accounts = reloaded.GetAll();

            AssertTrue(renamed, "rename result");
            AssertEqual("1", accounts.Length.ToString(), "remaining account count");
            AssertEqual(first.Id, accounts[0].Id, "remaining account id");
            AssertEqual("售前客服", accounts[0].DisplayName, "renamed account");
            AssertTrue(!reloaded.UpdateDisplayName("missing-account", "无效账号"), "missing rename result");
        }

        private static void PreservesAccountRemarksOnProcessRefresh()
        {
            string root = CreateTempRoot();
            AccountRepository repository = new AccountRepository(root);
            AccountRecord record = repository.UpsertFromProcess(4001, "微信窗口 4001", "Detected");
            repository.UpdateDisplayName(record.Id, "售前客服");

            repository.UpsertFromProcess(4001, "微信窗口标题", "Detected");

            AccountRecord[] accounts = repository.GetAll();
            AssertEqual("售前客服", accounts[0].DisplayName, "remark after refresh");
        }

        private static void MarksMissingAccountProcessesOffline()
        {
            string root = CreateTempRoot();
            AccountRepository repository = new AccountRepository(root);
            AccountRecord active = repository.UpsertFromProcess(4101, "在线账号", "Detected");
            AccountRecord missing = repository.UpsertFromProcess(4102, "离线账号", "Detected");

            int changed = repository.MarkMissingProcessesOffline(new[] { 4101 });

            AccountRecord[] accounts = repository.GetAll();
            AccountRecord refreshedActive = FindAccount(accounts, active.Id);
            AccountRecord refreshedMissing = FindAccount(accounts, missing.Id);

            AssertEqual("1", changed.ToString(), "offline changed count");
            AssertEqual("Detected", refreshedActive.Status, "active status");
            AssertEqual("4101", refreshedActive.ProcessId.ToString(), "active pid");
            AssertEqual("Offline", refreshedMissing.Status, "missing status");
            AssertEqual("0", refreshedMissing.ProcessId.ToString(), "missing pid");
            AssertEqual("", refreshedMissing.WindowHandle, "missing handle");
        }

        private static void ReordersAccountRecords()
        {
            string root = CreateTempRoot();
            AccountRepository repository = new AccountRepository(root);
            AccountRecord first = repository.UpsertFromProcess(5001, "一号", "Ready");
            AccountRecord second = repository.UpsertFromProcess(5002, "二号", "Ready");
            AccountRecord third = repository.UpsertFromProcess(5003, "三号", "Ready");

            bool movedUp = repository.MoveAccount(third.Id, -1);
            bool movedDown = repository.MoveAccount(first.Id, 1);

            AccountRepository reloaded = new AccountRepository(root);
            AccountRecord[] accounts = reloaded.GetAll();

            AssertTrue(movedUp, "move up result");
            AssertTrue(movedDown, "move down result");
            AssertEqual(third.Id, accounts[0].Id, "first after reorder");
            AssertEqual(first.Id, accounts[1].Id, "second after reorder");
            AssertEqual(second.Id, accounts[2].Id, "third after reorder");
            AssertTrue(!reloaded.MoveAccount(second.Id, 1), "move last down result");
        }

        private static void FormatsFallbackWorkspaceFocusStatus()
        {
            AccountRecord account = new AccountRecord();
            account.DisplayName = "售前客服";
            account.Status = "Detected";
            account.ProcessId = 6001;
            account.WindowHandle = "123456";

            string focused = WorkspaceStatusFormatter.FormatFocusMode(account, "成功", "{\"ok\":true,\"data\":{\"focused\":true}}");
            string notFocused = WorkspaceStatusFormatter.FormatFocusMode(account, "失败", "{\"ok\":true,\"data\":{\"focused\":false}}");
            string unavailable = WorkspaceStatusFormatter.FormatFocusMode(account, false, string.Empty);

            AssertContains(focused, "非嵌入聚焦模式", "focus mode label");
            AssertContains(focused, "售前客服", "focus account");
            AssertContains(focused, "PID：6001", "focus pid");
            AssertContains(focused, "窗口句柄：123456", "focus handle");
            AssertContains(focused, "聚焦：成功", "focus result");
            AssertContains(notFocused, "聚焦：失败", "not focused result");
            AssertContains(unavailable, "聚焦：未执行", "unavailable focus result");
        }

        private static void DefinesReleasePackageManifest()
        {
            ReleasePackageManifest manifest = ReleasePackageManifest.CreateDefault();

            AssertEqual("WeChat Plus MVP runtime package", manifest.Name, "release manifest name");
            AssertTrue(manifest.Files.Length >= 5, "release file count");
            AssertPackageFile(manifest, "WeChatPlus.Shell.exe", "ClosedSourceShell");
            AssertPackageFile(manifest, "WeChatPlus.Core.dll", "NeutralCore");
            AssertPackageFile(manifest, "WeChatPlus.OpenHelper.exe", "OpenHelper");
            AssertPackageFile(manifest, "LICENSE", "OpenSourceLicense");
            AssertPackageFile(manifest, "README.md", "RuntimeGuide");
            AssertPackageFile(manifest, "components.json", "OpenSourceNotice");
            AssertPackageFile(manifest, "update-manifest.json", "UpdateManifest");
        }

        private static void DefinesInstallerManifest()
        {
            InstallerManifest manifest = InstallerManifest.CreateDefault(ReleasePackageManifest.CreateDefault());

            AssertEqual("WeChat Plus", manifest.ProductName, "installer product name");
            AssertContains(manifest.DefaultInstallDirectory, "WeChat Plus", "installer default path");
            AssertEqual("WeChat Plus", manifest.StartMenuFolder, "installer start menu folder");
            AssertEqual("WeChat Plus.lnk", manifest.ShortcutName, "installer shortcut name");
            AssertContains(manifest.ShortcutTarget, "WeChatPlus.Shell.exe", "installer shortcut target");
            AssertContains(manifest.UninstallCommand, "WeChatPlus.Uninstall.exe", "installer uninstall command");
            AssertContains(manifest.OpenHelperSourceUrl, "huiyadanli/RevokeMsgPatcher", "installer helper source url");
            AssertTrue(manifest.Files.Length >= 7, "installer file count");
            AssertPackageFile(manifest.RuntimePackage, "WeChatPlus.OpenHelper.exe", "OpenHelper");
        }

        private static void ValidatesReleasePackageFiles()
        {
            string runtimeRoot = CreateTempRoot();
            string[] files = new[]
            {
                "WeChatPlus.Shell.exe",
                "WeChatPlus.Core.dll",
                "LICENSE",
                "README.md",
                "components.json",
                "update-manifest.json"
            };
            for (int i = 0; i < files.Length; i++)
            {
                File.WriteAllText(Path.Combine(runtimeRoot, files[i]), "test");
            }

            ReleasePackageValidationResult missing = ReleasePackageValidator.Validate(runtimeRoot, ReleasePackageManifest.CreateDefault());

            AssertTrue(!missing.IsComplete, "incomplete runtime package");
            AssertTrue(missing.MissingFiles.Length == 1, "missing helper count");
            AssertEqual("WeChatPlus.OpenHelper.exe", missing.MissingFiles[0].Path, "missing helper path");
            AssertContains(missing.SummaryText, "缺少 1 个必需文件", "missing package summary");

            File.WriteAllText(Path.Combine(runtimeRoot, "WeChatPlus.OpenHelper.exe"), "test");

            ReleasePackageValidationResult complete = ReleasePackageValidator.Validate(runtimeRoot, ReleasePackageManifest.CreateDefault());

            AssertTrue(complete.IsComplete, "complete runtime package");
            AssertTrue(complete.MissingFiles.Length == 0, "complete missing count");
            AssertContains(complete.SummaryText, "运行包完整", "complete package summary");
        }

        private static void BuildsLicenseActivationRequest()
        {
            string root = CreateTempRoot();
            TrialLicenseService service = new TrialLicenseService(root);
            LicenseState state = service.GetOrCreateTrial();
            LicenseApiClient client = new LicenseApiClient("https://license.example.test/api");

            LicenseActivationRequest request = client.BuildActivationRequest("ABC-123-SECRET", state);

            AssertEqual("https://license.example.test/api/licenses/activate", request.Url, "activation url");
            AssertEqual("POST", request.Method, "activation method");
            AssertContains(request.BodyJson, "\"licenseKey\":\"ABC-123-SECRET\"", "license key body");
            AssertContains(request.BodyJson, "\"deviceIdHash\":\"" + state.DeviceIdHash + "\"", "device hash body");
            AssertContains(request.BodyJson, "\"product\":\"wechat-plus\"", "product body");
        }

        private static void ParsesUpdateManifestAndFormatsStatus()
        {
            string json = "{\"productVersion\":\"0.2.0\",\"helperVersion\":\"0.1.1\",\"downloadUrl\":\"https://example.test/wechat-plus.zip\",\"helperSha256\":\"abc123\",\"releaseNotes\":\"新增窗口嵌入降级提示\"}";
            UpdateManifest manifest = UpdateManifest.Parse(json);
            UpdateCheckStatus current = UpdateCheckService.Evaluate(manifest, "0.1.0", "0.1.0");
            UpdateCheckStatus latest = UpdateCheckService.Evaluate(manifest, "0.2.0", "0.1.1");

            AssertEqual("0.2.0", manifest.ProductVersion, "manifest product version");
            AssertEqual("0.1.1", manifest.HelperVersion, "manifest helper version");
            AssertTrue(current.ProductUpdateAvailable, "product update available");
            AssertTrue(current.HelperUpdateAvailable, "helper update available");
            AssertContains(current.StatusText, "主程序 0.2.0 可用", "product update text");
            AssertContains(current.StatusText, "助手组件 0.1.1 可用", "helper update text");
            AssertContains(current.StatusText, "新增窗口嵌入降级提示", "release notes text");
            AssertTrue(!latest.ProductUpdateAvailable, "latest product no update");
            AssertTrue(!latest.HelperUpdateAvailable, "latest helper no update");
            AssertContains(latest.StatusText, "已是最新版本", "latest status text");
        }

        private static void VerifiesHelperSha256FromUpdateManifest()
        {
            string root = CreateTempRoot();
            string helperPath = Path.Combine(root, "WeChatPlus.OpenHelper.exe");
            File.WriteAllText(helperPath, "abc");

            UpdateManifest manifest = new UpdateManifest();
            manifest.HelperSha256 = "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD";

            HelperIntegrityStatus matched = HelperIntegrityVerifier.Verify(helperPath, manifest);

            AssertTrue(matched.FileExists, "helper file exists");
            AssertTrue(matched.HashProvided, "helper hash provided");
            AssertTrue(matched.HashMatches, "helper hash matches");
            AssertContains(matched.StatusText, "校验通过", "helper integrity matched text");

            manifest.HelperSha256 = "000000";
            HelperIntegrityStatus mismatch = HelperIntegrityVerifier.Verify(helperPath, manifest);

            AssertTrue(!mismatch.HashMatches, "helper hash mismatch");
            AssertContains(mismatch.StatusText, "校验失败", "helper integrity mismatch text");

            manifest.HelperSha256 = string.Empty;
            HelperIntegrityStatus skipped = HelperIntegrityVerifier.Verify(helperPath, manifest);

            AssertTrue(!skipped.HashProvided, "helper hash missing");
            AssertContains(skipped.StatusText, "未提供", "helper integrity skipped text");

            HelperIntegrityStatus missing = HelperIntegrityVerifier.Verify(Path.Combine(root, "missing.exe"), manifest);

            AssertTrue(!missing.FileExists, "missing helper file");
            AssertContains(missing.StatusText, "未找到", "missing helper text");
        }

        private static void SeedsOpenSourceComponentDeclarations()
        {
            ComponentRepository repository = new ComponentRepository(CreateTempRoot());

            OpenSourceComponent[] components = repository.GetAll();

            AssertTrue(components.Length > 0, "component count");
            AssertEqual("wechat-plus-open-helper", components[0].Id, "component id");
            AssertEqual("WeChatPlus.OpenHelper", components[0].Name, "component name");
            AssertEqual("GPLv3", components[0].License, "component license");
            AssertContains(components[0].SourceUrl, "huiyadanli/RevokeMsgPatcher", "component source");
        }

        private static void CopiesPackagedOpenSourceComponentDeclarations()
        {
            string dataRoot = CreateTempRoot();
            string packageRoot = CreateTempRoot();
            string packageJson = "[{\"id\":\"packaged-helper\",\"name\":\"Packaged Helper\",\"version\":\"1.0\",\"license\":\"GPLv3\",\"sourceUrl\":\"https://example.test/helper\",\"binaryPath\":\"helper.exe\",\"sha256\":\"abc\",\"installedAtUtc\":\"2026-06-26T00:00:00Z\"}]";
            File.WriteAllText(Path.Combine(packageRoot, "components.json"), packageJson);

            ComponentRepository repository = new ComponentRepository(dataRoot, packageRoot);
            OpenSourceComponent[] components = repository.GetAll();

            AssertEqual("1", components.Length.ToString(), "packaged component count");
            AssertEqual("packaged-helper", components[0].Id, "packaged component id");
            AssertEqual("Packaged Helper", components[0].Name, "packaged component name");
            AssertEqual("abc", components[0].Sha256, "packaged component sha");
        }

        private static void WritesAndExportsDiagnosticsLog()
        {
            string root = CreateTempRoot();
            DiagnosticsLogService diagnostics = new DiagnosticsLogService(root);

            string logPath = diagnostics.Write("helper", "start failed", new InvalidOperationException("missing helper"));
            string exportedPath = Path.Combine(root, "diagnostics-export.log");
            diagnostics.ExportTo(exportedPath);

            string log = File.ReadAllText(logPath);
            string exported = File.ReadAllText(exportedPath);

            AssertTrue(File.Exists(logPath), "diagnostics log exists");
            AssertContains(log, "helper", "diagnostics area");
            AssertContains(log, "start failed", "diagnostics message");
            AssertContains(log, "InvalidOperationException", "diagnostics exception type");
            AssertContains(log, "missing helper", "diagnostics exception message");
            AssertEqual(log, exported, "exported diagnostics content");
        }

        private static void BuildsSettingsSummary()
        {
            string dataRoot = CreateTempRoot();
            string runtimeRoot = CreateTempRoot();

            SettingsSummary summary = SettingsSummaryService.Create(dataRoot, runtimeRoot);

            AssertEqual(dataRoot, summary.DataRoot, "settings data root");
            AssertEqual(runtimeRoot, summary.RuntimeRoot, "settings runtime root");
            AssertContains(summary.HelperPath, "WeChatPlus.OpenHelper.exe", "settings helper path");
            AssertContains(summary.UpdateManifestPath, "update-manifest.json", "settings update manifest");
            AssertContains(summary.AccountsPath, "accounts.json", "settings accounts path");
            AssertContains(summary.QuickRepliesPath, "quick_replies.json", "settings replies path");
            AssertContains(summary.LicenseStatePath, "license_state.json", "settings license path");
            AssertContains(summary.PrivacyLockPath, "privacy_lock.json", "settings privacy path");
        }

        private static void FormatsRuntimeEnvironmentChecks()
        {
            HelperRuntimeStatus helper = new HelperRuntimeStatus();
            helper.HelperOk = true;
            helper.ProcessCount = 1;
            helper.InstallPath = "C:\\Program Files\\Tencent\\WeChat";

            RuntimeEnvironmentCheck complete = RuntimeEnvironmentChecker.Create(true, true, helper);

            AssertTrue(complete.IsReady, "environment ready");
            AssertContains(complete.SummaryText, "管理员权限：已启用", "administrator summary");
            AssertContains(complete.SummaryText, "微信安装路径：C:\\Program Files\\Tencent\\WeChat", "wechat path summary");
            AssertContains(complete.SummaryText, "微信进程：1", "wechat process summary");

            HelperRuntimeStatus missingPath = new HelperRuntimeStatus();
            missingPath.HelperOk = true;
            RuntimeEnvironmentCheck incomplete = RuntimeEnvironmentChecker.Create(false, true, missingPath);

            AssertTrue(!incomplete.IsReady, "environment not ready");
            AssertContains(incomplete.SummaryText, "管理员权限：未启用", "missing admin summary");
            AssertContains(incomplete.SummaryText, "微信安装路径：未检测到", "missing path summary");
        }

        private static void PersistsPrivacyLockState()
        {
            string root = CreateTempRoot();
            PrivacyLockService service = new PrivacyLockService(root);

            PrivacyLockState initial = service.GetOrCreate();
            AssertTrue(!initial.IsLocked, "privacy lock initially unlocked");

            PrivacyLockState locked = service.Lock();
            AssertTrue(locked.IsLocked, "privacy lock locked");
            AssertTrue(!service.TryUnlock("bad-pin"), "bad pin stays locked");
            AssertTrue(service.GetOrCreate().IsLocked, "bad pin persisted locked");
            AssertTrue(service.TryUnlock(PrivacyLockService.DefaultPin), "default pin unlocks");
            AssertTrue(!service.GetOrCreate().IsLocked, "unlock persisted");
        }

        private static void UpdatesQuickRepliesById()
        {
            QuickReplyRepository repository = new QuickReplyRepository(CreateTempRoot());
            QuickReply reply = new QuickReply();
            reply.Id = "reply-to-update";
            reply.Title = "Before";
            reply.Content = "Original content";
            reply.CategoryId = "common";
            reply.Tags = "old";
            reply.SortOrder = 10;
            repository.SaveReply(reply);

            QuickReply changed = new QuickReply();
            changed.Id = "reply-to-update";
            changed.Title = "After";
            changed.Content = "Updated content";
            changed.CategoryId = "welcome";
            changed.Tags = "new,updated";
            changed.SortOrder = 20;
            changed.IsFavorite = true;
            repository.SaveReply(changed);

            QuickReply[] replies = repository.Search("Updated");
            AssertEqual("1", replies.Length.ToString(), "updated reply count");
            AssertEqual("After", replies[0].Title, "updated title");
            AssertEqual("welcome", replies[0].CategoryId, "updated category");
            AssertEqual("new,updated", replies[0].Tags, "updated tags");
            AssertTrue(replies[0].IsFavorite, "updated favorite");
        }

        private static void DeletesQuickRepliesById()
        {
            QuickReplyRepository repository = new QuickReplyRepository(CreateTempRoot());
            QuickReply reply = new QuickReply();
            reply.Id = "reply-to-delete";
            reply.Title = "Delete me";
            reply.Content = "Temporary content";
            reply.CategoryId = "common";
            reply.Tags = "temporary";
            repository.SaveReply(reply);

            bool deleted = repository.DeleteReply("reply-to-delete");

            AssertTrue(deleted, "delete result");
            AssertEqual("0", repository.Search("Temporary").Length.ToString(), "deleted search count");
            AssertTrue(!repository.DeleteReply("missing-reply"), "delete missing result");
        }

        private static void ImportsExportedQuickReplyJson()
        {
            QuickReplyRepository source = new QuickReplyRepository(CreateTempRoot());
            source.EnsureSeedData();
            string exported = source.ExportJson();

            QuickReplyRepository target = new QuickReplyRepository(CreateTempRoot());
            int imported = target.ImportJson(exported, true);

            AssertTrue(imported > 0, "json import count");
            AssertTrue(target.Search("欢迎").Length > 0, "json import search");
        }

        private static void ImportsQuickReplyCsv()
        {
            QuickReplyRepository repository = new QuickReplyRepository(CreateTempRoot());
            repository.EnsureSeedData();

            int imported = repository.ImportCsv("title,content,category,tags\r\n测试话术,欢迎来到门店,welcome,测试\r\n", true);

            AssertEqual("1", imported.ToString(), "csv import count");
            QuickReply[] replies = repository.Search("门店");
            AssertTrue(replies.Length > 0, "csv search");
            AssertContains(replies[0].Content, "门店", "csv content");
        }

        private static string CreateTempRoot()
        {
            string root = Path.Combine(Path.GetTempPath(), "WeChatPlusTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        private static AccountRecord FindAccount(AccountRecord[] accounts, string id)
        {
            for (int i = 0; i < accounts.Length; i++)
            {
                if (string.Equals(accounts[i].Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return accounts[i];
                }
            }

            throw new InvalidOperationException("Missing account " + id);
        }

        private static void AssertPackageFile(ReleasePackageManifest manifest, string path, string role)
        {
            for (int i = 0; i < manifest.Files.Length; i++)
            {
                if (string.Equals(manifest.Files[i].Path, path, StringComparison.OrdinalIgnoreCase))
                {
                    AssertEqual(role, manifest.Files[i].Role, "release file role " + path);
                    return;
                }
            }

            throw new InvalidOperationException("Missing release package file " + path);
        }

        private static void AssertEqual(string expected, string actual, string message)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(message + ": expected '" + expected + "', got '" + actual + "'");
            }
        }

        private static void AssertTrue(bool value, string message)
        {
            if (!value)
            {
                throw new InvalidOperationException(message + ": expected true");
            }
        }

        private static void AssertContains(string value, string expectedSubstring, string message)
        {
            if (value == null || value.IndexOf(expectedSubstring, StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException(message + ": missing '" + expectedSubstring + "' in '" + value + "'");
            }
        }
    }
}
