//ReSharper disable all
using System;
using System.IO;
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
        
        public static readonly PlatformNotSupportedException PlatformNotSupported = new PlatformNotSupportedException(
            "The current operating system isn't supported!", new Exception(RuntimeInformation.OSDescription));
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the supported architectures of this operating system.
        /// </summary>
        /// <returns>The supported architectures of this operating system.</returns>
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
        
        /// <summary>
        /// Retrieves the user directory.
        /// </summary>
        /// <returns>The user directory.</returns>
        public static string GetUserDirectory()
        {
            string dir = string.Empty;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                dir = Environment.GetEnvironmentVariable("USERPROFILE")!;
            else
                dir = Environment.GetEnvironmentVariable("HOME")!;

            return dir != null ? dir : string.Empty;
        }
        #endregion
        
        #region Platform-specific Classes
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
            
            [DllImport("libdl.dylib")]
            public static extern string dlerror();
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
            
            [DllImport("libdl.so")]
            public static extern string dlerror();
            #endregion
        }
        #endregion
    }
}