using System;
using Arlecchino.Rendering;

namespace Arlecchino.Tests;

public sealed class ColorSupportScope : IDisposable
{
    private readonly ColorSupport _previous = TerminalCapabilities.Color;

    public ColorSupportScope(ColorSupport support)
    {
        TerminalCapabilities.Color = support;
    }

    public void Dispose() => TerminalCapabilities.Color = _previous;
}
