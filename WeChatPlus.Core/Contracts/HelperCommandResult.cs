using System.Collections.Generic;

namespace WeChatPlus.Core.Contracts
{
    public sealed class HelperCommandResult
    {
        public HelperCommandResult()
        {
            Data = new Dictionary<string, object>();
        }

        public bool Ok { get; set; }

        public string Command { get; set; }

        public string Message { get; set; }

        public Dictionary<string, object> Data { get; private set; }

        public static HelperCommandResult Success(string command)
        {
            return new HelperCommandResult
            {
                Ok = true,
                Command = command,
                Message = string.Empty
            };
        }

        public static HelperCommandResult Failure(string command, string message)
        {
            return new HelperCommandResult
            {
                Ok = false,
                Command = command,
                Message = message ?? string.Empty
            };
        }
    }
}
