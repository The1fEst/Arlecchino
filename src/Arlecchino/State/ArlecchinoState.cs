using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Local;
using Arlecchino.Diagnostics;
using Arlecchino.Rendering.Colors;
using Arlecchino.Modals;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Choosing;
using Arlecchino.Modals.Setting;
using Arlecchino.Modals.Telling;

namespace Arlecchino.State;

/// <summary>
/// State that outlives a single screen: the output line, the dialog that is open, and a pending file
/// picker request. Derive from it to hang application state that every screen reads.
///
/// A frame reads all of it, so all of it is written on the drawing thread — the <c>Request…</c> methods
/// included, since each of them opens a dialog. Anything arriving on a timer, a task or a socket hands
/// the change over with <see cref="FrameThread.Post"/>, which runs it just before the next frame; only
/// <see cref="Invalidate"/> may be called from anywhere.
///
/// The stack of dialogs is a <see cref="LocalAtomsList{T}"/>, so opening or closing one asks for a frame
/// by itself. It is outside the undo history: stepping back through what was typed should not reopen a
/// dialog that was answered.
/// </summary>
public class ArlecchinoState
{
    private static readonly string WritingOutput = FrameMembers.Of<ArlecchinoState>(nameof(Output));
    private static readonly string OpeningModal = FrameMembers.Of<ArlecchinoState>(nameof(Modal));
    private static readonly string Pushing = FrameMembers.Of<ArlecchinoState>(nameof(PushModal));
    private static readonly string Closing = FrameMembers.Of<ArlecchinoState>(nameof(CloseModal));
    private static readonly string ClosingAll = FrameMembers.Of<ArlecchinoState>(nameof(CloseAllModals));
    private static readonly string Picking = FrameMembers.Of<ArlecchinoState>(nameof(FilePicker));

    private static readonly string Remembering =
        FrameMembers.Of<ArlecchinoState>(nameof(PickerLastFolder));

    private readonly Repaint _repaint;
    private readonly Notifications _notifications;

    private readonly LocalAtomsList<Modal> _modals = new();

    /// <summary>Creates the state.</summary>
    /// <param name="repaint">Signal raised whenever anything here changes.</param>
    /// <param name="notifications">Holds the output row and the notifications screen behind it.</param>
    public ArlecchinoState(Repaint repaint, Notifications notifications)
    {
        _repaint = repaint;
        _notifications = notifications;
    }

    /// <summary>
    /// The status line at the bottom of the frame. Writing to it raises a notification, so the line
    /// clears itself after <c>ArlecchinoOptions.NotificationTimeout</c> and the message stays
    /// readable afterward on the notifications screen. An empty string clears the row at once.
    /// </summary>
    public string Output
    {
        get => _notifications.Current?.Line ?? string.Empty;
        set
        {
            FrameThread.Verify(WritingOutput);
            _notifications.Notify(value);
        }
    }

    /// <summary>What the application has said lately, and the screen behind the output row.</summary>
    public Notifications Notifications => _notifications;

    /// <summary>
    /// The dialog on top, or <c>null</c> when none is open. It takes every key while it is there.
    /// Assigning replaces whatever was open, however deep it was stacked; use <see cref="PushModal"/>
    /// to open one over another instead.
    ///
    /// Opened on the drawing thread: a dialog that appeared halfway through a frame would be drawn
    /// into a surface that has already been measured without it. Hand it over with
    /// <see cref="FrameThread.Post"/> from anywhere else.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public Modal? Modal
    {
        get => _modals.Count == 0 ? null : _modals[^1];
        set
        {
            FrameThread.Verify(OpeningModal);
            _modals.Reset(value is null ? [] : [value]);
        }
    }

    /// <summary>
    /// Every open dialog, bottom first. Drawing goes through this, so the ones underneath stay visible
    /// behind the top one.
    ///
    /// A live view of the stack rather than a copy, and read-only all the way down. A widget handed it once
    /// draws whatever is open on every later frame, and there is no cast that gets a caller back to the list
    /// underneath.
    /// </summary>
    public IReadOnlyList<Modal> Modals => _modals.Value;

    /// <summary>
    /// Opens a dialog over whatever is already open, which is how a callback asks a follow-up question
    /// without losing what the user was in the middle of. Closing it uncovers the one underneath.
    /// </summary>
    /// <param name="modal">The dialog to open.</param>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public void PushModal(Modal modal)
    {
        FrameThread.Verify(Pushing);
        _modals.Add(modal);
    }

    /// <summary>
    /// What the file picker should show. Fill it in, then navigate to <c>Routes.FilePicker</c>; it
    /// is cleared when the picker finishes either way. Written on the drawing thread, as
    /// <see cref="Modal"/> is.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public FilePickerRequest? FilePicker
    {
        get;
        set
        {
            FrameThread.Verify(Picking);
            field = value;
            _repaint.Request();
        }
    }

