using System;
using System.Collections.Generic;

namespace Arlecchino.Widgets.Text;

/// <summary>How a line of a document is laid out, which is what says whether it may be wrapped.</summary>
internal enum MarkdownShape
{
    /// <summary>Prose, wrapped to the width it is drawn in.</summary>
    Prose,

    /// <summary>A line of a code block, drawn as it stands on a band of its own.</summary>
    Code,

    /// <summary>A row of a table, already padded to its columns and never wrapped.</summary>
    Table,

    /// <summary>A rule across whatever room is left beside it.</summary>
    Rule,

    /// <summary>
    /// A picture, which is drawn rather than written. The line holds the address it was given and the
    /// words that stand for it until it arrives, or for good where nothing can draw it.
    /// </summary>
    Picture,
}

/// <summary>
/// What a stretch of a line is, said as what it is for rather than as a color. The color it comes to is
/// <see cref="Arlecchino.Widgets.Readouts.MarkdownStyles"/>, and the document knows nothing about it.
/// </summary>
internal enum MarkdownInk
{
    /// <summary>Ordinary prose.</summary>
    Text,

    /// <summary>What the document put between two stars.</summary>
    Strong,

    /// <summary>What it put between one.</summary>
    Emphasis,

    /// <summary>A code span, and every line of a code block.</summary>
    Code,

    /// <summary>The first two levels of heading.</summary>
    Heading,

    /// <summary>The four below them.</summary>
    Subheading,

    /// <summary>A bullet, the number of an item, the bar down the side of a quote.</summary>
    Marker,

    /// <summary>What a quote says.</summary>
    Quote,

    /// <summary>What a link is written as.</summary>
    Link,

    /// <summary>Where it goes.</summary>
    Url,

    /// <summary>Rules, and the lines between the columns of a table.</summary>
    Rule,

    /// <summary>The first row of a table.</summary>
    TableHeader,
}

/// <summary>One stretch of a line written in one ink.</summary>
/// <param name="Text">What it says.</param>
/// <param name="Ink">What it is.</param>
internal readonly record struct MarkdownRun(string Text, MarkdownInk Ink);

/// <summary>
/// One line of a read document, before it is wrapped. Its three sets of runs differ in what becomes of
/// them once the line is broken: see <see cref="Lead"/>, <see cref="Marker"/> and <see cref="Runs"/>.
/// </summary>
internal sealed record MarkdownLine
{
    /// <summary>What the line is.</summary>
    public MarkdownShape Shape { get; init; } = MarkdownShape.Prose;

    /// <summary>How far in it starts, which is how a list inside a list is set apart.</summary>
    public int Indent { get; init; }

    /// <summary>
    /// Where the pictures of this line are, for a line that is pictures, and nothing at all for every
    /// other line. A line the document wrote two into is two, drawn side by side as it wrote them.
    /// </summary>
    public IReadOnlyList<string> Pictures { get; init; } = [];

    /// <summary>The bar down the side of a quote: on every row the line takes.</summary>
    public IReadOnlyList<MarkdownRun> Lead { get; init; } = [];

    /// <summary>The bullet or the number of an item, which the rest of the item is aligned under.</summary>
    public IReadOnlyList<MarkdownRun> Marker { get; init; } = [];

    /// <summary>What the line says.</summary>
    public IReadOnlyList<MarkdownRun> Runs { get; init; } = [];
}

/// <summary>
/// A document read into the lines it is drawn as. Everything that depends on how wide the pane turns out
/// to be is left to <see cref="MarkdownWrap"/>; what is settled here is what each line is and what it says.
/// </summary>
internal static class Markdown
{
    private const string Tab = "    ";
    private const string Bar = "▌ ";
    private const string CheckedBox = "☑ ";
    private const string UncheckedBox = "☐ ";
    private const int NestWidth = 2;
    private const int Deepest = 2;
    private const int Headings = 6;

