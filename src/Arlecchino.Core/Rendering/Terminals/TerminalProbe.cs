using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Rendering.Terminals;

/// <summary>
/// What the terminal said when it was asked what it can do. Sizes of nought mean it did not say.
/// </summary>
/// <param name="Sixel">Whether the device attributes listed sixel.</param>
/// <param name="Kitty">Whether the kitty graphics query was answered.</param>
/// <param name="CellWidth">Pixels across a cell, or nought.</param>
/// <param name="CellHeight">Pixels down a cell, or nought.</param>
/// <param name="Background">The colour behind the text, or <c>null</c> when it did not say.</param>
internal readonly record struct TerminalAnswers(
    bool Sixel,
    bool Kitty,
    int CellWidth,
    int CellHeight,
    Rgb? Background);

/// <summary>
/// Asks the terminal what it can do, once, before the application starts reading keys.
///
/// Everything here rests on one arrangement: the questions go out in an order that ends with the one
/// every terminal answers — primary device attributes — so the reply to it is the signal that no other
/// reply is coming. Without that fence there is nothing to wait for but a guess at how long a terminal
/// takes to stay silent.
///
/// The fence stops the waiting, not the reading: once it has arrived, whatever is already buffered is
/// taken too, and only a lull or a keystroke ends it. A terminal that answers out of the order it was
/// asked would otherwise have its last answer cut off, and nothing in any specification says it must
/// answer in order.
///
/// A terminal that answers nothing costs the deadline and leaves every setting as it was, which is the
/// behaviour an application already had to live with. Whatever was read is then handed straight back,
/// because without the fence there is no telling an answer from a keystroke.
///
/// With the fence, there is: answers are escape sequences, so what was read outside every sequence was
/// typed while the terminal was being asked, and <see cref="TerminalReply"/> picks it out to be handed
/// back. Judging the whole read by its shape does not work — on Windows the console layer eats the kitty
/// query's reply and leaves the last character of it behind, so the first thing a terminal says can be a
/// lone backslash, and treating that as something a person typed threw away every answer behind it.
/// </summary>
public static class TerminalProbe
{
    private const string KittyQuery = "\e_Gi=31,s=1,v=1,a=q,t=d,f=24;AAAA\e\\";
    private const string CellSizeQuery = "\e[16t";
    private const string TextAreaQuery = "\e[14t";
    private const string BackgroundQuery = "\e]11;?\a";
    private const string AttributesQuery = "\e[c";

    /// <summary>
    /// Asks, waits no longer than it is told, and installs what came back into
    /// <see cref="TerminalCapabilities"/> and <see cref="Glyphs"/>. Anything the terminal did not answer
    /// is left alone.
    ///
    /// Call it before the mouse and paste modes go on, so their reports cannot arrive among the answers.
    /// </summary>
    /// <param name="terminal">The terminal to ask.</param>
    /// <param name="within">How long to wait for the last answer before giving up.</param>
    /// <returns>
    /// <c>true</c> when the terminal said anything at all. What it said is in
    /// <see cref="TerminalCapabilities.Sixel"/>, <see cref="TerminalCapabilities.Kitty"/> and
    /// <see cref="Glyphs.CellWidth"/>; a <c>false</c> here is how you tell a terminal that cannot draw
    /// pictures from one that never replied.
    /// </returns>
    public static bool Ask(IArlecchinoTerminal terminal, TimeSpan within)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        terminal.Write(KittyQuery + CellSizeQuery + TextAreaQuery + BackgroundQuery + AttributesQuery);

        var heard = Listen(terminal, within);

        Install(Read(heard, terminal.Width, terminal.Height));

