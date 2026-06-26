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
                Run("seeds and searches quick replies", SeedsAndSearchesQuickReplies);
                Run("imports exported quick reply json", ImportsExportedQuickReplyJson);
                Run("imports quick reply csv", ImportsQuickReplyCsv);
                Run("persists account records", PersistsAccountRecords);
                Run("creates trial license state", CreatesTrialLicenseState);
                Run("builds license activation request", BuildsLicenseActivationRequest);
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

        private static void CreatesTrialLicenseState()
        {
            string root = CreateTempRoot();
            TrialLicenseService service = new TrialLicenseService(root);

            LicenseState state = service.GetOrCreateTrial();

            AssertEqual("trial", state.Plan, "trial plan");
            AssertTrue(!string.IsNullOrEmpty(state.DeviceIdHash), "device hash");
            AssertTrue(state.OfflineGraceUntilUtc > DateTime.UtcNow, "offline grace");
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