    private static readonly string[] Fences = ["```", "~~~"];
    private static readonly string[] Bullets = ["• ", "◦ ", "▪ "];

    /// <summary>Reads a document.</summary>
    /// <param name="text">The document as it was written.</param>
    /// <returns>The lines it is drawn as, in order.</returns>
    public static List<MarkdownLine> Read(string text)
    {
        var source = Expanded(text);
        var lines = new List<MarkdownLine>();
        var fence = "";
        var index = 0;

        while (index < source.Length)
        {
            var raw = source[index];
            var trimmedLine = raw.TrimStart();

            if (fence.Length > 0)
            {
                if (trimmedLine.StartsWith(fence, StringComparison.Ordinal))
                {
                    fence = "";
                }
                else
                {
                    lines.Add(new()
                    {
                        Shape = MarkdownShape.Code,
                        Indent = NestWidth,
                        Runs = [new(raw, MarkdownInk.Code)],
                    });
                }

                index++;
                continue;
            }

            if (Fenced(trimmedLine) is { } opening)
            {
                fence = opening;
                index++;
                continue;
            }

            if (trimmedLine.Length == 0)
            {
                Blank(lines);
                index++;
                continue;
            }

            if (Broken(trimmedLine))
            {
                lines.Add(new() { Shape = MarkdownShape.Rule });
                index++;
                continue;
            }

            if (Underlined(trimmedLine) && Promote(lines))
            {
                index++;
                continue;
            }

            if (trimmedLine.StartsWith('|'))
            {
                var count = MarkdownTable.Read(source, index, lines);

                if (count > 0)
                {
                    index += count;
                    continue;
                }
            }

            var ran = Paragraph(source, index);

            Block(ran.Text, [], MarkdownInk.Text, lines);
            index += ran.Lines;
        }

        return lines;
    }

    /// <summary>
    /// A paragraph as one line, however many the document broke it over. A blank line ends it, and so does
    /// a line that is plainly something else.
    /// </summary>
    /// <param name="source">Every line of the document.</param>
    /// <param name="index">The line it starts on.</param>
    /// <returns>The paragraph and how many lines it was written over.</returns>
    private static (string Text, int Lines) Paragraph(string[] source, int index)
    {
        var text = source[index];
        var next = index + 1;

        while (next < source.Length && Carries(source[next]))
        {
            text = $"{text} {source[next].Trim()}";
            next++;
        }

        return (text, next - index);
    }

    /// <summary>Whether a line is more of the paragraph above it rather than something of its own.</summary>
    /// <param name="line">The line.</param>
    /// <returns><c>true</c> when it carries on.</returns>
    private static bool Carries(string line)
    {
        var trimmedLine = line.TrimStart();

        return trimmedLine.Length > 0 &&
               Fenced(trimmedLine) is null &&
               Hashes(trimmedLine) == 0 &&
               !Broken(trimmedLine) &&
               !Underlined(trimmedLine) &&
               !trimmedLine.StartsWith('>') &&
               !trimmedLine.StartsWith('|') &&
               Listed(trimmedLine, 0) is null;
    }

    /// <summary>
    /// The document as lines with their tabs written out. A tab is a stop rather than a width, but a
    /// document is read here to be shown rather than to be compiled, and four spaces is what one looks like.
    /// </summary>
    /// <param name="text">The document.</param>
    /// <returns>Its lines.</returns>
    private static string[] Expanded(string text)
    {
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            lines[index] = lines[index].Replace("\t", Tab, StringComparison.Ordinal).TrimEnd();
        }

