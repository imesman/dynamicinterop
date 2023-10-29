//ReSharper disable all
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    /// <summary>
    /// A class that selects paths based on its target platform and architecture.
    /// </summary>
    public sealed class PathResolver
    {
        #region Private Properties
        /// <summary>
        /// The paths that need to be resolved.
        /// </summary>
        private List<OSPath> Paths { get; set; }
        #endregion
    
        #region Constructors
        /// <summary>
        /// Create a PathResolver.
        /// </summary>
        public PathResolver()
        {
            Paths = new List<OSPath>();
        }
        #endregion
    
        #region Public Methods
        /// <summary>
        /// Add a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="NullReferenceException">Thrown if the path is empty.</exception>
        public void Add(OSPath path)
        {
            if (path.Path == "" && path.Path == string.Empty)
                throw Internal.PathEmpty;
            if (!File.Exists(path.Path))
                throw Internal.LibraryNotFound;
    
            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Platform == path.Platform &&
                    Paths[i].Architecture == path.Architecture)
                    Paths.Remove(Paths[i]);
            }
    
            Paths.Add(path);
        }
    
        /// <summary>
        /// Selects the optimal path based on its target platform and architecture.
        /// </summary>
        /// <returns>The optimal path.</returns>
        public string Get()
        {
            for (int i = 0; i < Paths.Count; i++)
            {
                if (RuntimeInformation.IsOSPlatform(Paths[i].Platform) &&
                    RuntimeInformation.OSArchitecture == Paths[i].Architecture)
                    return Paths[i].Path;
            }
    
            return string.Empty;
        }
        #endregion
    }
}