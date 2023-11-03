//ReSharper disable all
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    internal static class Internal
    {
        #region Exceptions
        public static readonly FileNotFoundException LibraryNotFound = new FileNotFoundException(
            "The provided library path doesn't exist!");

        public static readonly NullReferenceException LibraryNotLoaded = new NullReferenceException(
            "The library has not been loaded!");
        
        public static readonly NullReferenceException PathEmpty = new NullReferenceException(
            "The provided path is empty!");
        #endregion

        #region Public Methods
        public static Architecture[] GetSupportedArchitectures()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Windows.SupportedArchitectures;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OSX.SupportedArchitectures;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Linux.SupportedArchitectures;
            else return new Architecture[] { };
        }
        #endregion
        
        #region Public Classes
        public static partial class Windows
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
            #endregion
        }
        
        public static partial class OSX
        {
            #region Public Properties
            public static Architecture[] SupportedArchitectures { get; } = new[]
            {
                Architecture.X64, Architecture.Arm64,
            };
            #endregion
            
            #region Imports
            [DllImport("libdl.dylib")]
            public static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("libdl.dylib")]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport("libdl.dylib")]
            public static extern int dlclose(IntPtr handle);
            #endregion
        }
        
        public partial class Linux
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
            #endregion
        }
        #endregion
    }
}