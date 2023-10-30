//ReSharper disable all
using System.IO;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    /// <summary>
    /// A file path targeting a specified platform and architecture.
    /// </summary>
    public struct OSPath
    {
        #region Public Properties
        /// <summary>
        /// The target platform of the path.
        /// </summary>
        public OSPlatform Platform { get; set; }
    
        /// <summary>
        /// The target architecture of the path.
        /// </summary>
        public Architecture Architecture { get; set; }
    
        /// <summary>
        /// The path.
        /// </summary>
        public string Path { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create an OSPath.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="platform">The target platform of the path.</param>
        /// <param name="architecture">The target architecture of the path.</param>
        public OSPath(string path, OSPlatform platform, Architecture architecture)
        {
            Path = path;
            Platform = platform;
            Architecture = architecture;
        }
        #endregion
    }
}