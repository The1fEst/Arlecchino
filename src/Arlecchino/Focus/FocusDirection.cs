namespace Arlecchino.Focus;

/// <summary>
/// Which way the focus is being asked to move, on the key that walks the fields of a screen.
/// </summary>
public enum FocusDirection
{
    /// <summary>Toward the next field, the way <c>Tab</c> goes.</summary>
    Next,

    /// <summary>Toward the previous one, the way <c>Shift+Tab</c> goes.</summary>
    Previous,
}
