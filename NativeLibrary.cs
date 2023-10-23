//ReSharper disable all
using System;
using System.Runtime.InteropServices;

namespace DynamicInterop
{
    /// <summary>
    /// A native library.
    /// </summary>
    public class NativeLibrary
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
        public virtual void Load(string name)
        {
            throw new NotSupportedException();
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
            throw new NotSupportedException();
        }

        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the method isn't implemented.</exception>
        public virtual void Free()
        {
            throw new NotSupportedException();
        }
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
        public override void Load(string name)
        {
            Pointer = Kernel32.LoadLibrary(name);
        }

        /// <summary>
        /// Retrieves a function pointer as a delegate.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <returns>A function pointer as a delegate.</returns>
        public override T GetFunction<T>(string name)
        {
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException("A native library hasn't been loaded!");
            return Marshal.GetDelegateForFunctionPointer<T>(Kernel32.GetProcAddress(Pointer, name));
        }

        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException("A native library hasn't been loaded!");
            Kernel32.FreeLibrary(Pointer);
            Pointer = IntPtr.Zero;
        }
        #endregion

        #region Private Classes
        private static class Kernel32
        {
            [DllImport("kernel32")]
            public static extern IntPtr LoadLibrary(string fileName);

            [DllImport("kernel32")]
            public static extern IntPtr GetProcAddress(IntPtr module, string procName);

            [DllImport("kernel32")]
            public static extern int FreeLibrary(IntPtr module);
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
        public override void Load(string name)
        {
            Pointer = Libdl.dlopen(name, 2);
        }
        
        /// <summary>
        /// Retrieves a function pointer as a delegate.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <returns>A function pointer as a delegate.</returns>
        public override T GetFunction<T>(string name)
        {
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException("A native library hasn't been loaded!");
            return Marshal.GetDelegateForFunctionPointer<T>(Libdl.dlsym(Pointer, name));
        }
        
        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException("A native library hasn't been loaded!");
            Libdl.dlclose(Pointer);
            Pointer = IntPtr.Zero;
        }
        #endregion

        #region Private Classes
        private static class Libdl
        {
            [DllImport("libdl.dylib")]
            public static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("libdl.dylib")]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport("libdl.dylib")]
            public static extern int dlclose(IntPtr handle);
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
        public override void Load(string name)
        {
            Pointer = Libdl.dlopen(name, 2);
        }

        /// <summary>
        /// Retrieves a function pointer as a delegate.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <typeparam name="T">The delegate type.</typeparam>
        /// <returns>A function pointer as a delegate.</returns>
        public override T GetFunction<T>(string name)
        {
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException("A native library hasn't been loaded!");
            return Marshal.GetDelegateForFunctionPointer<T>(Libdl.dlsym(Pointer, name));
        }

        /// <summary>
        /// Disposes of the native library.
        /// </summary>
        public override void Free()
        {
            if (Pointer == IntPtr.Zero)
                throw new NullReferenceException("A native library hasn't been loaded!");
            Libdl.dlclose(Pointer);
            Pointer = IntPtr.Zero;
        }

        #endregion
        
        #region Private Classes
        private static class Libdl
        {
            [DllImport("libdl.so")]
            public static extern IntPtr dlopen(string fileName, int flags);

            [DllImport("libdl.so")]
            public static extern IntPtr dlsym(IntPtr handle, string name);

            [DllImport("libdl.so")]
            public static extern int dlclose(IntPtr handle);
        }
        #endregion
    }
}