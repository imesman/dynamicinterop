//ReSharper disable all
namespace DynamicInterop;
using System.Runtime.InteropServices;

public sealed class PathResolver
{
    #region Private Properties
    private List<OSPath> Paths { get; set; }
    #endregion

    #region Constructors
    public PathResolver()
    {
        Paths = new List<OSPath>();
    }
    #endregion

    #region Public Methods
    public void Add(OSPath path)
    {
        if (path.Path == "" && path.Path == string.Empty)
            throw new NullReferenceException("OSPath is empty!");

        for (int i = 0; i < Paths.Count; i++)
        {
            if (Paths[i].Platform == path.Platform &&
                Paths[i].Architecture == path.Architecture)
                Paths.Remove(Paths[i]);
        }

        Paths.Add(path);
    }

    public string Get()
    {
        for (int i = 0; i < Paths.Count; i++)
        {
            if (RuntimeInformation.IsOSPlatform(Paths[i].Platform) &&
                RuntimeInformation.OSArchitecture == Paths[i].Architecture)
                return Paths[i].Path;
        }

        return string.Empty;
    }
    #endregion
}