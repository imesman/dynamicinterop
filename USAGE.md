# Usage Guide

To begin using DynamicInterop, you will need to [clone](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository) the [master](https://github.com/imesman/dynamicinterop/tree/master) branch 
and build the main project. 
The library is very simple to use, and anyone familiar with similar libraries like [nativelibraryloader](https://github.com/mellinoe/nativelibraryloader) 
should have no trouble getting started.

### Loading an unmanaged library

To load a library, you first have to instantiate an instance of the `NativeLibrary` class and pass the path of the unmanaged library you want to interop with:
```csharp
    NativeLibrary library = NativeLibrary.Create();
    library.Load("YOUR-LIBRARY");
```
> :warning: **DO NOT** wrap `NativeLibrary.Create()` in a using directive, this will result in a protected memory exception 
> when an interoped function is called. This is because the using directive will dispose of the `NativeLibrary`, thereby 
> freeing the library pointer.

Although the above approach will allow you to correctly interop with the unmanaged library, it is insufficient when you 
need to pass different paths depending on the platform and architecture. This can be done using the `PathResolver` class, 
as demonstrated below:

```csharp
    PathResolver resolver = new PathResolver();
    resolver.Add("YOUR-LIBRARY", OSPlatform.Windows, Architecture.X86);
    resolver.Add("YOUR-LIBRARY", OSPlatform.Windows, Architecture.X64);
```

The `OSPlatform` and `Architecture` arguments can be changed to any platform or architecture that is supported by .NET.
The unmanaged library can then be loaded as such:

```csharp
    library.Load(resolver.Get());
```

### Loading a function

Once an unmanaged library has been successfully loaded, a function can be loaded using the `GetFunction<T>()` method. 
However, first a delegate needs to be made that mirrors the unmanaged function you are attempting to load, 
as demonstrated below:

```csharp
    public delegate int YourFunctionDelegate(int arg1, int arg2);
    public static YourFunctionDelegate YourFunction { get; private set; }
```

The unmanaged function will need to be loaded into a variable of some kind, but it doesn't necessarily have to be a 
static field or property like above. It can simply be a variable if that is appropriate for your use-case. 
Also, the name of the delegate and object doesn't have to mirror the name of the unmanaged function. As long as you pass 
the same function name you would when using `[DllImport()]` into `GetFunction<T>()`, it will load successfully.
The unmanaged function can be loaded as such:

```csharp
    YourFunction = library.GetFunction<YourFunctionDelegate>("NAME_OF_UNMANAGED_FUNCTION");
```