    /// <summary>
    /// Folder the picker ended in. Pass it as the next starting path to resume where the user left
    /// off. Written on the drawing thread, as <see cref="Modal"/> is.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public string PickerLastFolder
    {
        get;
        set
        {
            FrameThread.Verify(Remembering);
            field = value;
        }
    } = string.Empty;

    /// <summary>
    /// Asks for a repaint. Needed only for changes the framework cannot see — a field mutated from
    /// outside, or data that arrived on a timer.
    /// </summary>
    public void Invalidate() => _repaint.Request();

    /// <summary>Asks for a line of text.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Text the field starts with.</param>
    /// <param name="validate">
    /// Checked on confirm; return a message to keep the dialog open, or <c>null</c> to accept.
    /// </param>
    /// <param name="onSubmit">Called with the accepted text.</param>
    public void RequestText(string title, string initial, Func<string, string?>? validate, Action<string> onSubmit)
    {
        Modal = new TextModal
        {
            Title = title,
            Text = initial,
            Validate = validate,
            OnSubmit = onSubmit,
        };
    }

    /// <summary>
    /// Asks for a secret. The field shows dots, but the text handed to the callback is untouched.
    /// </summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="onSubmit">Called with what was typed.</param>
    public void RequestPassword(string title, Action<string> onSubmit)
    {
        Modal = new TextModal
        {
            Title = title,
            Masked = true,
            OnSubmit = onSubmit,
        };
    }

    /// <summary>Asks for an email address, checked before the dialog will close.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Text the field starts with.</param>
    /// <param name="onSubmit">Called with the accepted address.</param>
    public void RequestEmail(string title, string initial, Action<string> onSubmit)
    {
        Modal = new TextModal
        {
            Title = title,
            Text = initial,
            Format = TextFormat.Email,
            OnSubmit = onSubmit,
        };
    }

    /// <summary>Asks for an http or https link, checked before the dialog will close.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Text the field starts with, often just the scheme.</param>
    /// <param name="onSubmit">Called with the accepted link.</param>
    public void RequestUrl(string title, string initial, Action<string> onSubmit)
    {
        Modal = new TextModal
        {
            Title = title,
            Text = initial,
            Format = TextFormat.Url,
            OnSubmit = onSubmit,
        };
    }

    /// <summary>Asks for a date, edited one segment at a time.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Date the field starts on.</param>
    /// <param name="onSubmit">Called with the chosen date.</param>
    public void RequestDate(string title, DateOnly initial, Action<DateOnly> onSubmit)
    {
        Modal = new DateModal
        {
            Title = title,
            Value = initial,
            OnSubmit = onSubmit,
        };
    }

    /// <summary>Asks for a time of day, edited one segment at a time.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Time the field starts on.</param>
    /// <param name="onSubmit">Called with the chosen time.</param>
    public void RequestTime(string title, TimeOnly initial, Action<TimeOnly> onSubmit)
    {
        Modal = new TimeModal
        {
            Title = title,
            Value = initial,
            OnSubmit = onSubmit,
        };
    }

    /// <summary>
    /// Asks for a color with a swatch and three sliders. Channels are whole numbers, so a color
    /// that goes in can come back shifted by a unit or two.
    /// </summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Color the sliders start on.</param>
    /// <param name="onPicked">Called with the chosen color.</param>
    public void RequestColor(string title, Rgb initial, Action<Rgb> onPicked)
    {
        var modal = new ColorModal
        {
            Title = title,
            OnPicked = onPicked,
        };

        modal.SetValue(initial);
        Modal = modal;
    }

    /// <summary>
    /// Asks for a number within bounds. Typing is restricted to digits, and stepping keys clamp to
    /// the range.
    /// </summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Value the field starts on, clamped to the range.</param>
    /// <param name="minimum">Lowest value allowed.</param>
    /// <param name="maximum">Highest value allowed.</param>
    /// <param name="onSubmit">Called with the accepted number.</param>
    public void RequestNumber(string title, decimal initial, decimal minimum, decimal maximum, Action<decimal> onSubmit)
    {
        var modal = new NumberModal
        {
            Title = title,
            Minimum = minimum,
            Maximum = maximum,
            OnSubmit = onSubmit,
        };

        modal.Text = modal.FormatNumber(Math.Clamp(initial, minimum, maximum));
        Modal = modal;
    }

