using System;
using System.Collections.Generic;

namespace WeChatPlus.Core.Contracts
{
    public static class HelperCommandParser
    {
        public static HelperCommand Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return new HelperCommand("help", string.Empty, true, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }

            string area = args[0] ?? string.Empty;
            string action = string.Empty;
            int index = 1;
            bool json = false;
            Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (index < args.Length && !IsOption(args[index]))
            {
                action = args[index] ?? string.Empty;
                index++;
            }

            while (index < args.Length)
            {
                string token = args[index] ?? string.Empty;
                if (!IsOption(token))
                {
                    throw new ArgumentException("Unexpected argument: " + token);
                }

                string name = token.Substring(2);
                if (string.Equals(name, "json", StringComparison.OrdinalIgnoreCase))
                {
                    json = true;
                    options[name] = "true";
                    index++;
                    continue;
                }

                string value = "true";
                if (index + 1 < args.Length && !IsOption(args[index + 1]))
                {
                    value = args[index + 1];
                    index += 2;
                }
                else
                {
                    index++;
                }
                options[name] = value;
            }

            return new HelperCommand(area, action, json, options);
        }

        private static bool IsOption(string token)
        {
            return !string.IsNullOrEmpty(token) && token.StartsWith("--", StringComparison.Ordinal);
        }
    }
}
