//ReSharper disable all
namespace DynamicInterop;
using System.Runtime.InteropServices;

public class NativeLibrary
{
    #region Public Properties
    public IntPtr Pointer { get; protected set; }
    #endregion

    #region Constructors
    protected NativeLibrary()
    {
        Pointer = IntPtr.Zero;
    }
    #endregion

    #region Static Methods
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
    public virtual void Load(string name)
    {
        throw new NotSupportedException();
    }

    public virtual T GetFunction<T>(string name) where T : Delegate
    {
        throw new NotSupportedException();
    }

    public virtual void Free()
    {
        throw new NotSupportedException();
    }
    #endregion
}

public sealed class WindowsLibrary : NativeLibrary
{
    #region Overrides
    public override void Load(string name)
    {
        Pointer = Kernel32.LoadLibrary(name);
    }

    public override T GetFunction<T>(string name)
    {
        if (Pointer == IntPtr.Zero)
            throw new NullReferenceException("A native library hasn't been loaded!");
        return Marshal.GetDelegateForFunctionPointer<T>(Kernel32.GetProcAddress(Pointer, name));
    }

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

public sealed class OSXLibrary : NativeLibrary
{
    #region Overrides
    public override void Load(string name)
    {
        Pointer = Libdl.dlopen(name, 2);
    }

    public override T GetFunction<T>(string name)
    {
        if (Pointer == IntPtr.Zero)
            throw new NullReferenceException("A native library hasn't been loaded!");
        return Marshal.GetDelegateForFunctionPointer<T>(Libdl.dlsym(Pointer, name));
    }

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

public sealed class LinuxLibrary : NativeLibrary
{
    #region Overrides
    public override void Load(string name)
    {
        Pointer = Libdl.dlopen(name, 2);
    }

    public override T GetFunction<T>(string name)
    {
        if (Pointer == IntPtr.Zero)
            throw new NullReferenceException("A native library hasn't been loaded!");
        return Marshal.GetDelegateForFunctionPointer<T>(Libdl.dlsym(Pointer, name));
    }

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