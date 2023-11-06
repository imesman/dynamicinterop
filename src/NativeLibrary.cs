//ReSharper disable all
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyModel;

#if !NET6_0_OR_GREATER
using System.Reflection;
#endif

namespace DynamicInterop
{
    /// <summary>
    /// A native library.
    /// </summary>
    public class NativeLibrary : IDisposable
    {
        #region Public Properties
        /// <summary>
        /// The pointer of the library.
        /// </summary>
        public IntPtr Pointer { get; protected set; }
        
        /// <summary>
        /// The resolved paths of the library.
        /// </summary>
        public List<OSPath> ResolvedPaths { get; protected set; }
        
        /// <summary>
        /// The active resolved path of the library.
        /// </summary>
        public string ActivePath
        {
            get { return ResolvedPaths[_activePath].Path; }
        }
        #endregion

        #region Private Fields
        /// <summary>
        /// The currently active resolved path.
        /// </summary>
        private int _activePath;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Create a NativeLibrary.
        /// </summary>
        protected NativeLibrary()
        {
            Pointer = IntPtr.Zero;
            ResolvedPaths = new List<OSPath>();
            _activePath = 0;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Create a native library for the current platform.
        /// </summary>
        /// <returns>A NativeLibrary class.</returns>
        /// <exception cref="NotSupportedException">The current operating system isn't supported!</exception>
        public static NativeLibrary Create()
        {
            if (Platform.OperatingSystem == OSPlatform.Windows)
                return new WindowsLibrary();
            else if (Platform.OperatingSystem == OSPlatform.OSX)
                return new OSXLibrary();
            else if (Platform.OperatingSystem == OSPlatform.Linux)
                return new LinuxLibrary();
            else throw Internal.PlatformNotSupported;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a path that will be used to load the library depending on the platform information provided.
        /// </summary>
        /// <param name="path">The path to be added.</param>
        /// <param name="platform">The provided platform information.</param>
        /// <param name="shouldThrow">Should an exception be thrown if the path cannot be found or resolved?</param>
        /// <param name="shouldBruteForce">Should the path be found using brute-force if all else fails?</param>
        /// <returns>Whether or not the path was added successfully.</returns>
        /// <exception cref="FileNotFoundException">The provided library path doesn't exist!</exception>
        public NativeLibrary AddPath(string path, Platform platform, bool shouldBruteForce = true, bool shouldThrow = true)
        {
            // Attempt to resolve the path if it can't be immediately found.
            if (!File.Exists(path))
            {
                // This array will be important for dependency checking later.
                string[] rids = new string[2];
                
                // First, we attempt to resolve the path using the first available RID.
                rids[0] = Platform.GetRID();
                string full_rid_path = System.IO.Path.Combine(rids[0], path);
                string full_rid_path_r = System.IO.Path.Combine("runtimes", full_rid_path);
                
                if (File.Exists(full_rid_path))
                {
                    path = full_rid_path;
                    goto resolved;
                }
                else if (File.Exists(full_rid_path_r))
                {
                    path = full_rid_path_r;
                    goto resolved;
                }
                
                // Then, we attempt to resolve the path using a guessed RID.
                rids[1] = Platform.GuessRID(platform);
                if (rids[1] != string.Empty)
                {
                    string guessed_rid_path = System.IO.Path.Combine(rids[1], path);
                    string guessed_rid_path_r = System.IO.Path.Combine("runtimes", guessed_rid_path);
                    
                    if (File.Exists(guessed_rid_path))
                    {
                        path = guessed_rid_path;
                        goto resolved;
                    }
                    else if (File.Exists(guessed_rid_path_r))
                    {
                        path = guessed_rid_path_r;
                        goto resolved;
                    }
                }
                
                // Then, we attempt to resolve the path using Windows-specific special folders.
                if (Platform.OperatingSystem == OSPlatform.Windows)
                {
                    string windowspath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder
                        .Windows), path);
                    if (File.Exists(windowspath))
                    {
                        path = windowspath;
                        goto resolved;
                    }
                    
                    string sys64path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder
                        .System), path);
                    if (File.Exists(sys64path))
                    {
                        path = sys64path;
                        goto resolved;
                    }
                    
                    string sys86path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder
                        .SystemX86), path);
                    if (File.Exists(sys86path))
                    {
                        path = sys86path;
                        goto resolved;
                    }
                }
                
                // Then, we attempt to resolve the path using the application's dependencies.
                DependencyContext context = DependencyContext.Default!;
                if (context != null)
                {
                    for (int i = 0; i < context.RuntimeLibraries.Count; i++)
                    {
                        string[] rid0assets = context.RuntimeLibraries[i].GetRuntimeNativeAssets(context, rids[0])
                            .ToArray();
                        string[] rid1assets = context.RuntimeLibraries[i].GetRuntimeNativeAssets(context, rids[1])
                            .ToArray();
                        string[] assets = new string[rid0assets.Length + rid1assets.Length];
                        rid0assets.CopyTo(assets, 0);
                        rid1assets.CopyTo(assets, rid0assets.Length);

                        for (int j = 0; j < assets.Length; j++)
                        {
                            if (Path.GetFileName(assets[j]) == path || Path.GetFileNameWithoutExtension(assets[j]) ==
                                path)
                            {
                                string localpath = Path.Combine(AppContext.BaseDirectory, assets[j]);
                                localpath = Path.GetFullPath(localpath);
                                if (File.Exists(localpath))
                                {
                                    path = localpath;
                                    goto resolved;
                                }
                                
                                string userdir = Platform.GetUserDirectory();
                                if (userdir != string.Empty)
                                {
                                    string rootnugetpath = Path.Combine(userdir, ".nuget", "packages", path);
                                    string nugetpath = Path.Combine(rootnugetpath, 
                                        context.RuntimeLibraries[j].Name.ToLowerInvariant(), 
                                        context.RuntimeLibraries[j].Version,
                                        assets[j]);
                                    nugetpath = Path.GetFullPath(nugetpath);
                                    
                                    if (File.Exists(nugetpath))
                                    {
                                        path = nugetpath;
                                        goto resolved;
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Finally, we attempt to resolve the path using brute force.
                if (shouldBruteForce)
                {
#if NET6_0_OR_GREATER
                    string processpath = Path.GetDirectoryName(Environment.ProcessPath)!;
#else
                    string processpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
#endif
                    if (processpath != null)
                    {
                        string[] files = Directory.GetFiles(processpath, "*.*", SearchOption.AllDirectories);
                        for (int i = 0; i < files.Length; i++)
                        {
                            if (Path.GetFileName(files[i]) == path)
                            {
                                path = files[i];
                                goto resolved;
                            }
                        }
                    }
                }
            }
            
            resolved:
            // If it still can't be found after resolving, then an exception must be thrown.
            if (!File.Exists(path))
            {
                if(shouldThrow)
                    throw Internal.LibraryNotFound;
                return this;
            }
            else ResolvedPaths.Add(new OSPath(path, platform));

            return this;
        }
        
        /// <summary>
        /// Adds a path that will be used to load the library depending on the platform information provided.
        /// </summary>
        /// <param name="path">The path to be added.</param>
        /// <param name="platform">The provided platform information.</param>
        /// <param name="shouldThrow">Should an exception be thrown if a path cannot be found or resolved?</param>
        /// <returns>Whether or not the path was added successfully.</returns>
        public NativeLibrary AddPath(string path, OSPlatform platform, bool shouldThrow = true)
        {
            Architecture[] architectures = Platform.GetSupportedArchitectures();
            bool success = false;
            for (int i = 0; i < architectures.Length; i++)
            {
                AddPath(path, new Platform(platform, architectures[i]), false, 
                    false); // Brute-forcing needs to be disabled so that binaries with the
                                        // same name that are built for differentbarchitectures don't get
                                        // intermixed.
                if (ResolvedPaths.Any())
                    success = true;
            }

            if(shouldThrow && !success)
                throw Internal.LibraryNotFound;
            return this;
        }
        #endregion
        
        #region Virtual Methods
        /// <summary>
        /// Loads the native library.
        /// </summary>
        /// <returns>The native library.</returns>
        /// <exception cref="NullReferenceException">"The provided path is empty!"</exception>
        /// <exception cref="FileNotFoundException">"The provided library path doesn't exist!"</exception>
        public virtual NativeLibrary Load()
        {
            string path = string.Empty;
            if (!ResolvedPaths.Any())
                throw Internal.PathEmpty;
            else
            {
                for (int i = 0; i < ResolvedPaths.Count; i++)
                {
                    if (ResolvedPaths[i].Platform == Platform.Current)
                    {
                        if (!File.Exists(ResolvedPaths[i].Path))
                            throw Internal.LibraryNotFound;
                        _activePath = i;
                    }
                }
            }
            
            return this;
        }

        /// <summary>
        /// Retrieves a function pointer as a delegate.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <returns>A function pointer as a delegate.</returns>
        /// <exception cref="NotSupportedException">Thrown if the method isn't implemented.</exception>
        public virtual T GetFunction<T>(string name) where T : Delegate
        {
            if (Pointer == IntPtr.Zero)
                throw Internal.LibraryNotLoaded;
            return default!;
        }

        /// <summary>
        /// Frees the native pointer of the library. Warning: if the native pointer is freed, all functions will stop working.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the method isn't implemented.</exception>
        public virtual void Free()
        {
            if (Pointer == IntPtr.Zero)
                return;
        }
        #endregion
        
        #region Overrides
        /// <summary>
        /// Disposes of the library. Warning: if the library is disposed, all functions will stop working.
        /// </summary>
        public void Dispose() => Free();
        #endregion
    }

    /// <summary>
    /// A Windows-specific native library.
    /// </summary>
    public sealed class WindowsLibrary : NativeLibrary
    {
        #region Overrides
        /// <summary>
        /// Loads the native library.
        /// </summary>
        /// <returns>The native library.</returns>
        public override NativeLibrary Load()
        {
            base.Load();
            Pointer = Windows.LoadLibrary(ActivePath);
            if (Pointer == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), Internal.LibraryNotLoaded.ToString());
            return this;
        }

        /// <summary>
        /// Retrieves a function pointer as a delegate.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <returns>A function pointer as a delegate.</returns>
        public override T GetFunction<T>(string name)
        {
            base.GetFunction<T>(name);
            return Marshal.GetDelegateForFunctionPointer<T>(Windows.GetProcAddress(
                Pointer, name));
        }

        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            base.Free();
            Windows.FreeLibrary(Pointer);
            Pointer = IntPtr.Zero;
        }
        #endregion
    }

    /// <summary>
    /// An OSX-specific native library.
    /// </summary>
    public sealed class OSXLibrary : NativeLibrary
    {
        #region Overrides
        /// <summary>
        /// Loads the native library.
        /// </summary>
        /// <returns>The native library.</returns>
        public override NativeLibrary Load()
        {
            base.Load();
            Pointer = MacOSX.dlopen(ActivePath, 2);
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException(MacOSX.dlerror(), Internal.LibraryNotLoaded);
            return this;
        }
        
        /// <summary>
        /// Retrieves a function pointer as a delegate.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <returns>A function pointer as a delegate.</returns>
        public override T GetFunction<T>(string name)
        {
            base.GetFunction<T>(name);
            return Marshal.GetDelegateForFunctionPointer<T>(MacOSX.dlsym(Pointer, name));
        }
        
        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            base.Free();
            MacOSX.dlclose(Pointer);
            Pointer = IntPtr.Zero;
        }
        #endregion
    }

    /// <summary>
    /// A Linux-specific native library.
    /// </summary>
    public sealed class LinuxLibrary : NativeLibrary
    {
        #region Overrides
        /// <summary>
        /// Loads the native library.
        /// </summary>
        /// <returns>The native library.</returns>
        public override NativeLibrary Load()
        {
            base.Load();
            Pointer = Linux.dlopen(ActivePath, 2);
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException(Linux.dlerror(), Internal.LibraryNotLoaded);
            return this;
        }

        /// <summary>
        /// Retrieves a function pointer as a delegate.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <returns>A function pointer as a delegate.</returns>
        public override T GetFunction<T>(string name)
        {
            base.GetFunction<T>(name);
            return Marshal.GetDelegateForFunctionPointer<T>(Linux.dlsym(Pointer, name));
        }

        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            base.Free();
            Linux.dlclose(Pointer);
            Pointer = IntPtr.Zero;
        }
        #endregion
    }
}