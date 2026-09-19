using System;
using System.Collections.Generic;
using System.Text;

namespace Arlecchino.Widgets.Text;

/// <summary>
/// One line of a document cut into the runs it is drawn as: what is said loudly, what is code, and what
/// is a link. The marks that say so are read rather than drawn.
/// </summary>
internal static class MarkdownSpans
{
    private static readonly string[] Schemes = ["http://", "https://", "mailto:", "ftp://"];

    /// <summary>Reads a line.</summary>
    /// <param name="text">The line, as the document wrote it.</param>
    /// <param name="ink">What its plain stretches are, which a heading and a quote each change.</param>
    /// <param name="pictures">
    /// Where the pictures of the line are gathered, which takes them out of it: a picture is drawn under
    /// the line rather than in it, a cell being no place for one. Nothing at all where the caller has
    /// nowhere to draw one, and a picture is then written out as what it was to be shown instead of.
    /// </param>
    /// <returns>The runs it is drawn as, left to right.</returns>
    public static List<MarkdownRun> Of(
        string text,
        MarkdownInk ink,
        List<(string Address, string Alt)>? pictures = null)
    {
        var source = Collapsed(text);
        var runs = new List<MarkdownRun>();
        var plain = new StringBuilder();
        var index = 0;

        while (index < source.Length)
        {
            var symbol = source[index];

            if (symbol == '\\' && index + 1 < source.Length && char.IsPunctuation(source[index + 1]))
            {
                plain.Append(source[index + 1]);
                index += 2;
                continue;
            }

            if (symbol == '`' && Coded(source, index) is { } code)
            {
                Flush(runs, plain, ink);
                runs.Add(new(code.Text, MarkdownInk.Code));
                index = code.Next;
                continue;
            }

            if (symbol is '*' or '_' && Stressed(source, index, ink) is { } stress)
            {
                Flush(runs, plain, ink);
                runs.AddRange(stress.Runs);
                index = stress.Next;
                continue;
            }

            if (symbol is '[' or '!' && Linked(source, index, ink, pictures) is { } link)
            {
                Flush(runs, plain, ink);
                runs.AddRange(link.Runs);
                index = link.Next;
                continue;
            }

            if (symbol == '&' && MarkdownEntities.At(source, index) is { } entity)
            {
                plain.Append(entity.Symbol);
                index = entity.Next;
                continue;
            }

            if (symbol == '<' && Angled(source, index, pictures) is { } angled)
            {
                Parted(plain);
                Flush(runs, plain, ink);
                runs.AddRange(angled.Runs);
                index = angled.Next;
                continue;
            }

            plain.Append(symbol);
            index++;
        }

        Flush(runs, plain, ink);

        if (Wordless(runs))
        {
            runs.Clear();
        }

        Trimmed(runs);

        return runs;
    }

    /// <summary>
    /// Keeps a word boundary where markup was dropped. Two words written either side of a tag are two
    /// words, and a line that ran them together would be saying something the document did not.
    /// </summary>
    /// <param name="plain">The plain text gathered so far.</param>
    private static void Parted(StringBuilder plain)
    {
        if (plain.Length > 0 && plain[^1] != ' ')
        {
            plain.Append(' ');
        }
    }

    /// <summary>Takes the space off either end of a line, which dropped markup can leave behind.</summary>
    /// <param name="runs">The runs of the line.</param>
    private static void Trimmed(List<MarkdownRun> runs)
    {
        if (runs.Count == 0)
        {
            return;
        }

        runs[0] = runs[0] with { Text = runs[0].Text.TrimStart() };
        runs[^1] = runs[^1] with { Text = runs[^1].Text.TrimEnd() };

        if (runs[^1].Text.Length == 0)
        {
            runs.RemoveAt(runs.Count - 1);
        }

        if (runs.Count > 0 && runs[0].Text.Length == 0)
        {
            runs.RemoveAt(0);
        }
    }

