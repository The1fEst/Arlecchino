namespace Arlecchino.State;

/// <summary>
/// One recorded change of one atom, as kept by <see cref="StateHistory"/>. Replaying it does not
/// record a new step.
/// </summary>
public interface IStateEdit
{
    /// <summary>The atom this edit belongs to.</summary>
    object? Owner { get; }

    /// <summary>Puts the value back to what it was before the edit.</summary>
    void Undo();

    /// <summary>Applies the edit again.</summary>
    void Redo();
}
