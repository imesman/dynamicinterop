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
    }
}