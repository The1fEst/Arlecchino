using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Arlecchino;

/// <summary>
/// The programs a desktop keeps for its clipboard, tried in turn until one takes the text. They are what a
/// copy falls back on where OSC 52 is switched off, which a terminal never says.
/// </summary>
internal static class ClipboardPrograms
{
    /// <summary>How long one program is given to take the text before the next one is tried.</summary>
    private const int Waiting = 500;

    private static readonly UTF8Encoding Utf8 = new(false);

    private static readonly ClipboardProgram[] KnownPrograms =
    [
        new("pbcopy"),
        new("termux-clipboard-set"),
        new("wl-copy"),
        new("xclip", "-selection", "clipboard"),
        new("xsel", "-ib"),
    ];

    /// <summary>
    /// The programs to try, in the order they are tried. A test sets it somewhere harmless rather than at
    /// the clipboard of whoever is running the test.
    /// </summary>
    internal static IReadOnlyList<ClipboardProgram> Programs { get; set; } = KnownPrograms;

    /// <summary>
    /// Hands the text to the first program that takes it, and to none at all away from Linux, macOS and
    /// the BSDs. One that is not installed fails to start at once and costs nothing.
    /// </summary>
    /// <param name="text">What to copy.</param>
    /// <returns><c>true</c> when one of them took it.</returns>
    internal static bool Write(string text)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS() && !OperatingSystem.IsFreeBSD())
        {
            return false;
        }

        foreach (var program in Programs)
        {
            if (TryWrite(program, text))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Runs one of them and writes the text down its standard input, all three streams redirected to keep
    /// a complaint about a display off the frame. One that outstays <see cref="Waiting"/> is left running.
    /// </summary>
    /// <param name="program">The program to run.</param>
    /// <param name="text">What to write to it.</param>
    /// <returns><c>true</c> when it ran and ended happy.</returns>
    private static bool TryWrite(ClipboardProgram program, string text)
    {
        var start = new ProcessStartInfo(program.FileName)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardInputEncoding = Utf8,
        };

        foreach (var argument in program.Arguments)
        {
            start.ArgumentList.Add(argument);
        }

        try
        {
            using var running = Process.Start(start);

            if (running is null)
            {
                return false;
            }

            using (var input = running.StandardInput)
            {
                input.Write(text);
            }

            return running.WaitForExit(Waiting) && running.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
    }
}
