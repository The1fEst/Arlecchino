using System;
using System.Collections.Generic;

namespace Arlecchino.Pictures.Formats.Svg;

/// <summary>
/// The elements of a document, one at a time, with what each was written with. Nothing here nests, every
/// shape <see cref="Svg"/> draws being written as one element.
/// </summary>
/// <param name="text">The document.</param>
internal sealed class SvgTags(string text)
{
    private int _at;

    /// <summary>Reads the next opening element.</summary>
    /// <param name="name">What it is called, in lower case.</param>
    /// <param name="attributes">What it was written with, by name in lower case.</param>
    /// <returns><c>false</c> once the document is read out.</returns>
    public bool Next(out string name, out Dictionary<string, string> attributes)
    {
        name = "";
        attributes = [];

        while (Opening() is { } start)
        {
            if (Skipped(start))
            {
                continue;
            }

            var at = start + 1;

            if (text[at] == '/')
            {
                _at = Past(at);
                continue;
            }

            name = Word(ref at).ToLowerInvariant();
            attributes = Attributes(ref at);
            _at = Past(at);

            return name.Length > 0;
        }

        return false;
    }

    /// <summary>Where the next element starts, or nothing where the document is read out.</summary>
    /// <returns>The index of the angle bracket.</returns>
    private int? Opening()
    {
        var start = text.IndexOf('<', _at);

        if (start >= 0 && start + 1 < text.Length)
        {
            return start;
        }

        _at = text.Length;

        return null;
    }

    /// <summary>
    /// Steps over what is not an element: a comment, the declaration, a DOCTYPE or a marked section. A
    /// comment is run to its own ending, an angle bracket inside one being no element.
    /// </summary>
    /// <param name="start">Where the bracket is.</param>
    /// <returns><c>true</c> when something was stepped over.</returns>
    private bool Skipped(int start)
    {
        if (text.AsSpan(start).StartsWith("<!--", StringComparison.Ordinal))
        {
            var ending = text.IndexOf("-->", start, StringComparison.Ordinal);

            _at = ending < 0 ? text.Length : ending + 3;

            return true;
        }

        if (text[start + 1] is not ('?' or '!'))
        {
            return false;
        }

        _at = Past(start);

        return true;
    }

    /// <summary>Where the element that starts at an index ends.</summary>
    /// <param name="at">Anywhere inside it.</param>
    /// <returns>Just past its closing bracket.</returns>
    private int Past(int at)
    {
        var ending = text.IndexOf('>', at);

        return ending < 0 ? text.Length : ending + 1;
    }

    /// <summary>Everything the element was written with, up to its closing bracket.</summary>
    /// <param name="at">Just past the name; left where reading stopped.</param>
    /// <returns>The attributes.</returns>
    private Dictionary<string, string> Attributes(ref int at)
    {
        var attributes = new Dictionary<string, string>(StringComparer.Ordinal);

        while (at < text.Length && text[at] != '>')
        {
            while (at < text.Length && (char.IsWhiteSpace(text[at]) || text[at] == '/'))
            {
                at++;
            }

            if (at >= text.Length || text[at] == '>')
            {
                break;
            }

            var name = Word(ref at).ToLowerInvariant();

            while (at < text.Length && char.IsWhiteSpace(text[at]))
            {
                at++;
            }

            if (name.Length == 0 || at >= text.Length || text[at] != '=')
            {
                continue;
            }

            at++;

            attributes[name] = Value(ref at);
        }

        return attributes;
    }

    /// <summary>What an attribute was given, quoted either way.</summary>
    /// <param name="at">The equals sign; left just past the value.</param>
    /// <returns>The value, without its quotes.</returns>
    private string Value(ref int at)
    {
        while (at < text.Length && char.IsWhiteSpace(text[at]))
        {
            at++;
        }

        if (at >= text.Length || text[at] is not ('"' or '\''))
        {
            return Word(ref at);
        }

        var quote = text[at];
        var ending = text.IndexOf(quote, at + 1);

        if (ending < 0)
        {
            at = text.Length;

            return "";
        }

        var value = text[(at + 1)..ending];

        at = ending + 1;

        return value;
    }

    /// <summary>A name: everything up to a space, an equals sign or the end of the element.</summary>
    /// <param name="at">Where it starts; left where it ends.</param>
    /// <returns>The name.</returns>
    private string Word(ref int at)
    {
        var start = at;

        while (at < text.Length && !char.IsWhiteSpace(text[at]) && text[at] is not ('=' or '>' or '/'))
        {
            at++;
        }

        return text[start..at];
    }
}
