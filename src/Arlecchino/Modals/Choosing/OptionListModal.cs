using System;
using System.Collections.Generic;
using Arlecchino.Editing;
using Arlecchino.Rendering;
using Arlecchino.Input;

namespace Arlecchino.Modals.Choosing;

/// <summary>
/// What the single- and multi-choice dialogs share: the options, what has been typed to narrow them, and the
/// cursor. What is typed is edited the way any other line is, so a symbol goes in one press.
/// </summary>
public abstract class OptionListModal : Modal, ITextEntry
{
    private string _text = "";
    private int _caret;
    private int _anchor;

    /// <summary>Everything that can be chosen from.</summary>
    public IReadOnlyList<string> Options { get; init; } = [];

    /// <summary>Whatever has been typed to narrow the list. Editing it resets the cursor to the top.</summary>
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

    /// <summary>Where the caret sits in what has been typed, which is at the end of it.</summary>
    public int Caret
    {
        get => _caret;
        set => _caret = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>Where a selection would start from, on the caret since none is drawn here.</summary>
    public int Anchor
    {
        get => _anchor;
        set => _anchor = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>Cursor position within the options that match.</summary>
    public int Index { get; set; }

    /// <summary>The line being typed into, which here is what narrows the list.</summary>
    public override ITextEntry Typing => this;

    /// <summary>Where the rows were drawn last frame, used to turn a click into a row.</summary>
    public SurfaceRegion Rows { get; set; }

    /// <summary>Index of the first option drawn, since a long list only shows a window of it.</summary>
    public int FirstVisible { get; set; }

    /// <summary>The options that pass the filter, in their original order.</summary>
    /// <returns>Matching options; all of them when nothing is typed.</returns>
    public List<string> MatchingOptions()
    {
        if (_text.Length == 0)
        {
            return [.. Options];
        }

        var matching = new List<string>();
        foreach (var choice in Options)
        {
            if (choice.Contains(_text, StringComparison.OrdinalIgnoreCase))
            {
                matching.Add(choice);
            }
        }

        return matching;
    }

    /// <summary>
    /// Takes pasted text into the filter and puts the cursor back on the first row, since what is showing
    /// after the paste is a different set of rows.
    /// </summary>
    /// <param name="frame">The keys to obey, and how to close.</param>
    /// <param name="text">What was pasted.</param>
    public override void HandlePaste(ModalFrame frame, string text)
    {
        base.HandlePaste(frame, text);

        Index = 0;
    }

    /// <summary>Acts on the row that was picked, which is what tells one kind of list from the other.</summary>
    /// <param name="frame">How to close, when picking closes.</param>
    /// <param name="choice">The option.</param>
    protected abstract void Take(ModalFrame frame, string choice);

    /// <summary>
    /// The wheel walks the list, and a click picks the row it landed on. A row is taken only when it was
    /// already the one under the cursor.
    /// </summary>
    /// <param name="frame">How to close.</param>
    /// <param name="mouse">The event that arrived.</param>
    public override void HandleMouse(ModalFrame frame, MouseEvent mouse)
    {
        var matching = MatchingOptions();

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                Index = Math.Max(0, Index - 1);
                return;
            case MouseAction.ScrolledDown:
                Index = Math.Min(Math.Max(0, matching.Count - 1), Index + 1);
                return;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left &&
                                          Rows.Contains(mouse.Row, mouse.Column):
                Picked(frame, matching, mouse);
                return;
        }
    }

    private void Picked(ModalFrame frame, List<string> matching, MouseEvent mouse)
    {
        var (row, _) = Rows.ToLocal(mouse.Row, mouse.Column);
        var index = FirstVisible + row;

        if (index < 0 || index >= matching.Count)
        {
            return;
        }

        var settled = index == Index;

        Index = index;

        if (settled)
        {
            Take(frame, matching[Math.Clamp(Index, 0, matching.Count - 1)]);
        }
    }
}
