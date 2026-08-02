using System;
using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Modals.Choosing;
using Arlecchino.Modals.Setting;
using Arlecchino.Modals.Telling;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// What a click on a dialog means. Every dialog that can be clicked says where its parts were drawn
/// while it was drawing them, so this asks the dialog rather than working out where a chip must have
/// gone from the same arithmetic all over again.
/// </summary>
internal sealed class ModalClicks
{
    private readonly ArlecchinoState _state;

    /// <summary>Reads clicks for the dialogs the framework brings.</summary>
    /// <param name="state">Where the dialog on top lives, so that it can be closed.</param>
    public ModalClicks(ArlecchinoState state) => _state = state;

    /// <summary>Hands a click to whichever kind of dialog is open.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns><c>false</c> when it is a kind this does not know, which the router deals with itself.</returns>
    public bool Handle(Modal modal, MouseEvent mouse)
    {
        switch (modal)
        {
            case OptionListModal list:
                Options(list, mouse);
                return true;
            case SliderModal slider:
                Track(slider.Track, mouse, slider.SetFromFraction);
                return true;
            case ToggleModal toggle:
                Toggle(toggle, mouse);
                return true;
            case NotificationModal opened:
                Notification(opened, mouse);
                return true;
            case ColorModal color:
                Color(color, mouse);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Follows a click or a drag along a track. Pressing anywhere on it jumps the value there rather
    /// than nudging it, since a track that only nudges is a scroll bar nobody can aim.
    /// </summary>
    /// <param name="track">Where the track was drawn.</param>
    /// <param name="mouse">The event that arrived.</param>
    /// <param name="apply">What to do with how far along it landed.</param>
    private static void Track(SurfaceRegion track, MouseEvent mouse, Action<decimal> apply)
    {
        if (track.IsEmpty ||
            mouse.Action is not (MouseAction.Pressed or MouseAction.Moved) ||
            !track.Contains(mouse.Row, mouse.Column))
        {
            return;
        }

        var (_, column) = track.ToLocal(mouse.Row, mouse.Column);

        apply(track.Width <= 1 ? 0m : (decimal)column / (track.Width - 1));
    }

    private static void Toggle(ToggleModal modal, MouseEvent mouse)
    {
        if (mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left)
        {
            return;
        }

        if (modal.YesChip.Contains(mouse.Row, mouse.Column))
        {
            modal.Value = true;
        }
        else if (modal.NoChip.Contains(mouse.Row, mouse.Column))
        {
            modal.Value = false;
        }
    }

    private static void Color(ColorModal modal, MouseEvent mouse)
    {
        if (mouse.Action is not (MouseAction.Pressed or MouseAction.Moved) || mouse.Button != MouseButton.Left)
        {
            return;
        }

        for (var channel = 0; channel < modal.ChannelRows.Length; channel++)
        {
            if (!modal.ChannelRows[channel].Contains(mouse.Row, mouse.Column))
            {
                continue;
            }

            var which = (ColorChannel)channel;

            modal.Channel = which;

            Track(modal.ChannelTracks[channel], mouse, fraction => modal.SetChannelFromFraction(which, fraction));

            return;
        }
    }

    private void Notification(NotificationModal modal, MouseEvent mouse)
    {
        if (mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left)
        {
            return;
        }

        for (var index = 0; index < modal.Chips.Count; index++)
        {
            if (!modal.Chips[index].Contains(mouse.Row, mouse.Column))
            {
                continue;
            }

            modal.Index = index;

            _state.CloseModal();
            modal.Run();

            return;
        }
    }

    /// <summary>
    /// A list. The wheel walks it, and a click picks the row it landed on — but only takes it when
    /// that row was already the one under the cursor, so a click never confirms something the eye had
    /// not settled on yet.
    /// </summary>
    /// <param name="modal">The list.</param>
    /// <param name="mouse">The event that arrived.</param>
    private void Options(OptionListModal modal, MouseEvent mouse)
    {
        var matching = modal.MatchingOptions();

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                modal.Index = Math.Max(0, modal.Index - 1);
                return;
            case MouseAction.ScrolledDown:
                modal.Index = Math.Min(Math.Max(0, matching.Count - 1), modal.Index + 1);
                return;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left &&
                modal.Rows.Contains(mouse.Row, mouse.Column):
                Picked(modal, matching, mouse);
                return;
        }
    }

    private void Picked(OptionListModal modal, List<string> matching, MouseEvent mouse)
    {
        var (row, _) = modal.Rows.ToLocal(mouse.Row, mouse.Column);
        var index = modal.FirstVisible + row;

        if (index < 0 || index >= matching.Count)
        {
            return;
        }

        var settled = index == modal.Index;

        modal.Index = index;

        if (settled)
        {
            Confirm(modal, matching);
        }
    }

    private void Confirm(OptionListModal modal, List<string> matching)
    {
        var picked = matching[Math.Clamp(modal.Index, 0, matching.Count - 1)];

        switch (modal)
        {
            case ChoiceModal choice:
                _state.CloseModal();
                choice.OnPicked(picked);
                return;
            case MultiChoiceModal multiChoice:
                multiChoice.Toggle(picked);
                return;
        }
    }
}
