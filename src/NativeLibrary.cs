//ReSharper disable all
using System;
using System.IO;
using System.Runtime.InteropServices;

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
        #endregion

        #region Constructors
        /// <summary>
        /// Create a NativeLibrary.
        /// </summary>
        protected NativeLibrary()
        {
            Pointer = IntPtr.Zero;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Create a native library for the current platform.
        /// </summary>
        /// <returns>A NativeLibrary class.</returns>
        /// <exception cref="NotSupportedException">Thrown if the current platform is not supported.</exception>
        public static NativeLibrary Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsLibrary();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new OSXLibrary();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new LinuxLibrary();
            else throw new NotSupportedException(RuntimeInformation.OSDescription);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads a native library.
        /// </summary>
        /// <param name="name">The name of the library.</param>
        /// <exception cref="NotSupportedException">Thrown if the method isn't implemented.</exception>
        public virtual NativeLibrary Load(string name)
        {
            if (!File.Exists(name))
                throw Internal.LibraryNotFound;
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
        /// Disposes of the native library.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the method isn't implemented.</exception>
        public virtual void Free()
        {
            if (Pointer == IntPtr.Zero)
                return;
        }
        #endregion

        #region Overrides
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
        /// Loads a native library.
        /// </summary>
        /// <param name="name">The name of the library.</param>
        public override NativeLibrary Load(string name)
        {
            base.Load(name);
            Pointer = Internal.Windows.LoadLibrary(name);
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
            return Marshal.GetDelegateForFunctionPointer<T>(Internal.Windows.GetProcAddress(
                Pointer, name));
        }

        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            base.Free();
            Internal.Windows.FreeLibrary(Pointer);
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
        /// Loads a native library.
        /// </summary>
        /// <param name="name">The name of the library.</param>
        public override NativeLibrary Load(string name)
        {
            base.Load(name);
            Pointer = Internal.OSX.dlopen(name, 2);
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
            return Marshal.GetDelegateForFunctionPointer<T>(Internal.OSX.dlsym(Pointer, name));
        }
        
        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            base.Free();
            Internal.OSX.dlclose(Pointer);
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
        /// Loads a native library.
        /// </summary>
        /// <param name="name">The name of the library.</param>
        public override NativeLibrary Load(string name)
        {
            base.Load(name);
            Pointer = Internal.Linux.dlopen(name, 2);
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
            return Marshal.GetDelegateForFunctionPointer<T>(Internal.Linux.dlsym(Pointer, name));
        }

        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            base.Free();
            Internal.Linux.dlclose(Pointer);
            Pointer = IntPtr.Zero;
        }
        #endregion
    }
}