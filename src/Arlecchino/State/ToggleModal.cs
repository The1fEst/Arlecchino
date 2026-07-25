using System;
using Arlecchino.Rendering;

namespace Arlecchino.State;

/// <summary>A yes-or-no answer, flipped with the arrows or picked by clicking one of the two chips.</summary>
public sealed class ToggleModal : Modal
{
    /// <summary>The answer as it stands.</summary>
    public bool Value { get; set; }

    /// <summary>Called with the answer that was confirmed.</summary>
    public required Action<bool> OnSubmit { get; init; }

    /// <summary>Where the affirmative chip was drawn last frame, used to turn a click into an answer.</summary>
    public Region YesChip { get; set; }

    /// <summary>Where the negative chip was drawn last frame.</summary>
    public Region NoChip { get; set; }
}
