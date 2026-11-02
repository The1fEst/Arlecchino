using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Arlecchino;

/// <summary>
/// The languages the person at the machine reads, read from the operating system whenever they are asked for.
/// <c>CultureInfo</c> is not used, since under <c>InvariantGlobalization</c> it knows no language at all.
/// </summary>
public static partial class SystemLanguages
{
    private const uint MuiLanguageName = 0x8;

    private static readonly string[] LocaleVariables = ["LC_ALL", "LC_MESSAGES", "LANG"];

    /// <summary>
    /// The user's interface languages, the one they prefer most first.
    /// </summary>
    /// <returns>
    /// Tags such as <c>ru-RU</c> or <c>en</c>, without duplicates, never the C or POSIX locale; empty when
    /// nothing is known.
    /// </returns>
    public static IReadOnlyList<string> GetPreferred() => OperatingSystem.IsWindows()
        ? FromWindows()
        : FromEnvironment(static name => Environment.GetEnvironmentVariable(name));

    internal static IReadOnlyList<string> FromEnvironment(Func<string, string?> variable)
    {
        var locale = LocaleVariables.Select(variable).FirstOrDefault(static value => !string.IsNullOrEmpty(value));

        var tag = Normalize(locale);
        if (tag is null)
        {
            return [];
        }

        var tags = new List<string>();

        foreach (var entry in (variable("LANGUAGE") ?? "").Split(':'))
        {
            Add(tags, Normalize(entry));
        }

        Add(tags, tag);

        return tags;
    }

    internal static IReadOnlyList<string> FromWindowsList(ReadOnlySpan<char> list)
    {
        var tags = new List<string>();

        while (!list.IsEmpty)
        {
            var end = list.IndexOf('\0');
            if (end <= 0)
            {
                break;
            }

            Add(tags, list[..end].ToString());
            list = list[(end + 1)..];
        }

        return tags;
    }

    private static string? Normalize(string? locale)
    {
        if (locale is null)
        {
            return null;
        }

        var end = locale.AsSpan().IndexOfAny('.', '@');
        var name = (end >= 0 ? locale[..end] : locale).Trim().Replace('_', '-');

        return name.Length == 0 ||
               name.Equals("C", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("POSIX", StringComparison.OrdinalIgnoreCase)
            ? null
            : name;
    }

    private static void Add(List<string> tags, string? tag)
    {
        if (tag is not null && !tags.Exists(other => other.Equals(tag, StringComparison.OrdinalIgnoreCase)))
        {
            tags.Add(tag);
        }
    }

    [SupportedOSPlatform("windows")]
    private static unsafe IReadOnlyList<string> FromWindows()
    {
        try
        {
            uint size = 0;
            if (!GetUserPreferredUILanguages(MuiLanguageName, out _, null, ref size) || size == 0)
            {
                return [];
            }

            var buffer = new char[size];

            fixed (char* start = buffer)
            {
                return GetUserPreferredUILanguages(MuiLanguageName, out _, start, ref size)
                    ? FromWindowsList(buffer.AsSpan(0, (int)size))
                    : [];
            }
        }
        catch (DllNotFoundException)
        {
            return [];
        }
        catch (EntryPointNotFoundException)
        {
            return [];
        }
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool GetUserPreferredUILanguages(
        uint flags,
        out uint languageCount,
        char* buffer,
        ref uint bufferSize);
}
