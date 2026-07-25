using System;

namespace Arlecchino.State;

/// <summary>One option out of a filterable list.</summary>
public sealed class ChoiceModal : OptionListModal
{
    /// <summary>Called with the chosen option.</summary>
    public required Action<string> OnPicked { get; init; }
}
