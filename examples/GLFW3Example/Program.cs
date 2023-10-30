//ReSharper disable all
using System.Runtime.InteropServices;
using DynamicInterop;
using NativeLibrary = DynamicInterop.NativeLibrary;

Console.WriteLine(Glfw.Init());

public static class Glfw
{
    public delegate bool glfwInit();
    public static glfwInit Init { get; }
    
    static Glfw()
    {
        NativeLibrary library = NativeLibrary.Create();
            PathResolver resolver = new PathResolver();
            resolver.Add(new OSPath(OSPlatform.Windows, Architecture.X64, "runtimes/win-x64/glfw3.dll"));
        
            library.Load(resolver.Get());
            
            Init = library.GetFunction<glfwInit>("glfwInit");
    }
}