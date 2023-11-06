//ReSharper disable all
using System;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    internal static class Windows
    {
        #region Public Properties
        public static Architecture[] SupportedArchitectures { get; } = new[]
        {
            Architecture.X86, Architecture.X64, 
            Architecture.Arm, Architecture.Arm64,
        };
        #endregion
            
        #region Imports
        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr module, string procName);

        [DllImport("kernel32")]
        public static extern int FreeLibrary(IntPtr module);
        
        [DllImport("kernel32.dll")]
        public static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);
        #endregion
    }
}