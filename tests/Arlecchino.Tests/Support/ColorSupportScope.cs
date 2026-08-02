using System;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Tests.Support;

public sealed class ColorSupportScope : IDisposable
{
    private readonly ColorSupport _previous = TerminalCapabilities.Color;

    public ColorSupportScope(ColorSupport support)
    {
        TerminalCapabilities.Color = support;
    }

    public void Dispose() => TerminalCapabilities.Color = _previous;
}
