using System;

namespace Arlecchino.Rendering;

/// <summary>
/// Where text or a block sits inside the space it is drawn into. Horizontal and vertical flags
/// combine, so <c>Align.Right | Align.Bottom</c> anchors to a corner.
/// </summary>
[Flags]
public enum Align : byte
{
    /// <summary>Against the left edge of the content area.</summary>
    Left = 1 << 0,

    /// <summary>Centred horizontally in the content area.</summary>
    Center = 1 << 1,

    /// <summary>Against the right edge of the content area.</summary>
    Right = 1 << 2,

    /// <summary>Against the top edge. Only block and region calls honour the vertical flags.</summary>
    Top = 1 << 3,

    /// <summary>Centred vertically.</summary>
    Middle = 1 << 4,

    /// <summary>Against the bottom edge.</summary>
    Bottom = 1 << 5,
}
