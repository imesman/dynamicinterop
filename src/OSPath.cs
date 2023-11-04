//ReSharper disable all
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    /// <summary>
    /// A file path targeting a specified Platform.
    /// </summary>
    public struct OSPath : IEquatable<OSPath>
    {
        #region Static Constructors
        /// <summary>
        /// An empty OSPath.
        /// </summary>
        public static OSPath Empty { get; } = new OSPath(string.Empty, Platform.Current);
        #endregion
        
        #region Public Properties
        /// <summary>
        /// The path.
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// The platform of the path.
        /// </summary>
        public Platform Platform { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create an OSPath.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="platform">The target platform of the path.</param>
        public OSPath(string path, Platform platform)
        {
            Path = path;
            Platform = platform;
        }
        #endregion

        #region Operators
        public static bool operator ==(OSPath o1, OSPath o2) => 
            (o1.Path == o2.Path && o1.Platform == o2.Platform);

        public static bool operator !=(OSPath o1, OSPath o2) => !(o1 == o2);
        #endregion

        #region Overrides
        /// <summary>
        /// Is the provided OSPath the same as this OSPath?
        /// </summary>
        /// <param name="other">The provided OSPath.</param>
        public bool Equals(OSPath other)
        {
            return Path == other.Path && Platform.Equals(other.Platform);
        }

        /// <summary>
        /// Is the provided object the same as this OSPath?
        /// </summary>
        /// <param name="obj">The provided object.</param>
        public override bool Equals(object? obj)
        {
            return obj is OSPath other && Equals(other);
        }

        /// <summary>
        /// Get the hash code of this OSPath.
        /// </summary>
        /// <returns>The hash code of this OSPath.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Platform);
        }
        #endregion
    }
}