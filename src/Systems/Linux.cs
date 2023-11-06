//ReSharper disable all
using System;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    internal static class Linux
    {
        #region Public Properties
        public static Architecture[] SupportedArchitectures { get; } = new[]
        {
            Architecture.X86, Architecture.X64, 
            Architecture.Arm, Architecture.Arm64,
        };
        #endregion
            
        #region Imports
        [DllImport("libdl.so")]
        public static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl.so")]
        public static extern IntPtr dlsym(IntPtr handle, string name);

        [DllImport("libdl.so")]
        public static extern int dlclose(IntPtr handle);
            
        [DllImport("libdl.so")]
        public static extern string dlerror();
        #endregion
    }
}