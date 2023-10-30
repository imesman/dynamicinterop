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
        
        #region Public Classes
        public static partial class Windows
        {
            [DllImport("kernel32")]
            public static extern IntPtr LoadLibrary(string fileName);

            [DllImport("kernel32")]
            public static extern IntPtr GetProcAddress(IntPtr module, string procName);

            [DllImport("kernel32")]
            public static extern int FreeLibrary(IntPtr module);
        }
        
        public static partial class OSX
        {
            [DllImport("libdl.dylib")]
            public static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("libdl.dylib")]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport("libdl.dylib")]
            public static extern int dlclose(IntPtr handle);
        }
        
        public partial class Linux
        {
            [DllImport("libdl.so")]
            public static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("libdl.so")]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport("libdl.so")]
            public static extern int dlclose(IntPtr handle);
        }
        #endregion
    }
}