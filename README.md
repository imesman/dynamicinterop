# DynamicInterop

A simple library for loading native libraries and retrieving function pointers as delegates. A use case for this 
functionality would be for using a cross-platform library without having to rewrite DllImport's for each 
platform or handle the library's path at compile-time.

DynamicInterop supports .NET Standard 2.1 and .NET 7.

### Usage

The preferred method for using DynamicInterop would be through a static class with  a constructor. Below is an 
example in which methods from Kernel32 are interoped:

```csharp
namespace ExampleBindings;
using System.Runtime.InteropServices;
using DynamicInterop;
using NativeLibrary = DynamicInterop.NativeLibrary;

public static class Kernel32
{
    public static PathResolver resolver;
    public static NativeLibrary library;
    
    public delegate int Kernel32_GetCurrentThreadId();
    public static Kernel32_GetCurrentThreadId GetCurrentThreadId;
    
    static Kernel32()
    {
        resolver = new PathResolver();
        resolver.Add(new OSPath(OSPlatform.Windows, Architecture.X86, "kernel32"));
        resolver.Add(new OSPath(OSPlatform.Windows, Architecture.X64, "kernel32"));

        library = NativeLibrary.Create();
        library.Load(resolver.Get());

        GetCurrentThreadId = library.GetFunction<Kernel32_GetCurrentThreadId>("GetCurrentThreadId");
    }
}
```
`GetCurrentThreadId` can then be called to execute the interoped function in Kernel32.

### License
DynamicInterop is distributed under the very permissive [MIT/X11 license](LICENSE).