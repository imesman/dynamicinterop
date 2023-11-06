//ReSharper disable all
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    /// <summary>
    /// A struct representing an operating system and its architecture.
    /// </summary>
    public struct Platform : IEquatable<Platform>
    {
        #region Static Properties
        /// <summary>
        /// The current Platform.
        /// </summary>
        public static Platform Current { get; internal set; }
        
        /// <summary>
        /// The current operating system.
        /// </summary>
        public static OSPlatform OperatingSystem { get; internal set; }
        #endregion
        
        #region Public Properties
        /// <summary>
        /// The operating system.
        /// </summary>
        public OSPlatform System { get; set; }
        
        /// <summary>
        /// The architecture of the operating system.
        /// </summary>
        public Architecture Architecture { get; set; }
        #endregion

        #region Static Constructors
        static Platform()
        {
            OperatingSystem = GetCurrentSystem();
            Current = new Platform(OperatingSystem, RuntimeInformation.OSArchitecture);
        }
        #endregion
        
        #region Constructors
        /// <summary>
        /// Create a Platform.
        /// </summary>
        /// <param name="system">The operating system.</param>
        /// <param name="architecture">The architecture of the operating system.</param>
        public Platform(OSPlatform system, Architecture architecture)
        {
            System = system;
            Architecture = architecture;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Is this Platform the current Platform?
        /// </summary>
        /// <returns>Whether or not this Platform is the current Platform.</returns>
        public bool Is() => this == Current;
        #endregion
        
        #region Static Methods
        /// <summary>
        /// Gets the current operating system.
        /// </summary>
        /// <returns>The current operating system.</returns>
        /// <exception cref="PlatformNotSupportedException">The current operating system isn't supported!</exception>
        public static OSPlatform GetCurrentSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OSPlatform.Windows;
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OSPlatform.OSX;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OSPlatform.Linux;
            else throw Internal.PlatformNotSupported;
        }

        /// <summary>
        /// Get the current runtime identifier.
        /// </summary>
        /// <returns>The current runtime identifier.</returns>
        public static string GetRID()
        {
#if NET5_0_OR_GREATER
            return RuntimeInformation.RuntimeIdentifier;
#else
            return GuessRID(Platform.Current);
#endif
        }
        
        /// <summary>
        /// Attempts to guess the runtime identifier based on the provided Platform.
        /// </summary>
        /// <param name="platform">The provided Platform.</param>
        /// <returns>The runtime identifier that was guessed.</returns>
        public static string GuessRID(Platform platform)
        {
            string rid = string.Empty;
            if (OperatingSystem == OSPlatform.Windows)
            {
                switch (platform.Architecture)
                {
                    case Architecture.X64:
                        rid = "win-x64";
                        break;
                    case Architecture.X86:
                        rid = "win-x86";
                        break;
                    case Architecture.Arm:
                        rid = "win-arm";
                        break;
                    case Architecture.Arm64:
                        rid = "win-arm64";
                        break;
                }
            }
            else if (OperatingSystem == OSPlatform.OSX)
            {
                switch (platform.Architecture)
                {
                    case Architecture.X64:
                        rid = "osx-x64";
                        break;
                    case Architecture.Arm64:
                        rid = "osx-arm64";
                        break;
                }
            }
            else if (OperatingSystem == OSPlatform.Linux)
            {
                switch (platform.Architecture)
                {
                    case Architecture.X64:
                        rid = "linux-x64";
                        break;
                    case Architecture.X86:
                        rid = "linux-x86";
                        break;
                    case Architecture.Arm:
                        rid = "linux-arm";
                        break;
                    case Architecture.Arm64:
                        rid = "linux-arm64";
                        break;
                }
            }

            return rid;
        }
        
        /// <summary>
        /// Retrieves the user directory.
        /// </summary>
        /// <returns>The user directory.</returns>
        public static string GetUserDirectory()
        {
            string dir = string.Empty;
            
            if (OperatingSystem == OSPlatform.Windows)
                dir = Environment.GetEnvironmentVariable("USERPROFILE")!;
            else
                dir = Environment.GetEnvironmentVariable("HOME")!;

            return dir != null ? dir : string.Empty;
        }
        
        /// <summary>
        /// Gets the supported architectures of this operating system.
        /// </summary>
        /// <returns>The supported architectures of this operating system.</returns>
        public static Architecture[] GetSupportedArchitectures()
        {
            if (OperatingSystem == OSPlatform.Windows)
                return Windows.SupportedArchitectures;
            else if (OperatingSystem == OSPlatform.OSX)
                return MacOSX.SupportedArchitectures;
            else if (OperatingSystem == OSPlatform.Linux)
                return Linux.SupportedArchitectures;
            else return new Architecture[] { };
        }

        /// <summary>
        /// Gets the full name of the CPU.
        /// </summary>
        /// <returns>The full name of the CPU.</returns>
        /// <exception cref="PlatformNotSupportedException">The current operating system isn't supported!</exception>
        public static string GetProcessorName()
        {
            if (OperatingSystem == OSPlatform.Windows)
            {
#pragma warning disable CA1416
                return Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"HARDWARE\DESCRIPTION\System\CentralProcessor\0\")?.GetValue(
                    "ProcessorNameString")?.ToString() ?? "Not Found";
#pragma warning restore CA1416
            }
            else if (OperatingSystem == OSPlatform.OSX)
                return MacOSX.Bash("sysctl -n machdep.cpu.brand_string").TrimEnd();
            else if (OperatingSystem == OSPlatform.Linux)
            {
                const string cpufile = "/proc/cpuinfo";

                string cpuline = File.ReadLines(cpufile).FirstOrDefault(l => l.StartsWith("model name",
                    StringComparison.InvariantCultureIgnoreCase))!;
                if (cpuline == null)
                    return "UNKNOWN";

                const string sep = ": ";
                int startIdx = cpuline.IndexOf(sep, StringComparison.Ordinal) + sep.Length;
                return cpuline.Substring(startIdx, cpuline.Length - startIdx);
            }
            else throw Internal.PlatformNotSupported;
        }

        /// <summary>
        /// Get the system's installed memory in gigabytes.
        /// </summary>
        /// <returns>The system's installed memory in gigabytes.</returns>
        /// <exception cref="PlatformNotSupportedException">The current operating system isn't supported!</exception>
        public static long GetInstalledMemory()
        {
            if (OperatingSystem == OSPlatform.Windows)
            {
                Windows.GetPhysicallyInstalledSystemMemory(out long tmkb);
                return (tmkb / 1024) / 1024;
            }
            else if (OperatingSystem == OSPlatform.OSX)
                return long.Parse(MacOSX.Bash("sysctl -n hw.memsize")) / 1000_000_000;
            else if (OperatingSystem == OSPlatform.Linux)
            {
                const string memoryfile = "/proc/meminfo";
                string memoryline = File.ReadLines(memoryfile).FirstOrDefault(l => l.StartsWith("MemTotal:", 
                    StringComparison.InvariantCultureIgnoreCase))!;

                if (memoryline == null)
                    return -1;

                const string beginsep = ":";
                const string endsep = "kB";
                int startIdx = memoryline.IndexOf(beginsep, StringComparison.Ordinal) + beginsep.Length;
                int endIdx = memoryline.IndexOf(endsep, StringComparison.Ordinal);
                string memStr = memoryline.Substring(startIdx, endIdx - startIdx);
                return long.Parse(memStr) / 1000_000;
            }
            else throw Internal.PlatformNotSupported;
        }
        #endregion
        
        #region Operators
        public static bool operator ==(Platform p1, Platform p2) => 
            (p1.System == p2.System && p1.Architecture == p2.Architecture && 
             p1.Architecture == p2.Architecture);
        
        public static bool operator !=(Platform p1, Platform p2) => !(p1 == p2);
        #endregion

        #region Overrides
        /// <summary>
        /// Is the provided Platform the same as this Platform?
        /// </summary>
        /// <param name="other">The provided Platform.</param>
        public bool Equals(Platform other)
        {
            return System.Equals(other.System) && Architecture == other.Architecture;
        }

        /// <summary>
        /// Is the provided object the same as this Platform?
        /// </summary>
        /// <param name="obj">The provided object.</param>
        public override bool Equals(object? obj)
        {
            return obj is Platform other && Equals(other);
        }

        /// <summary>
        /// Get the hash code of this Platform.
        /// </summary>
        /// <returns>The hash code of this Platform.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(System, (int)Architecture);
        }
        #endregion
    }
}