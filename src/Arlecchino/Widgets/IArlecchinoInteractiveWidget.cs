using Arlecchino.Focus;

namespace Arlecchino.Widgets;

/// <summary>
/// A widget that answers keys and the mouse as well as drawing. Adding one to a <see cref="FocusRing"/> is
/// the whole integration, and its members come from <see cref="IArlecchinoFocusable"/>.
/// </summary>
public interface IArlecchinoInteractiveWidget : IArlecchinoWidget, IArlecchinoFocusable;
