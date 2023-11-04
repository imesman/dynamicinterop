//ReSharper disable all
namespace Kernel32Example;
using System.Runtime.InteropServices;
using DynamicInterop;
using NativeLibrary = DynamicInterop.NativeLibrary;

public static class Kernel32
{
    public delegate int Kernel32_GetCurrentThreadId();
    public static Kernel32_GetCurrentThreadId GetCurrentThreadId;
    
    static Kernel32()
    {
        NativeLibrary library = NativeLibrary.Create();
        library.AddPath("kernel32.dll", OSPlatform.Windows);
        library.Load();
            
        GetCurrentThreadId = library.GetFunction<Kernel32_GetCurrentThreadId>("GetCurrentThreadId");
    }
}
