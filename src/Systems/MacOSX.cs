//ReSharper disable all
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    internal static class MacOSX
    {
        #region Public Properties
        public static Architecture[] SupportedArchitectures { get; } = new[]
        {
            Architecture.X64, Architecture.Arm64,
        };
        #endregion
        
        #region Public Methods
        public static string Bash(string command)
        {
            string escapedArgs = command.Replace("\"", "\\\"");

            Process p = new Process()
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sh",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.Start();
            string result = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return result;
        }
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
}