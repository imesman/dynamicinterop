//ReSharper disable all
using System;
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
        public static Platform Current { get; } = new Platform(GetCurrentSystem(), RuntimeInformation.OSArchitecture);
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

        #region Static Methods
        /// <summary>
        /// Gets the current operating system.
        /// </summary>
        /// <returns>The current operating system.</returns>
        /// <exception cref="PlatformNotSupportedException">The current operating system isn't supported!</exception>
        public static OSPlatform GetCurrentSystem()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OSPlatform.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
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
            if (platform.System == OSPlatform.Windows)
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
            else if (platform.System == OSPlatform.OSX)
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
            else if (platform.System == OSPlatform.Linux)
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