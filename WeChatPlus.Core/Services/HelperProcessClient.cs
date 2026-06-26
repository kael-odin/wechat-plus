using System;
using System.Diagnostics;
using System.Text;

namespace WeChatPlus.Core.Services
{
    public sealed class HelperProcessClient
    {
        private readonly string _helperPath;
        private readonly int _timeoutMilliseconds;

        public HelperProcessClient(string helperPath)
            : this(helperPath, 10000)
        {
        }

        public HelperProcessClient(string helperPath, int timeoutMilliseconds)
        {
            _helperPath = helperPath;
            _timeoutMilliseconds = timeoutMilliseconds;
        }

        public string Run(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = _helperPath;
            startInfo.Arguments = arguments ?? string.Empty;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.StandardErrorEncoding = Encoding.UTF8;

            using (Process process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Unable to start helper process.");
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                if (!process.WaitForExit(_timeoutMilliseconds))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                    }
                    throw new TimeoutException("Helper process timed out.");
                }

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(error.Length > 0 ? error : output);
                }

                return output;
            }
        }
    }
}
