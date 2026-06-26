using System;

namespace WeChatPlus.Core.Models
{
    public sealed class OpenSourceComponent
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string License { get; set; }

        public string SourceUrl { get; set; }

        public string BinaryPath { get; set; }

        public string Sha256 { get; set; }

        public DateTime InstalledAtUtc { get; set; }
    }
}
