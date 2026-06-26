using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WeChatPlus.OpenHelper.MultiInstance
{
    internal static class MutexHandleCloser
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SystemHandleInformation
        {
            public ushort ProcessId;
            public ushort CreatorBackTrackIndex;
            public byte ObjectType;
            public byte HandleAttribute;
            public ushort Handle;
            public IntPtr ObjectPointer;
            public IntPtr AccessMask;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ObjectNameInformation
        {
            public UnicodeString Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UnicodeString
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            DuplicateHandle = 0x00000040,
            VirtualMemoryRead = 0x00000010
        }

        [DllImport("ntdll.dll")]
        private static extern uint NtQuerySystemInformation(int systemInformationClass, IntPtr systemInformation, int systemInformationLength, ref int returnLength);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryObject(IntPtr objectHandle, int objectInformationClass, IntPtr objectInformation, int objectInformationLength, ref int returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags desiredAccess, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DuplicateHandle(IntPtr sourceProcessHandle, IntPtr sourceHandle, IntPtr targetProcessHandle, out IntPtr targetHandle, uint desiredAccess, bool inheritHandle, uint options);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        private const uint StatusInfoLengthMismatch = 0xC0000004;
        private const int SystemHandleInformationClass = 0x10;
        private const int ObjectNameInformationClass = 1;
        private const int DuplicateCloseSource = 0x1;
        private const int DuplicateSameAccess = 0x2;
        private const string WeChatMutexMarker = "_Instance_Identity_Mutex_Name";

        public static int CloseWeChatMutexes(Process process)
        {
            if (process == null)
            {
                return 0;
            }

            int closed = 0;
            foreach (SystemHandleInformation handle in GetProcessHandles(process))
            {
                if (CloseIfWeChatMutex(handle, process))
                {
                    closed++;
                }
            }

            return closed;
        }

        private static IEnumerable<SystemHandleInformation> GetProcessHandles(Process process)
        {
            List<SystemHandleInformation> handles = new List<SystemHandleInformation>();
            int bufferLength = Marshal.SizeOf(typeof(SystemHandleInformation)) * 20000;
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocHGlobal(bufferLength);
                int returnLength = 0;
                while (NtQuerySystemInformation(SystemHandleInformationClass, buffer, bufferLength, ref returnLength) == StatusInfoLengthMismatch)
                {
                    Marshal.FreeHGlobal(buffer);
                    bufferLength = returnLength;
                    buffer = Marshal.AllocHGlobal(bufferLength);
                }

                long count = Marshal.ReadIntPtr(buffer).ToInt64();
                IntPtr current = new IntPtr(buffer.ToInt64() + IntPtr.Size);
                int itemSize = Marshal.SizeOf(typeof(SystemHandleInformation));

                for (long i = 0; i < count; i++)
                {
                    SystemHandleInformation info = (SystemHandleInformation)Marshal.PtrToStructure(current, typeof(SystemHandleInformation));
                    if (info.ProcessId == process.Id)
                    {
                        handles.Add(info);
                    }
                    current = new IntPtr(current.ToInt64() + itemSize);
                }
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            return handles;
        }

        private static bool CloseIfWeChatMutex(SystemHandleInformation handleInformation, Process process)
        {
            IntPtr duplicatedHandle = IntPtr.Zero;
            IntPtr processHandle = IntPtr.Zero;
            IntPtr objectNameBuffer = IntPtr.Zero;

            try
            {
                processHandle = OpenProcess(ProcessAccessFlags.DuplicateHandle | ProcessAccessFlags.VirtualMemoryRead, false, process.Id);
                if (processHandle == IntPtr.Zero)
                {
                    return false;
                }

                if (!DuplicateHandle(processHandle, new IntPtr(handleInformation.Handle), GetCurrentProcess(), out duplicatedHandle, 0, false, DuplicateSameAccess))
                {
                    return false;
                }

                int length = 0;
                objectNameBuffer = Marshal.AllocHGlobal(256 * 1024);
                uint queryResult = (uint)NtQueryObject(duplicatedHandle, ObjectNameInformationClass, objectNameBuffer, 0, ref length);
                while (queryResult == StatusInfoLengthMismatch)
                {
                    Marshal.FreeHGlobal(objectNameBuffer);
                    if (length <= 0)
                    {
                        return false;
                    }
                    objectNameBuffer = Marshal.AllocHGlobal(length);
                    queryResult = (uint)NtQueryObject(duplicatedHandle, ObjectNameInformationClass, objectNameBuffer, length, ref length);
                }

                ObjectNameInformation nameInfo = (ObjectNameInformation)Marshal.PtrToStructure(objectNameBuffer, typeof(ObjectNameInformation));
                if (nameInfo.Name.Buffer == IntPtr.Zero)
                {
                    return false;
                }

                string objectName = Marshal.PtrToStringUni(nameInfo.Name.Buffer);
                if (string.IsNullOrEmpty(objectName) || objectName.IndexOf(WeChatMutexMarker, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }

                IntPtr closedHandle;
                if (DuplicateHandle(processHandle, new IntPtr(handleInformation.Handle), GetCurrentProcess(), out closedHandle, 0, false, DuplicateCloseSource))
                {
                    CloseHandle(closedHandle);
                    return true;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (objectNameBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(objectNameBuffer);
                }
                if (duplicatedHandle != IntPtr.Zero)
                {
                    CloseHandle(duplicatedHandle);
                }
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
            }

            return false;
        }
    }
}