    /// <summary>Asks for a number on a track, adjusted with the arrows rather than typed.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Value the slider starts on, clamped to the range.</param>
    /// <param name="minimum">Left end of the track.</param>
    /// <param name="maximum">Right end of the track.</param>
    /// <param name="onSubmit">Called with the chosen value.</param>
    public void RequestSlider(string title, decimal initial, decimal minimum, decimal maximum, Action<decimal> onSubmit)
    {
        Modal = new SliderModal
        {
            Title = title,
            Minimum = minimum,
            Maximum = maximum,
            Value = Math.Clamp(initial, minimum, maximum),
            OnSubmit = onSubmit,
        };
    }

    /// <summary>Asks a yes-or-no question.</summary>
    /// <param name="title">The question.</param>
    /// <param name="initial">Which chip starts selected.</param>
    /// <param name="onSubmit">Called with the answer.</param>
    public void RequestToggle(string title, bool initial, Action<bool> onSubmit)
    {
        Modal = new ToggleModal
        {
            Title = title,
            Value = initial,
            OnSubmit = onSubmit,
        };
    }

    /// <summary>
    /// Asks for several lines of text. <c>Enter</c> starts a new line, so the text is confirmed with
    /// the <c>Submit</c> key — <c>Ctrl+Enter</c> unless the keymap says otherwise.
    /// </summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Text the editor starts with.</param>
    /// <param name="onSubmit">Called with the accepted text.</param>
    /// <param name="validate">
    /// Checked on submit; return a message to keep the dialog open, or <c>null</c> to accept.
    /// </param>
    /// <param name="visibleRows">How many rows to show before the text starts scrolling.</param>
    public void RequestTextArea(
        string title,
        string initial,
        Action<string> onSubmit,
        Func<string, string?>? validate = null,
        int visibleRows = 8)
    {
        Modal = new TextAreaModal
        {
            Title = title,
            Text = initial,
            OnSubmit = onSubmit,
            Validate = validate,
            VisibleRows = visibleRows,
        };
    }

    /// <summary>Shows a message with nothing to fill in; any of the closing keys dismisses it.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="text">What to say. Long text wraps inside the box.</param>
    /// <param name="onClosed">Called once it is dismissed.</param>
    public void RequestMessage(string title, string text, Action? onClosed = null)
    {
        Modal = new MessageModal
        {
            Title = title,
            Text = text,
            OnClosed = onClosed,
        };
    }

    /// <summary>
    /// Asks a question that has to be confirmed before something happens. The negative answer starts
    /// selected, so a stray <c>Enter</c> cancels rather than deletes.
    /// </summary>
    /// <param name="title">The question.</param>
    /// <param name="onConfirmed">Called only when the answer was yes.</param>
    public void RequestConfirmation(string title, Action onConfirmed)
    {
        Modal = new ToggleModal
        {
            Title = title,
            Value = false,
            OnSubmit = answer =>
            {
                if (answer)
                {
                    onConfirmed();
                }
            },
        };
    }

    /// <summary>Asks for one option out of a list that can be filtered by typing.</summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="options">What to choose from.</param>
    /// <param name="onPicked">Called with the chosen option.</param>
    /// <param name="current">Option to start on; the first one when it is not in the list.</param>
    public void RequestChoice(string title, IReadOnlyList<string> options, Action<string> onPicked, string current = "")
    {
        Modal = new ChoiceModal
        {
            Title = title,
            Options = options,
            OnPicked = onPicked,
            Index = IndexOf(options, current),
        };
    }

    /// <summary>
    /// Asks for any number of options. Marks survive filtering, and the result comes back in the
    /// order of <paramref name="options"/> rather than the order they were marked.
    /// </summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="options">What to choose from.</param>
    /// <param name="selected">Options marked to begin with.</param>
    /// <param name="onSubmit">Called with everything marked.</param>
    public void RequestMultiChoice(
        string title,
        IReadOnlyList<string> options,
        IReadOnlyList<string> selected,
        Action<IReadOnlyList<string>> onSubmit)
    {
        Modal = new MultiChoiceModal
        {
            Title = title,
            Options = options,
            Selected = new(selected, StringComparer.Ordinal),
            OnSubmit = onSubmit,
        };
    }

    /// <summary>
    /// Closes the dialog on top, uncovering whatever it was opened over. Submitting, picking and
    /// cancelling already do this, so it is only needed to dismiss one from the outside.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public void CloseModal()
    {
        FrameThread.Verify(Closing);

        if (_modals.Count > 0)
        {
            _modals.RemoveAt(_modals.Count - 1);
        }
    }

    /// <summary>Closes every open dialog at once, however deep they are stacked.</summary>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public void CloseAllModals()
    {
        FrameThread.Verify(ClosingAll);

        if (_modals.Count > 0)
        {
            _modals.Clear();
        }
    }

    private static int IndexOf(IReadOnlyList<string> options, string current)
    {
        for (var i = 0; i < options.Count; i++)
        {
            if (options[i].Equals(current, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return 0;
    }
}
