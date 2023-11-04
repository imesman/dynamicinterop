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
        library.AddPath("glfw.dll", OSPlatform.Windows);
        library.Load();
            
        Init = library.GetFunction<glfwInit>("glfwInit");
    }
}