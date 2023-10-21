//ReSharper disable all
namespace DynamicInterop;
using System.Runtime.InteropServices;

public struct OSPath
{
    #region Public Properties
    public OSPlatform Platform { get; set; }
    
    public Architecture Architecture { get; set; }
    
    public string Path { get; set; }
    #endregion

    #region Constructors
    public OSPath(OSPlatform platform, Architecture architecture, string path)
    {
        Platform = platform;
        Architecture = architecture;
        Path = path;
    }
    #endregion
}