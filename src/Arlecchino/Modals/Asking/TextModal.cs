using System;
using Arlecchino.Editing;
using Arlecchino.Input;

namespace Arlecchino.Modals.Asking;

/// <summary>
/// A line of text: free text, a secret, an email or a link. The built-in format check runs before
/// <see cref="Validate"/>, so your validator only sees input that already looks right.
/// </summary>
public sealed class TextModal : Modal, ITextEntryModal
{
    private string _text = "";
    private int _caret;
    private int _anchor;

    /// <summary>Whatever has been typed so far. Assigning it puts the caret at the end.</summary>
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

    /// <summary>Where the caret sits, pulled into the text when it would fall outside.</summary>
    public int Caret
    {
        get => Math.Clamp(_caret, 0, _text.Length);
        set => _caret = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>Where the selection was started from, on the caret while nothing is selected.</summary>
    public int Anchor
    {
        get => Math.Clamp(_anchor, 0, _text.Length);
        set => _anchor = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>Drawn before the field.</summary>
    public string Prefix { get; init; } = "";

    /// <summary>Drawn after the field.</summary>
    public string Suffix { get; init; } = "";

    /// <summary>Whether to draw dots instead of the text.</summary>
    public bool Masked { get; init; }

    /// <summary>Built-in check applied on confirm.</summary>
    public TextFormat Format { get; init; } = TextFormat.Free;

    /// <summary>
    /// Your own check, run after the format one. Return a message to keep the dialog open.
    /// </summary>
    public Func<string, string?>? Validate { get; init; }

    /// <summary>Called with the accepted text.</summary>
    public required Action<string> OnSubmit { get; init; }

    /// <summary>Validation message shown under the field.</summary>
    public string? Message { get; set; }

    /// <summary>The line being typed into, which for a field of one line is the field itself.</summary>
    public override ITextEntry Typing => this;

    /// <summary>Accepts anything that can be typed.</summary>
    /// <param name="character">The character resolved from the key press.</param>
    /// <returns>Always <c>true</c>.</returns>
    public bool AcceptsCharacter(char character) => true;

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) =>
        frame.Paint.Entry(this, Title, frame.Strings.ModalTextHints());

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key) => frame.Fields.Text(this, key);
}
