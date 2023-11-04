//ReSharper disable all
using System.IO;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    /// <summary>
    /// A file path targeting a specified Platform.
    /// </summary>
    public struct OSPath
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
    }
}