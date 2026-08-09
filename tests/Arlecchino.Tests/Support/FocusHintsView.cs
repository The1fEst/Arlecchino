using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Widgets;

namespace Arlecchino.Tests.Support;

public sealed class FocusHintsView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly FocusRing _ring;

    public FocusHintsView(Surface surface, ArlecchinoOptions options)
    {
        _surface = surface;
        _ring = new(options.Keymap);

        _ring.Add(new HintingWidget("f", "the first pane"));
        _ring.Add(new HintingWidget("s", "the second pane"));
    }

    public IArlecchinoFocusable Focus => _ring;

    public void Draw() => _surface.AppendLine("panes", Theme.Default);

    public ViewRoute Handle(KeyPress key) => _ring.Handle(key);

    public (string Key, string Description)[] Hints() => [("q", "leave")];

    private sealed class HintingWidget : IArlecchinoInteractiveWidget
    {
        private readonly (string Key, string Description)[] _hints;

        public HintingWidget(string key, string description)
        {
            _hints = [(key, description)];
        }

        public bool IsFocused { get; set; }

        public SurfaceRegion Draw(SurfaceRegion region) => region;

        public FocusResult Handle(KeyPress key) => FocusResult.Ignored;

        public (string Key, string Description)[] Hints() => _hints;
    }
}
