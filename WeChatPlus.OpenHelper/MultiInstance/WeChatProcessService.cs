using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WeChatPlus.OpenHelper.MultiInstance
{
    public sealed class WeChatProcessService
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public int CountProcesses()
        {
            return Process.GetProcessesByName("WeChat").Length;
        }

        public Process[] GetProcesses()
        {
            return Process.GetProcessesByName("WeChat");
        }

        public string FindInstallPath()
        {
            string[] registryPaths =
            {
                @"SOFTWARE\Tencent\WeChat",
                @"SOFTWARE\WOW6432Node\Tencent\WeChat"
            };

            foreach (string registryPath in registryPaths)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    string installPath = ReadInstallPath(key);
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        return installPath;
                    }
                }

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    string installPath = ReadInstallPath(key);
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        return installPath;
                    }
                }
            }

            return string.Empty;
        }

        public int StartWeChat()
        {
            CloseAllMutexes();
            string installPath = FindInstallPath();
            string executable = string.IsNullOrEmpty(installPath)
                ? "WeChat.exe"
                : System.IO.Path.Combine(installPath, "WeChat.exe");

            Process process = Process.Start(executable);
            return process == null ? 0 : process.Id;
        }

        public bool CloseMutex(int processId)
        {
            Process process = Process.GetProcessById(processId);
            return MutexHandleCloser.CloseWeChatMutexes(process) > 0;
        }

        public int CloseAllMutexes()
        {
            int closed = 0;
            Process[] processes = GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                closed += MutexHandleCloser.CloseWeChatMutexes(processes[i]);
            }
            return closed;
        }

        public int CloseAllProcesses()
        {
            int closed = 0;
            Process[] processes = GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                try
                {
                    processes[i].Kill();
                    closed++;
                }
                catch
                {
                }
            }
            return closed;
        }

        public bool FocusWindow(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return false;
            }
            return SetForegroundWindow(windowHandle);
        }

        private static string ReadInstallPath(RegistryKey key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            object value = key.GetValue("InstallPath") ?? key.GetValue("InstallLocation");
            return value == null ? string.Empty : Convert.ToString(value);
        }
    }
}
