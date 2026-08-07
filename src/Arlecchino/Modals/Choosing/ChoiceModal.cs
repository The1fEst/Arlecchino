using System;
using Arlecchino.Input;

namespace Arlecchino.Modals.Choosing;

/// <summary>One option out of a filterable list.</summary>
public sealed class ChoiceModal : OptionListModal
{
    /// <summary>Called with the chosen option.</summary>
    public required Action<string> OnPicked { get; init; }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Lists.One(this);

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key) => frame.Choices.One(this, key);

    /// <inheritdoc/>
    protected override void Take(ModalFrame frame, string picked)
    {
        ArgumentNullException.ThrowIfNull(frame);

        frame.Close();
        OnPicked(picked);
    }
}
