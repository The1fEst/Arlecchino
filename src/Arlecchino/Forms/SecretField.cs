using Arlecchino.Atoms;

namespace Arlecchino.Forms;

/// <summary>
/// A line of text that is never shown or typed in the clear. Nothing about a string says it is a secret,
/// so the row says it instead; the atom still holds the text as it is.
/// </summary>
public sealed class SecretField : Field<string>
{
    /// <summary>Creates the row.</summary>
    /// <param name="value">The atom to read and write.</param>
    public SecretField(Atom<string> value)
        : base(value) { }
}
