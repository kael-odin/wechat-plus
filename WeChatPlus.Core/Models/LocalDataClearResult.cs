namespace WeChatPlus.Core.Models
{
    public sealed class LocalDataClearResult
    {
        public bool Ok { get; set; }

        public int RemovedEntries { get; set; }

        public string[] Errors { get; set; }

        public string SummaryText { get; set; }
    }
}
