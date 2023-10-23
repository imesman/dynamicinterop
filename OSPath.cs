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
        /// <param name="platform">The target platform of the path.</param>
        /// <param name="architecture">The target architecture of the path.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="FileNotFoundException">Thrown if the provided file path doesn't exist.</exception>
        public OSPath(OSPlatform platform, Architecture architecture, string path)
        {
            Platform = platform;
            Architecture = architecture;

            if (!File.Exists(path))
                throw new FileNotFoundException("The provided file path doesn't exist.");
            Path = path;
        }
        #endregion
    }
}