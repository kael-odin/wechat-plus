namespace WeChatPlus.Core.Contracts
{
    public sealed class HelperRuntimeStatus
    {
        public bool HelperOk { get; set; }

        public string Command { get; set; }

        public string Message { get; set; }

        public int ProcessCount { get; set; }

        public string InstallPath { get; set; }
    }
}
