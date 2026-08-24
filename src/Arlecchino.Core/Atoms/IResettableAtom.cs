namespace Arlecchino.Atoms;

/// <summary>
/// An atom that knows where it began and can be put back there. One keeping no record of its own cannot
/// tell the value it was built with from the one it holds.
/// </summary>
public interface IResettableAtom
{
    /// <summary>Writes back the value the atom started at.</summary>
    /// <returns><c>false</c> when the atom has no starting value to go back to.</returns>
    bool TryReset();
}
