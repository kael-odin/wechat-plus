using System;
using System.Collections.Generic;

namespace WeChatPlus.Core.Contracts
{
    public sealed class HelperCommand
    {
        private readonly Dictionary<string, string> _options;

        public HelperCommand(string area, string action, bool json, Dictionary<string, string> options)
        {
            Area = area ?? string.Empty;
            Action = action ?? string.Empty;
            Json = json;
            _options = options ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Area { get; private set; }

        public string Action { get; private set; }

        public bool Json { get; private set; }

        public string GetOption(string name)
        {
            string value;
            if (_options.TryGetValue(name, out value))
            {
                return value;
            }
            return string.Empty;
        }

        public IDictionary<string, string> Options
        {
            get { return _options; }
        }
    }
}
