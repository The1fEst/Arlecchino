using Arlecchino.Atoms;

namespace Arlecchino.Forms;

/// <summary>
/// A number moved along a track rather than typed. Both kinds of number row are over the same
/// <c>decimal</c>, so the row says which it is.
/// </summary>
public sealed class SliderField : Field<decimal>
{
    /// <summary>Creates the row.</summary>
    /// <param name="value">The atom to read and write.</param>
    public SliderField(Atom<decimal> value)
        : base(value) { }
}
