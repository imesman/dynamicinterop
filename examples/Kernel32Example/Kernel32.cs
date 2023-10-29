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
        using (NativeLibrary library = NativeLibrary.Create()) 
        {
            PathResolver resolver = new PathResolver();
            resolver.Add(new OSPath(OSPlatform.Windows, Architecture.X86, "kernel32"));
            resolver.Add(new OSPath(OSPlatform.Windows, Architecture.X64, "kernel32"));
        
            library.Load(resolver.Get());
            
            GetCurrentThreadId = library.GetFunction<Kernel32_GetCurrentThreadId>("GetCurrentThreadId");
        }
    }
}