using Mediapipe.Net.Util;

namespace OTTO.Humanoid.Console.Mp;

/// <inheritdoc />
public class DummyResourceManager : ResourceManager
{
    /// <inheritdoc />
    public override PathResolver ResolvePath => (path) =>
    {
        System.Console.WriteLine($"PathResolver: (not) resolving path '{path}'");
        return path;
    };

    /// <inheritdoc />
    public override ResourceProvider ProvideResource => (path) =>
    {
        System.Console.WriteLine($"ResourceProvider: providing resource '{path}'");
        var bytes = File.ReadAllBytes(path);
        return bytes;
    };
}