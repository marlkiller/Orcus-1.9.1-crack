using System;
using System.Runtime.InteropServices;

namespace Orcus.Shared.Utilities
{
    public class CoreMemoryApi
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memcpy(IntPtr dst, IntPtr src, UIntPtr count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe IntPtr memcpy(void* dst, void* src, UIntPtr count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memcmp(IntPtr ptr1, IntPtr ptr2, UIntPtr count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe IntPtr memcmp(void* ptr1, void* ptr2, UIntPtr count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memmove(IntPtr dest, IntPtr src, UIntPtr count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe IntPtr memmove(void* dest, void* src, UIntPtr count);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe IntPtr memset(void* dest, int c, UIntPtr n);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memset(IntPtr dest, int c, UIntPtr n);

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, IntPtr size);
    }
}