    /// <summary>What a tag was given under a name, which is how a picture in markup says where it is.</summary>
    /// <param name="tag">The tag, without its brackets.</param>
    /// <param name="name">Which of the things it was given.</param>
    /// <returns>What it says, or nothing where the tag does not carry it.</returns>
    private static string Given(string tag, string name)
    {
        var at = tag.IndexOf($"{name}=", StringComparison.OrdinalIgnoreCase);

        if (at < 0)
        {
            return "";
        }

        var from = at + name.Length + 1;

        if (from >= tag.Length)
        {
            return "";
        }

        var isQuoted = tag[from] is '"' or '\'';
        var start = isQuoted ? from + 1 : from;
        var ending = isQuoted ? tag.IndexOf(tag[from], start) : tag.IndexOf(' ', start);

        return ending < 0 ? tag[start..] : tag[start..ending];
    }

    /// <summary>
    /// Whether a line came to nothing but the space between what was dropped. A line of markup without
    /// words was written for a browser, and what is left of it is no paragraph at all.
    /// </summary>
    /// <param name="runs">What the line came to.</param>
    /// <returns><c>true</c> when none of it says anything.</returns>
    private static bool Wordless(List<MarkdownRun> runs)
    {
        foreach (var run in runs)
        {
            if (!string.IsNullOrWhiteSpace(run.Text))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The line with every stretch of white space written as one space. A document breaks its paragraphs
    /// where the text happens to reach the edge of an editor, and that break is not one the pane keeps.
    /// </summary>
    /// <param name="text">The line as it was written.</param>
    /// <returns>The line as it is read.</returns>
    private static string Collapsed(string text)
    {
        var collapsedText = new StringBuilder(text.Length);

        foreach (var symbol in text)
        {
            if (!char.IsWhiteSpace(symbol))
            {
                collapsedText.Append(symbol);
                continue;
            }

            if (collapsedText.Length > 0 && collapsedText[^1] != ' ')
            {
                collapsedText.Append(' ');
            }
        }

        return collapsedText.ToString().TrimEnd();
    }

    /// <summary>Adds what has been gathered as plain text, if anything, and starts gathering again.</summary>
    /// <param name="runs">The runs so far.</param>
    /// <param name="plain">The text gathered so far.</param>
    /// <param name="ink">What plain text is here.</param>
    private static void Flush(List<MarkdownRun> runs, StringBuilder plain, MarkdownInk ink)
    {
        if (plain.Length == 0)
        {
            return;
        }

        runs.Add(new(plain.ToString(), ink));
        plain.Clear();
    }

    /// <summary>A code span, which takes whatever is between two runs of backticks of the same length.</summary>
    /// <param name="source">The line.</param>
    /// <param name="index">Where the opening backtick is.</param>
    /// <returns>What it says and where the line carries on, or nothing where it is never closed.</returns>
    private static (string Text, int Next)? Coded(string source, int index)
    {
        var ticks = 0;

        while (index + ticks < source.Length && source[index + ticks] == '`')
        {
            ticks++;
        }

        var fence = source.Substring(index, ticks);
        var closing = source.IndexOf(fence, index + ticks, StringComparison.Ordinal);

        return closing < 0
            ? null
            : (source[(index + ticks)..closing].Trim(), closing + ticks);
    }

    /// <summary>
    /// What a document put between stars or underscores. Two marks are strong, one is emphasis, and what
    /// is between them is read again so that a link or a code span inside it is still one.
    /// </summary>
    /// <param name="source">The line.</param>
    /// <param name="index">Where the opening mark is.</param>
    /// <param name="ink">What plain text is around it.</param>
    /// <returns>The runs and where the line carries on, or nothing where the marks are not a pair.</returns>
    private static (List<MarkdownRun> Runs, int Next)? Stressed(string source, int index, MarkdownInk ink)
    {
        var mark = source[index];
        var doubled = index + 1 < source.Length && source[index + 1] == mark;
        var fence = doubled ? source.Substring(index, 2) : source.Substring(index, 1);
        var from = index + fence.Length;
        var closing = source.IndexOf(fence, from, StringComparison.Ordinal);

        if (closing <= from || source[from] == ' ' || source[closing - 1] == ' ')
        {
            return null;
        }

        if (mark == '_' && (Wordy(source, index - 1) || Wordy(source, closing + fence.Length)))
        {
            return null;
        }

        var innerInk = doubled ? MarkdownInk.Strong : MarkdownInk.Emphasis;

        return (Of(source[from..closing], ink is MarkdownInk.Text ? innerInk : ink), closing + fence.Length);
    }

    /// <summary>
    /// Whether the character beside a mark makes a word of it. An underscore inside a name is not
    /// emphasis, and a document full of <c>snake_case</c> would otherwise read as one long slant.
    /// </summary>
    /// <param name="source">The line.</param>
    /// <param name="index">Which character, which may be off either end.</param>
    /// <returns><c>true</c> when there is a letter or a digit there.</returns>
    private static bool Wordy(string source, int index) =>
        index >= 0 && index < source.Length && char.IsLetterOrDigit(source[index]);

    /// <summary>
    /// A link, or a picture, which is drawn as what it was to be shown instead of. Where the link goes is
    /// written after it, a link in a pane being worth the address it holds.
    /// </summary>
    /// <param name="source">The line.</param>
    /// <param name="index">Where the bracket, or the mark before it, is.</param>
    /// <param name="ink">What plain text is around it.</param>
    /// <param name="pictures">Where a picture goes, or nothing where the caller cannot draw one.</param>
    /// <returns>The runs and where the line carries on, or nothing where the shape is not a link.</returns>
    private static (List<MarkdownRun> Runs, int Next)? Linked(
        string source,
        int index,
        MarkdownInk ink,
        List<(string Address, string Alt)>? pictures)
    {
        var opening = source[index] == '!' ? index + 1 : index;

        if (opening >= source.Length || source[opening] != '[')
        {
            return null;
        }

        var label = source.IndexOf(']', opening);

        if (label < 0 || label + 1 >= source.Length || source[label + 1] != '(')
        {
            return null;
        }

        var address = source.IndexOf(')', label + 2);

        if (address < 0)
        {
            return null;
        }

        var text = source[(opening + 1)..label];
        var url = source[(label + 2)..address].Trim();

        if (pictures is not null && opening != index && url.Length > 0)
        {
            List<MarkdownRun> pictureRuns = [];

            pictures.Add((url, text));

            return (pictureRuns, address + 1);
        }

        List<MarkdownRun> runs = text.Length == 0
            ? []
            : Of(text, ink is MarkdownInk.Text ? MarkdownInk.Link : ink);

        if (url.Length > 0 && !string.Equals(url, text, StringComparison.Ordinal))
        {
            runs.Add(new(runs.Count == 0 ? url : $" {url}", MarkdownInk.Url));
        }

        return (runs, address + 1);
    }

    /// <summary>
    /// What a document wrote between angle brackets: an address, which is a link, or a tag, which is
    /// dropped. A tag places a picture or centers a heading in a browser, neither of which a pane does.
    /// </summary>
    /// <param name="source">The line.</param>
    /// <param name="index">Where the opening bracket is.</param>
    /// <param name="pictures">Where a picture goes, or nothing where the caller cannot draw one.</param>
    /// <returns>The runs and where the line carries on, or nothing where the brackets are prose.</returns>
    private static (List<MarkdownRun> Runs, int Next)? Angled(
        string source,
        int index,
        List<(string Address, string Alt)>? pictures)
    {
        var closing = source.IndexOf('>', index);
        var stray = source.IndexOf('<', index + 1);

        if (closing < 0 || (stray >= 0 && stray < closing))
        {
            return null;
        }

        var tag = source[(index + 1)..closing];

        foreach (var scheme in Schemes)
        {
            if (!tag.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            List<MarkdownRun> linkRuns = [new(tag, MarkdownInk.Link)];

            return (linkRuns, closing + 1);
        }

        if (tag.Length == 0 || !(char.IsAsciiLetter(tag[0]) || tag[0] == '/'))
        {
            return null;
        }

        List<MarkdownRun> droppedRuns = [];

        if (pictures is not null &&
            tag.StartsWith("img", StringComparison.OrdinalIgnoreCase) &&
            Given(tag, "src") is { Length: > 0 } address)
        {
            pictures.Add((address, Given(tag, "alt")));
        }

        return (droppedRuns, closing + 1);
    }
}
