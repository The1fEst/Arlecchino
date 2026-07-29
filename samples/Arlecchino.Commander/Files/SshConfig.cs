using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Arlecchino.Commander.Model;

namespace Arlecchino.Commander.Files;

public sealed record SshHost(string Alias, string HostName, string User, int Port, string KeyFile)
{
    public Connection AsConnection(string folder) =>
        new(Protocol.Sftp, HostName, Port, User, "", folder, KeyFile);

    public string Describe() => Port == Connection.PortFor(Protocol.Sftp)
        ? $"{Alias}  ({User}@{HostName})"
        : $"{Alias}  ({User}@{HostName}:{Port})";
}

public static class SshConfig
{
    private const string DefaultsPattern = "*";

    public static string Location => Path.Combine(Listing.Home(), ".ssh", "config");

    public static IReadOnlyList<SshHost> Hosts()
    {
        if (!File.Exists(Location))
        {
            return [];
        }

        try
        {
            return Read(File.ReadAllLines(Location));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    public static IReadOnlyList<SshHost> Read(IReadOnlyList<string> lines)
    {
        var hosts = new List<Draft>();
        var defaults = new Draft(DefaultsPattern);
        var current = defaults;

        foreach (var line in lines)
        {
            var (keyword, value) = Split(line);

            if (keyword.Length == 0)
            {
                continue;
            }

            if (keyword.Equals("host", StringComparison.OrdinalIgnoreCase))
            {
                current = new(value.Split(' ', StringSplitOptions.RemoveEmptyEntries) is [var first, ..]
                    ? first
                    : DefaultsPattern);

                if (!IsPattern(current.Alias))
                {
                    hosts.Add(current);
                }

                continue;
            }

            Take(current, keyword, value);
        }

        var found = new List<SshHost>(hosts.Count);

        foreach (var draft in hosts)
        {
            found.Add(new(
                draft.Alias,
                draft.HostName.Length > 0 ? draft.HostName : draft.Alias,
                Pick(draft.User, defaults.User, Environment.UserName),
                draft.Port > 0 ? draft.Port : defaults.Port > 0 ? defaults.Port : Connection.PortFor(Protocol.Sftp),
                Expand(Pick(draft.KeyFile, defaults.KeyFile, ""))));
        }

        return found;
    }

    private static void Take(Draft draft, string keyword, string value)
    {
        switch (keyword.ToLowerInvariant())
        {
            case "hostname":
                draft.HostName = value;
                break;
            case "user":
                draft.User = value;
                break;
            case "port":
                draft.Port = int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var port)
                    ? port
                    : 0;
                break;
            case "identityfile":
                draft.KeyFile = value.Trim('"');
                break;
        }
    }

    private static (string Keyword, string Value) Split(string line)
    {
        var trimmed = line.Trim();

        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
        {
            return ("", "");
        }

        var cut = trimmed.IndexOfAny([' ', '\t', '=']);

        return cut < 0 ? (trimmed, "") : (trimmed[..cut], trimmed[(cut + 1)..].Trim().Trim('='));
    }

    private static string Expand(string path)
    {
        if (path.Length == 0)
        {
            return "";
        }

        var expanded = Environment.ExpandEnvironmentVariables(path);

        return expanded.StartsWith('~')
            ? Path.Combine(Listing.Home(), expanded[1..].TrimStart('/', '\\'))
            : expanded;
    }

    private static string Pick(string first, string second, string fallback) =>
        first.Length > 0 ? first : second.Length > 0 ? second : fallback;

    private static bool IsPattern(string alias) =>
        alias.Contains('*', StringComparison.Ordinal) || alias.Contains('?', StringComparison.Ordinal);

    private sealed class Draft
    {
        public Draft(string alias) => Alias = alias;

        public string Alias { get; }

        public string HostName { get; set; } = "";

        public string User { get; set; } = "";

        public int Port { get; set; }

        public string KeyFile { get; set; } = "";
    }
}
