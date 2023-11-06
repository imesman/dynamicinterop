# DynamicInterop

A simple library for loading native libraries and retrieving function pointers as delegates. This allows a 
cross-platform library to be used without either having to rewrite DllImports for each platform or handle the 
library's path at compile-time.
For help getting started, check out the [usage guide](USAGE.md) and [examples](examples).

### Supported Frameworks & Platforms

DynamicInterop supports .NET Standard 2.1 and .NET 7. All examples support .NET 7 exclusively.

**Supported platforms**: 
* Windows
* OSX
* Linux

### Dependencies
* [Microsoft.Extensions.DependencyModel (7.0.0)](https://www.nuget.org/packages/Microsoft.Extensions.DependencyModel/7.0.0)
* [Microsoft.Win32.Registry (5.0.0)](https://www.nuget.org/packages/Microsoft.Win32.Registry/5.0.0)

### License & Acknowledgements
DynamicInterop is distributed under the very permissive [MIT/X11 license](LICENSE) and borrows source code from 
[nativelibraryloader](https://github.com/mellinoe/nativelibraryloader) and [Easy.Common](https://github.com/NimaAra/Easy.Common).