using System;
using Arlecchino.Editing;

namespace Arlecchino.Tests.Support;

public sealed class TestEntry : ITextEntry
{
    private string _text = "";
    private int _caret;
    private int _anchor;

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

    public int Caret
    {
        get => _caret;
        set => _caret = Math.Clamp(value, 0, _text.Length);
    }

    public int Anchor
    {
        get => _anchor;
        set => _anchor = Math.Clamp(value, 0, _text.Length);
    }
}
