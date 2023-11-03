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
        resolver.Add("glfw3.dll", OSPlatform.Windows);
        
        library.Load(resolver.Get());
            
        Init = library.GetFunction<glfwInit>("glfwInit");
    }
}