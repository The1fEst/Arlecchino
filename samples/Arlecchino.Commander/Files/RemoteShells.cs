using System;

namespace Arlecchino.Commander.Files;

/// <summary>What answers commands on the far side of an SSH connection.</summary>
public enum RemoteShellKind
{
    /// <summary>Not asked yet.</summary>
    Unknown,

    /// <summary>A POSIX shell: Linux, macOS, BSD, anything with <c>rm</c>.</summary>
    Posix,

    /// <summary>Windows <c>cmd.exe</c>, the stock default shell of OpenSSH on Windows.</summary>
    WindowsCommand,

    /// <summary>Windows PowerShell or PowerShell Core, set as the default shell.</summary>
    PowerShell,

    /// <summary>Something none of the above, or a server that would not answer; no shortcuts taken.</summary>
    Foreign,
}

/// <summary>
/// Tells one remote shell from another and writes the one command each of them understands. The three
/// disagree about everything that matters here — how a folder is removed, how a path is spelled, and
/// how a quote is escaped — so guessing wrong deletes nothing at best.
/// </summary>
public static class RemoteShells
{
    /// <summary>
    /// Works out what is answering, in as few round trips as it can: a POSIX shell owns up to
    /// <c>uname</c>, PowerShell to its own version table, and <c>cmd.exe</c> to its comspec.
    /// </summary>
    /// <param name="run">Runs a command and answers with its output and exit status.</param>
    /// <returns>What the far side is.</returns>
    public static RemoteShellKind Ask(Func<string, (string Output, int Status)> run)
    {
        ArgumentNullException.ThrowIfNull(run);

        var unix = run("uname -s");

        if (unix.Status == 0 && unix.Output.Trim().Length > 0 && !Confused(unix.Output))
        {
            return RemoteShellKind.Posix;
        }

        var powershell = run("$PSVersionTable.PSEdition");

        if (powershell.Output.Contains("Core", StringComparison.Ordinal) ||
            powershell.Output.Contains("Desktop", StringComparison.Ordinal))
        {
            return RemoteShellKind.PowerShell;
        }

        var comspec = run("echo %COMSPEC%");

        return comspec.Output.Contains("cmd.exe", StringComparison.OrdinalIgnoreCase)
            ? RemoteShellKind.WindowsCommand
            : RemoteShellKind.Foreign;
    }

    /// <summary>The command that removes a folder and everything under it, written for that shell.</summary>
    /// <param name="kind">What is answering.</param>
    /// <param name="path">The folder, as SFTP spells it.</param>
    /// <returns>The command, or <c>null</c> when the shell has no one-line way to do it.</returns>
    public static string? Sweep(RemoteShellKind kind, string path) => kind switch
    {
        RemoteShellKind.Posix => $"rm -rf -- '{path.Replace("'", @"'\''", StringComparison.Ordinal)}'",
        RemoteShellKind.WindowsCommand => $"rmdir /s /q \"{Local(path)}\"",
        RemoteShellKind.PowerShell =>
            $"Remove-Item -LiteralPath '{Local(path).Replace("'", "''", StringComparison.Ordinal)}' -Recurse -Force",
        _ => null,
    };

    /// <summary>
    /// The Windows spelling of a path SFTP reports as <c>/C:/Users/…</c>, which neither
    /// <c>cmd.exe</c> nor PowerShell will accept as it stands.
    /// </summary>
    /// <param name="path">The path as SFTP spells it.</param>
    /// <returns>The same path with a drive letter and backslashes.</returns>
    public static string Local(string path)
    {
        var trimmed = path.StartsWith('/') && path.Length > 2 && path[2] == ':' ? path[1..] : path;

        return trimmed.Replace('/', '\\');
    }

    private static bool Confused(string output) =>
        output.Contains("not recognized", StringComparison.OrdinalIgnoreCase) ||
        output.Contains("CommandNotFound", StringComparison.OrdinalIgnoreCase) ||
        output.Contains("not found", StringComparison.OrdinalIgnoreCase);
}
