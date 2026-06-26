using System;
using System.IO;
using System.Web.Script.Serialization;
using WeChatPlus.Core.Models;

namespace WeChatPlus.Core.Services
{
    public sealed class ComponentRepository
    {
        private readonly string _dataRoot;
        private readonly string _componentsPath;
        private readonly JavaScriptSerializer _serializer;

        public ComponentRepository(string dataRoot)
        {
            _dataRoot = dataRoot;
            _componentsPath = Path.Combine(_dataRoot, "components.json");
            _serializer = new JavaScriptSerializer();
        }

        public OpenSourceComponent[] GetAll()
        {
            EnsureSeedData();
            string json = File.ReadAllText(_componentsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new OpenSourceComponent[0];
            }

            return _serializer.Deserialize<OpenSourceComponent[]>(json) ?? new OpenSourceComponent[0];
        }

        public void EnsureSeedData()
        {
            AppPaths.EnsureDirectory(_dataRoot);
            if (File.Exists(_componentsPath))
            {
                return;
            }

            OpenSourceComponent helper = new OpenSourceComponent();
            helper.Id = "wechat-plus-open-helper";
            helper.Name = "WeChatPlus.OpenHelper";
            helper.Version = "0.1.0";
            helper.License = "GPLv3";
            helper.SourceUrl = "https://github.com/huiyadanli/RevokeMsgPatcher";
            helper.BinaryPath = "WeChatPlus.OpenHelper.exe";
            helper.Sha256 = string.Empty;
            helper.InstalledAtUtc = DateTime.UtcNow;

            File.WriteAllText(_componentsPath, _serializer.Serialize(new[] { helper }));
        }
    }
}
