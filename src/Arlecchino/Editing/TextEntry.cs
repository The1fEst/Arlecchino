using System;

namespace Arlecchino.Editing;

/// <summary>
/// A line of text held on its own, for whatever is typed into but is not a dialog: a filter in a view, a
/// line an application draws for itself. What each key does to it is <see cref="TextEditing"/>'s.
/// </summary>
public sealed class TextEntry : ITextEntry
{
    private string _text = "";
    private int _caret;
    private int _anchor;

    /// <inheritdoc/>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _caret = value.Length;
            _anchor = value.Length;
        }
    }

    /// <inheritdoc/>
    public int Caret
    {
        get => _caret;
        set => _caret = Math.Clamp(value, 0, _text.Length);
    }

    /// <inheritdoc/>
    public int Anchor
    {
        get => _anchor;
        set => _anchor = Math.Clamp(value, 0, _text.Length);
    }
}
