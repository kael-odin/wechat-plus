using System;
using System.IO;

namespace WeChatPlus.Core.Services
{
    public sealed class DiagnosticsLogService
    {
        private readonly string _logPath;

        public DiagnosticsLogService(string dataRoot)
        {
            AppPaths.EnsureDirectory(dataRoot);
            _logPath = Path.Combine(dataRoot, "diagnostics.log");
        }

        public string LogPath
        {
            get { return _logPath; }
        }

        public string Write(string area, string message, Exception exception)
        {
            string line = DateTime.UtcNow.ToString("o") +
                " [" + Clean(area) + "] " +
                Clean(message);

            if (exception != null)
            {
                line += " | " + exception.GetType().Name + ": " + Clean(exception.Message);
            }

            File.AppendAllText(_logPath, line + Environment.NewLine);
            return _logPath;
        }

        public void ExportTo(string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                throw new ArgumentException("Missing diagnostics export path.", "destinationPath");
            }

            string directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
            {
                AppPaths.EnsureDirectory(directory);
            }

            if (!File.Exists(_logPath))
            {
                File.WriteAllText(_logPath, string.Empty);
            }

            File.Copy(_logPath, destinationPath, true);
        }

        private static string Clean(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace(Environment.NewLine, " ").Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
