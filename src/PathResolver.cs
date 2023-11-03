//ReSharper disable all
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyModel;

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
        /// <param name="shouldResolve">Should the path be resolved?</param>
        public void Add(OSPath path, bool shouldResolve = true)
        {
            if (shouldResolve)
                path = Resolve(path);
    
            for (int i = 0; i < Paths.Count; i++)
            {
                if (Paths[i].Platform == path.Platform &&
                    Paths[i].Architecture == path.Architecture)
                    Paths.Remove(Paths[i]);
            }
    
            Paths.Add(path);
        }

        /// <summary>
        /// Add a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="platform">The target platform of the path.</param>
        /// <param name="architecture">The target architecture of the path.</param>
        /// <param name="shouldResolve">Should the path be resolved?</param>
        public void Add(string path, OSPlatform platform, Architecture architecture, bool shouldResolve = true) => 
            Add(new OSPath(path, platform, architecture), shouldResolve);

        /// <summary>
        /// Add a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="platform">The target platform of the path.</param>
        /// <param name="shouldResolve">Should the path be resolved?</param>
        public void Add(string path, OSPlatform platform, bool shouldResolve = true)
        {
            Architecture[] architectures = Internal.GetSupportedArchitectures();
            for (int i = 0; i < architectures.Length; i++)
            {
                OSPath ospath = new OSPath(path, platform, architectures[i]);
                if (shouldResolve)
                    ospath = Resolve(ospath, false, false);
                
                if(ospath != OSPath.Empty)
                    Add(ospath, false);
            }
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

        #region Private Methods
        /// <summary>
        /// Attempts to resolve a path that can't be immediately found.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="shouldThrow">Should an exception be thrown upon failure, or should an empty path be returned?</param>
        /// <param name="shouldBruteForce">Should the path be resolved by brute-forcing the executing directory if all
        /// other attempts fail?</param>
        /// <returns>The resolved path.</returns>
        /// <exception cref="NullReferenceException">"The provided path is empty!"</exception>
        /// <exception cref="FileNotFoundException">"The provided library path doesn't exist!"</exception>
        private static OSPath Resolve(OSPath path, bool shouldThrow = true, bool shouldBruteForce = true)
        {
            if (path.Path == "" || path.Path == string.Empty || path == OSPath.Empty)
                throw Internal.PathEmpty;
            if (!File.Exists(path.Path))
            {
                OSPath newpath = new OSPath(string.Empty, path.Platform, path.Architecture);

#if !NETSTANDARD
                // Attempt to resolve the path using RuntimeInformation.RuntimeIdentifier.
                string full_rid_path = Path.Combine(RuntimeInformation.RuntimeIdentifier, path.Path);
                string full_rid_path_r = Path.Combine("runtimes", full_rid_path);
                
                if (File.Exists(full_rid_path))
                    newpath.Path = full_rid_path;
                else if (File.Exists(full_rid_path_r))
                    newpath.Path = full_rid_path_r;          
#endif

                if (newpath.Path == string.Empty)
                {
                    string rid = GuessRID(path);
                    if (rid != string.Empty)
                    {
                        string guessed_rid_path = Path.Combine(rid, path.Path);
                        string guessed_rid_path_r = Path.Combine("runtimes", guessed_rid_path);
                
                        if(File.Exists(guessed_rid_path))
                            newpath.Path = guessed_rid_path;
                        if(File.Exists(guessed_rid_path_r))
                            newpath.Path = guessed_rid_path_r;   
                    }

                    if (newpath.Path == string.Empty)
                    {
                        string specialpath = ResolveSpecialFolders(path);
                        if(specialpath != string.Empty)
                            newpath.Path = specialpath;

                        if (newpath.Path == string.Empty)
                        {
                            string deppath = ResolveDependencies(path, new string[]
                            {
#if !NETSTANDARD
                                RuntimeInformation.RuntimeIdentifier,
#endif
                                rid
                            });
                            if(deppath != string.Empty)
                                newpath.Path = deppath;

                            if (newpath.Path == string.Empty && shouldBruteForce)
                            {
                                string brutepath = ResolveBruteForce(path);
                                if(brutepath != string.Empty)
                                    newpath.Path = brutepath;
                            }
                        }
                    }
                }

                if (newpath.Path == string.Empty)
                {
                    if(shouldThrow)
                        throw Internal.LibraryNotFound;
                    else return OSPath.Empty;
                }
                else return newpath;
            }

            return path;
        }

        /// <summary>
        /// Attempts to resolve a path using the application's dependencies.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="rids">An array of possible runtime identifiers.</param>
        /// <returns>The resolved path.</returns>
        private static string ResolveDependencies(OSPath path, string[] rids)
        {
            DependencyContext context = DependencyContext.Default!;
            
            if (context != null)
            {
                for (int i = 0; i < rids.Length; i++)
                {
                    for (int j = 0; j < context.RuntimeLibraries.Count; j++)
                    {
                        string[] nativeAssets = context.RuntimeLibraries[j].GetRuntimeNativeAssets(context, rids[i]).ToArray();
                        for (int k = 0; k < nativeAssets.Length; k++)
                        {
                            if (Path.GetFileName(nativeAssets[k]) == path.Path || Path.GetFileNameWithoutExtension
                                    (nativeAssets[k]) == path.Path)
                            {
                                string localpath = Path.Combine(AppContext.BaseDirectory, nativeAssets[k]);
                                localpath = Path.GetFullPath(localpath);
                                if (File.Exists(localpath))
                                    return localpath;

                                string userdir = GetUserDirectory();
                                if (userdir != string.Empty)
                                {
                                    string rootnugetpath = Path.Combine(userdir, ".nuget", "packages", path.Path);
                                    string nugetpath = Path.Combine(rootnugetpath, 
                                        context.RuntimeLibraries[j].Name.ToLowerInvariant(), 
                                        context.RuntimeLibraries[j].Version,
                                        nativeAssets[k]);
                                    nugetpath = Path.GetFullPath(nugetpath);
                                    
                                    if (File.Exists(nugetpath))
                                        return nugetpath;
                                }
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Attempts to resolve a path using Windows-specific special folders.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The resolved path.</returns>
        private static string ResolveSpecialFolders(OSPath path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string sys64path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), path.Path);
                if(File.Exists(sys64path))
                    return sys64path;
                    
                string sys86path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), path.Path);
                if(File.Exists(sys86path))
                    return sys86path;
                    
                string windowspath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), path.Path);
                if(File.Exists(windowspath))
                    return windowspath;
            }

            return string.Empty;
        }

        /// <summary>
        /// Attempts to resolve a path by brute-forcing the executing directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The resolved path.</returns>
        private static string ResolveBruteForce(OSPath path)
        {
#if !NETSTANDARD
            string processpath = Path.GetDirectoryName(Environment.ProcessPath)!;
#else
                string processpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
#endif
            if (processpath != null)
            {
                string[] files = Directory.GetFiles(processpath, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (Path.GetFileName(files[i]) == path.Path)
                        return files[i];
                }
            }

            return string.Empty;
        }
        
        /// <summary>
        /// Attempts to guess the runtime identifier based on the specified platform and architecture.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The runtime identifier that was guessed.</returns>
        private static string GuessRID(OSPath path)
        {
            string rid = string.Empty;
            if (path.Platform == OSPlatform.Windows)
            {
                switch (path.Architecture)
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
            else if (path.Platform == OSPlatform.OSX)
            {
                switch (path.Architecture)
                {
                    case Architecture.X64:
                        rid = "osx-x64";
                        break;
                    case Architecture.Arm64:
                        rid = "osx-arm64";
                        break;
                }
            }
            else if (path.Platform == OSPlatform.Linux)
            {
                switch (path.Architecture)
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
        private static string GetUserDirectory()
        {
            string dir = string.Empty;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                dir = Environment.GetEnvironmentVariable("USERPROFILE")!;
            else
                dir = Environment.GetEnvironmentVariable("HOME")!;

            return dir != null ? dir : string.Empty;
        }
        #endregion
    }
}