namespace WeChatPlus.Core.Contracts
{
    public sealed class HelperWindowInfo
    {
        public int ProcessId { get; set; }

        public string WindowHandle { get; set; }

        public string Title { get; set; }

        public bool HasWindow { get; set; }
    }
}
