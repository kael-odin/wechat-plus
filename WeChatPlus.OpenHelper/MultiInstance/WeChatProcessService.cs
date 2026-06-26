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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindow(IntPtr hWnd);

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

        public bool CloseProcess(int processId)
        {
            if (processId <= 0)
            {
                return false;
            }

            try
            {
                Process process = Process.GetProcessById(processId);
                if (!string.Equals(process.ProcessName, "WeChat", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                process.Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FocusWindow(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return false;
            }
            return SetForegroundWindow(windowHandle);
        }

        public bool EmbedWindow(IntPtr windowHandle, IntPtr parentHandle, int width, int height)
        {
            if (!IsWindow(windowHandle) || !IsWindow(parentHandle))
            {
                return false;
            }

            IntPtr previousParent = SetParent(windowHandle, parentHandle);
            if (previousParent == IntPtr.Zero)
            {
                return false;
            }

            int targetWidth = width <= 0 ? 800 : width;
            int targetHeight = height <= 0 ? 600 : height;
            MoveWindow(windowHandle, 0, 0, targetWidth, targetHeight, true);
            SetForegroundWindow(windowHandle);
            return true;
        }

        public bool DetachWindow(IntPtr windowHandle)
        {
            if (!IsWindow(windowHandle))
            {
                return false;
            }

            SetParent(windowHandle, IntPtr.Zero);
            SetForegroundWindow(windowHandle);
            return true;
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