        return lines;
    }

    /// <summary>
    /// One line that is not a fence, a rule or a table: a heading, a quote, an item of a list, or a
    /// paragraph. A quote is read again as whatever it holds.
    /// </summary>
    /// <param name="raw">The line, indent and all.</param>
    /// <param name="lead">What is written at the start of every row it takes.</param>
    /// <param name="ink">What its prose is written in, which a quote changes for everything inside it.</param>
    /// <param name="lines">Where the line goes.</param>
    private static void Block(
        string raw,
        IReadOnlyList<MarkdownRun> lead,
        MarkdownInk ink,
        List<MarkdownLine> lines)
    {
        var trimmedLine = raw.TrimStart();
        var indent = raw.Length - trimmedLine.Length;

        if (trimmedLine.StartsWith('>'))
        {
            var rest = trimmedLine[1..];

            Block(
                rest.StartsWith(' ') ? rest[1..] : rest,
                [.. lead, new(Bar, MarkdownInk.Marker)],
                MarkdownInk.Quote,
                lines);

            return;
        }

        var level = Hashes(trimmedLine);

        if (level > 0)
        {
            var title = trimmedLine[level..].Trim().TrimEnd('#').TrimEnd();
            var heading = level <= 2 ? MarkdownInk.Heading : MarkdownInk.Subheading;

            lines.Add(new() { Lead = lead, Runs = MarkdownSpans.Of(title, heading) });

            if (level == 1)
            {
                lines.Add(new() { Shape = MarkdownShape.Rule, Lead = lead });
            }

            return;
        }

        if (Listed(trimmedLine, indent) is { } item)
        {
            lines.Add(new()
            {
                Indent = Math.Min(indent / NestWidth, Deepest) * NestWidth,
                Lead = lead,
                Marker = [new(item.Marker, MarkdownInk.Marker)],
                Runs = MarkdownSpans.Of(item.Text, ink),
            });

            return;
        }

        var pictures = new List<(string Address, string Alt)>();
        var runs = MarkdownSpans.Of(trimmedLine, ink, pictures);

        if (runs.Count > 0)
        {
            lines.Add(new() { Lead = lead, Runs = runs });
        }
        else if (pictures.Count == 0)
        {
            Blank(lines);
        }

        if (pictures.Count == 0)
        {
            return;
        }

        var addresses = new List<string>(pictures.Count);
        var runsForPictures = new List<MarkdownRun>(pictures.Count);

        foreach (var picture in pictures)
        {
            addresses.Add(picture.Address);
            runsForPictures.Add(new(
                $"{(runsForPictures.Count > 0 ? " " : "")}{(picture.Alt.Length > 0 ? picture.Alt : picture.Address)}",
                MarkdownInk.Url));
        }

        lines.Add(new()
        {
            Shape = MarkdownShape.Picture,
            Lead = lead,
            Pictures = addresses,
            Runs = runsForPictures,
        });
    }

    /// <summary>The fence a line opens, or nothing where it opens none.</summary>
    /// <param name="trimmedLine">The line without its indent.</param>
    /// <returns>The fence to look for again to close it.</returns>
    private static string? Fenced(string trimmedLine)
    {
        foreach (var fence in Fences)
        {
            if (trimmedLine.StartsWith(fence, StringComparison.Ordinal))
            {
                return fence;
            }
        }

        return null;
    }

    /// <summary>How many levels of heading a line opens with, nought where it opens none.</summary>
    /// <param name="trimmedLine">The line without its indent.</param>
    /// <returns>The level, from one to six.</returns>
    private static int Hashes(string trimmedLine)
    {
        var level = 0;

        while (level < trimmedLine.Length && trimmedLine[level] == '#')
        {
            level++;
        }

        var opened = level is > 0 and <= Headings &&
                     (trimmedLine.Length == level || trimmedLine[level] == ' ');

        return opened ? level : 0;
    }

    /// <summary>
    /// The mark an item of a list opens with and what is left of it. A bullet is drawn by how deep the
    /// item is rather than by which of the three marks the document used for it.
    /// </summary>
    /// <param name="trimmedLine">The line without its indent.</param>
    /// <param name="indent">How far in it started, which is what says how deep it is.</param>
    /// <returns>The mark and what it is against, or nothing where the line is not an item.</returns>
    private static (string Marker, string Text)? Listed(string trimmedLine, int indent)
    {
        if (trimmedLine.Length > 1 && trimmedLine[0] is '-' or '*' or '+' && trimmedLine[1] == ' ')
        {
            var rest = trimmedLine[2..].TrimStart();

            return Boxed(rest) ?? (Bullets[Math.Min(indent / NestWidth, Deepest)], rest);
        }

        var digits = 0;

        while (digits < trimmedLine.Length && char.IsAsciiDigit(trimmedLine[digits]))
        {
            digits++;
        }

        var numbered = digits > 0 &&
                       digits + 1 < trimmedLine.Length &&
                       trimmedLine[digits] is '.' or ')' &&
                       trimmedLine[digits + 1] == ' ';

        return numbered
            ? ($"{trimmedLine[..digits]}. ", trimmedLine[(digits + 2)..].TrimStart())
            : null;
    }

    /// <summary>The box an item of a task list opens with, in place of its bullet.</summary>
    /// <param name="rest">What follows the bullet.</param>
    /// <returns>The box and what follows it, or nothing where there is no box.</returns>
    private static (string Marker, string Text)? Boxed(string rest)
    {
        if (rest.Length < 4 || rest[0] != '[' || rest[2] != ']' || rest[3] != ' ')
        {
            return null;
        }

        return rest[1] switch
        {
            ' ' => (UncheckedBox, rest[4..]),
            'x' or 'X' => (CheckedBox, rest[4..]),
            _ => null,
        };
    }

    /// <summary>Whether a line is a rule: three or more of one mark, and nothing else but spaces.</summary>
    /// <param name="trimmedLine">The line without its indent.</param>
    /// <returns><c>true</c> when it is one.</returns>
    private static bool Broken(string trimmedLine)
    {
        var mark = trimmedLine[0];

        if (mark is not ('-' or '*' or '_'))
        {
            return false;
        }

        var marks = 0;

        foreach (var symbol in trimmedLine)
        {
            if (symbol == mark)
            {
                marks++;
            }
            else if (symbol != ' ')
            {
                return false;
            }
        }

        return marks >= 3;
    }

    /// <summary>Whether a line is the row of <c>=</c> that makes a heading of the line above it.</summary>
    /// <param name="trimmedLine">The line without its indent.</param>
    /// <returns><c>true</c> when it is one.</returns>
    private static bool Underlined(string trimmedLine)
    {
        foreach (var symbol in trimmedLine)
        {
            if (symbol != '=')
            {
                return false;
            }
        }

        return trimmedLine.Length > 1;
    }

    /// <summary>
    /// Makes a heading of the line already read, which is the other way a document writes one. It is
    /// refused where there is nothing above to promote, and the row of <c>=</c> is then read as prose.
    /// </summary>
    /// <param name="lines">The lines read so far.</param>
    /// <returns><c>true</c> when the line above became a heading.</returns>
    private static bool Promote(List<MarkdownLine> lines)
    {
        if (lines.Count == 0)
        {
            return false;
        }

        var last = lines[^1];

        if (last.Shape != MarkdownShape.Prose || last.Runs.Count == 0 || last.Marker.Count > 0)
        {
            return false;
        }

        var heading = new List<MarkdownRun>(last.Runs.Count);

        foreach (var run in last.Runs)
        {
            heading.Add(run with { Ink = MarkdownInk.Heading });
        }

        lines[^1] = last with { Runs = heading };
        lines.Add(new() { Shape = MarkdownShape.Rule, Lead = last.Lead });

        return true;
    }

    /// <summary>
    /// Keeps one empty line where the document left several, and none at all at the top. A pane is a
    /// screenful, with no room to spare around a paragraph.
    /// </summary>
    /// <param name="lines">The lines read so far.</param>
    private static void Blank(List<MarkdownLine> lines)
    {
        if (lines.Count == 0 || lines[^1] is { Shape: MarkdownShape.Prose, Runs.Count: 0, Lead.Count: 0 })
        {
            return;
        }

        lines.Add(new());
    }
}
