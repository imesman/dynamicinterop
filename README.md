# DynamicInterop

A simple library for loading native libraries and retrieving function pointers as delegates. This allows a 
cross-platform library to be used without either having to rewrite DllImports for each platform or handle the 
library's path at compile-time.

DynamicInterop supports .NET Standard 2.1 and .NET 7. All examples use .NET 7.

### Examples

Below is an example of [loading a function from Kernel32](examples/Kernel32Example/Kernel32.cs):

```csharp
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
```
The `GetCurrentThreadId` field can then be called to execute the associated function.

### License
DynamicInterop is distributed under the very permissive [MIT/X11 license](LICENSE).