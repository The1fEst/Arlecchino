using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Diagnostics;

/// <summary>
/// One of the two writers <see cref="StrayOutput"/> puts in front of the console. It hands text to the
/// console's own writer while no frame is on the screen, and gathers it line by line while one is.
/// </summary>
internal sealed class StrayWriter : TextWriter
{
    private const int LongestLine = 4096;
    private const char Delete = (char)0x7f;
    private const char AfterTheControls = (char)0xa0;

    private readonly StrayOutput _strays;
    private readonly TextWriter _writer;
    private readonly string _category;
    private readonly LogLevel _level;
    private readonly StringBuilder _line = new();

    /// <summary>Creates the writer.</summary>
    /// <param name="strays">What holds the console and decides whether text is caught.</param>
    /// <param name="writer">The console's own writer for this stream.</param>
    /// <param name="category">What a caught line is logged under.</param>
    /// <param name="level">How a caught line is colored in the overlay.</param>
    public StrayWriter(StrayOutput strays, TextWriter writer, string category, LogLevel level)
    {
        _strays = strays;
        _writer = writer;
        _category = category;
        _level = level;

        strays.FlushWith(FlushLine);
    }

    /// <summary>The encoding of the writer being stood in front of.</summary>
    public override Encoding Encoding => _writer.Encoding;

    /// <summary>Takes one character.</summary>
    /// <param name="value">The character.</param>
    public override void Write(char value) => Write(value.ToString());

    /// <summary>Takes part of a buffer.</summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="index">Where the text starts.</param>
    /// <param name="count">How much of it there is.</param>
    public override void Write(char[] buffer, int index, int count) =>
        Write(new string(buffer, index, count));

    /// <summary>Takes text, and either passes it on or gathers it into a line.</summary>
    /// <param name="value">The text.</param>
    public override void Write(string? value)
    {
        if (value is null)
        {
            return;
        }

        if (!_strays.Holding)
        {
            _writer.Write(value);
            return;
        }

        Gather(value);
    }

    /// <summary>Flushes the console's own writer, and does nothing while text is being gathered.</summary>
    public override void Flush()
    {
        if (!_strays.Holding)
        {
            _writer.Flush();
        }
    }

    /// <summary>Logs a line that was gathered but never finished with a newline.</summary>
    public void FlushLine()
    {
        lock (_line)
        {
            Report();
        }
    }

    private void Gather(string text)
    {
        lock (_line)
        {
            foreach (var character in text)
            {
                if (character == '\n')
                {
                    Report();
                    continue;
                }

                _line.Append(character);

                if (_line.Length >= LongestLine)
                {
                    Report();
                }
            }
        }
    }

    private void Report()
    {
        if (_line.Length == 0)
        {
            return;
        }

        var line = Plain(_line.ToString());

        _line.Clear();

        if (line.Length > 0)
        {
            _strays.Caught(line, _category, _level);
        }
    }

    /// <summary>
    /// The text without what only a terminal can read. Escape sequences drawn into the overlay would go
    /// straight back out to the terminal and move the frame around, which is the very thing being caught.
    /// </summary>
    /// <param name="text">The line as it was written.</param>
    /// <returns>The line as it can be drawn.</returns>
    private static string Plain(string text)
    {
        var plain = new StringBuilder(text.Length);

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];

            if (character == '\e')
            {
                index = EndOfSequence(text, index);
                continue;
            }

            plain.Append(Readable(character) ? character : ' ');
        }

        return plain.ToString().TrimEnd();
    }

    private static bool Readable(char character) =>
        character >= ' ' && (character < Delete || character >= AfterTheControls);

    /// <summary>Where an escape sequence starting at a position ends.</summary>
    /// <param name="text">The line being read.</param>
    /// <param name="start">The position of the escape itself.</param>
    /// <returns>The position of the last character belonging to the sequence.</returns>
    private static int EndOfSequence(string text, int start)
    {
        var index = start + 1;

        if (index >= text.Length)
        {
            return index;
        }

        if (text[index] == '[')
        {
            for (index++; index < text.Length && (text[index] < '@' || text[index] > '~'); index++) { }

            return index;
        }

        if (text[index] is not (']' or 'P' or 'X' or '^' or '_'))
        {
            return index;
        }

        for (index++; index < text.Length; index++)
        {
            if (text[index] == '\a')
            {
                return index;
            }

            if (text[index] == '\e' && index + 1 < text.Length && text[index + 1] == '\\')
            {
                return index + 1;
            }
        }

        return index;
    }
}
