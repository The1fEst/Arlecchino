using System;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Widgets;

namespace Arlecchino.Tests;

public sealed class ProbeWidget : IArlecchinoInteractiveWidget
{
    private readonly ArlecchinoKeymap _keymap;

    private SurfaceRegion _drawn;

    public ProbeWidget(ArlecchinoKeymap keymap)
    {
        _keymap = keymap;
    }

    public string Text { get; set; } = "probe widget";

    public bool IsFocused { get; set; }

    public SurfaceRegion Draw(SurfaceRegion region)
    {
        _drawn = region;
        region.WriteLine(0, Text, IsFocused ? Theme.Active : Theme.Muted);

        return region.Rows(1, region.Height - 1);
    }

    public FocusResult Handle(ConsoleKeyInfo key) =>
        _keymap.Confirm.Matches(key) ? FocusResult.Handled : FocusResult.Ignored;

    public FocusResult HandleMouse(MouseEvent mouse) =>
        mouse.IsLeftClick && _drawn.Contains(mouse.Row, mouse.Column)
            ? FocusResult.Handled
            : FocusResult.Ignored;
}

public sealed class ProbeReadoutWidget : IArlecchinoWidget
{
    private readonly ProbeStore _store;

    public ProbeReadoutWidget(ProbeStore store)
    {
        _store = store;
    }

    public SurfaceRegion Draw(SurfaceRegion region)
    {
        region.WriteLine(0, _store.Name.Value, Theme.Muted);

        return region.Rows(1, region.Height - 1);
    }
}
