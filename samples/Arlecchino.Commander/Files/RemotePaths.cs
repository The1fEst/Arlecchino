using System;
using System.IO;

namespace Arlecchino.Commander.Files;

public static class RemotePaths
{
    public const string Root = "/";

    public static string Combine(string folder, string name) =>
        folder.EndsWith('/') ? folder + name : $"{folder}/{name}";

    public static string? Parent(string folder)
    {
        var trimmed = folder.TrimEnd('/');

        if (trimmed.Length == 0)
        {
            return null;
        }

        var cut = trimmed.LastIndexOf('/');

        return cut switch
        {
            < 0 => null,
            0 => Root,
            _ => trimmed[..cut],
        };
    }

    public static string NameOf(string path)
    {
        var trimmed = path.TrimEnd('/');
        var cut = trimmed.LastIndexOf('/');

        return cut < 0 ? trimmed : trimmed[(cut + 1)..];
    }

    public static bool IsHidden(string name) => name.StartsWith('.');

    public static IOException AsIoException(Exception error) => new(error.Message, error);
}
