namespace WeChatPlus.Core.Models
{
    public sealed class DiagnosticsPackageResult
    {
        public bool Created { get; set; }

        public string PackageDirectory { get; set; }

        public string ReportPath { get; set; }

        public string LogPath { get; set; }
    }
}
