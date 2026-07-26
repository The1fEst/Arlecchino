using Arlecchino.Focus;

namespace Arlecchino.Widgets;

/// <summary>
/// A widget that answers keys and the mouse as well as drawing: a list, a table, a set of tabs, a
/// form. Adding one to a <see cref="FocusRing"/> is the whole integration — the ring cycles the
/// focus with <c>Tab</c>, hands keys to whichever widget holds it, and moves the focus to the widget
/// that claims a click.
///
/// The members come from <see cref="IArlecchinoFocusable"/>: <c>IsFocused</c> for drawing the difference, and
/// <c>Handle</c> / <c>HandleMouse</c> returning a <see cref="FocusResult"/> that says whether the
/// event was claimed and whether it navigates.
/// </summary>
public interface IArlecchinoInteractiveWidget : IArlecchinoWidget, IArlecchinoFocusable;