        return heard.Length > 0;
    }

    private static void Install(TerminalAnswers answers)
    {
        var known = answers.CellWidth > 0 && answers.CellHeight > 0;

        TerminalCapabilities.Sixel = answers.Sixel;
        TerminalCapabilities.Kitty = answers.Kitty;
        TerminalCapabilities.CellSizeKnown = known;
        TerminalCapabilities.Background = answers.Background;

        if (!known)
        {
            return;
        }

        Glyphs.CellWidth = answers.CellWidth;
        Glyphs.CellHeight = answers.CellHeight;
    }

    private static string Listen(IArlecchinoTerminal terminal, TimeSpan within)
    {
        var heard = new StringBuilder();
        var until = DateTime.UtcNow + within;

        var read = new List<ConsoleKeyInfo>();
        var fenced = false;

        while (DateTime.UtcNow < until)
        {
            if (!terminal.KeyAvailable)
            {
                if (fenced)
                {
                    break;
                }

                Thread.Sleep(1);
                continue;
            }

            var key = terminal.ReadKey();

            read.Add(key);
            heard.Append(key.KeyChar);

            fenced = fenced || Fenced(heard);
        }

        if (!fenced)
        {
            foreach (var key in read)
            {
                terminal.Unread(key);
            }

            return "";
        }

        var said = heard.ToString();

        foreach (var at in TerminalReply.Typing(said))
        {
            terminal.Unread(read[at]);
        }

        return said;
    }

    private static bool Fenced(StringBuilder heard) =>
        heard.Length > 0 && heard[^1] == 'c' && heard.ToString().Contains("\e[?", StringComparison.Ordinal);

    internal static TerminalAnswers Read(string heard, int columns, int rows)
    {
        var kitty = heard.Contains("_Gi=31;OK", StringComparison.Ordinal);
        var (cellWidth, cellHeight) = CellSize(heard, columns, rows);

        return new(Sixel(heard), kitty, cellWidth, cellHeight, Background(heard));
    }

    /// <summary>
    /// Reads the answer to <c>OSC 11</c>, which is the colour behind the text written as
    /// <c>rgb:</c> and three hex groups. Terminals differ on how many digits a group has — two and four
    /// are both common — so each is scaled from however many it turned out to be.
    /// </summary>
    /// <param name="heard">Everything the terminal said.</param>
    /// <returns>The colour, or <c>null</c> when it did not say.</returns>
    private static Rgb? Background(string heard)
    {
        var at = heard.IndexOf("]11;rgb:", StringComparison.Ordinal);

        if (at < 0)
        {
            return null;
        }

        var reading = at + "]11;rgb:".Length;
        var channels = new byte[3];

        for (var channel = 0; channel < 3; channel++)
        {
            if (channel > 0)
            {
                if (reading >= heard.Length || heard[reading] != '/')
                {
                    return null;
                }

                reading++;
            }

            if (!TryChannel(heard, ref reading, out channels[channel]))
            {
                return null;
            }
        }

        return new(channels[0], channels[1], channels[2]);
    }

    private static bool TryChannel(string heard, ref int at, out byte channel)
    {
        var value = 0;
        var digits = 0;

        while (at < heard.Length && Uri.IsHexDigit(heard[at]) && digits < 4)
        {
            value = (value * 16) + Uri.FromHex(heard[at++]);
            digits++;
        }

        channel = digits switch
        {
            1 => (byte)(value * 17),
            2 => (byte)value,
            3 => (byte)(value >> 4),
            4 => (byte)(value >> 8),
            _ => 0,
        };

        return digits > 0;
    }

    private static bool Sixel(string heard)
    {
        var at = heard.IndexOf("\e[?", StringComparison.Ordinal);

        if (at < 0)
        {
            return false;
        }

        var end = heard.IndexOf('c', at);

        if (end < 0)
        {
            return false;
        }

        foreach (var attribute in heard[(at + 3)..end].Split(';'))
        {
            if (attribute == "4")
            {
                return true;
            }
        }

        return false;
    }

    private static (int Width, int Height) CellSize(string heard, int columns, int rows)
    {
        var (cellHeight, cellWidth) = Report(heard, "\e[6;");

        if (cellWidth > 0 && cellHeight > 0)
        {
            return (cellWidth, cellHeight);
        }

        var (areaHeight, areaWidth) = Report(heard, "\e[4;");

        if (areaWidth > 0 && areaHeight > 0 && columns > 0 && rows > 0)
        {
            return (areaWidth / columns, areaHeight / rows);
        }

        return (0, 0);
    }

    private static (int First, int Second) Report(string heard, string opening)
    {
        var at = heard.IndexOf(opening, StringComparison.Ordinal);

        if (at < 0)
        {
            return (0, 0);
        }

        var reading = at + opening.Length;
        var first = Number(heard, ref reading);

        if (reading >= heard.Length || heard[reading] != ';')
        {
            return (0, 0);
        }

        reading++;

        var second = Number(heard, ref reading);

        return reading < heard.Length && heard[reading] == 't' ? (first, second) : (0, 0);
    }

    private static int Number(string heard, ref int at)
    {
        var value = 0;
        var digits = 0;

        while (at < heard.Length && char.IsAsciiDigit(heard[at]))
        {
            value = (value * 10) + (heard[at++] - '0');
            digits++;
        }

        return digits == 0 ? 0 : value;
    }
}
