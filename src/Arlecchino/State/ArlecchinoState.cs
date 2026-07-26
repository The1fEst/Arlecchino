using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.Modals;

namespace Arlecchino.State;

/// <summary>
/// State that outlives a single screen: the output line, the dialog that is open, and a pending file
/// picker request. Derive from it to hang application state that every screen reads.
/// </summary>
public class ArlecchinoState
{
    private readonly Repaint _repaint;

    private readonly List<Modal> _modals = [];

    private string _output = string.Empty;
    private FilePickerRequest? _filePicker;

    /// <summary>Creates the state.</summary>
    /// <param name="repaint">Signal raised whenever any of this changes.</param>
    public ArlecchinoState(Repaint repaint)
    {
        _repaint = repaint;
    }

    /// <summary>
    /// The status line at the bottom of the frame. Not cleared for you; the palette clears it when
    /// it opens.
    /// </summary>
    public string Output
    {
        get => _output;
        set
        {
            _output = value;
            _repaint.Request();
        }
    }

    /// <summary>
    /// The dialog on top, or <c>null</c> when none is open. It takes every key while it is there.
    /// Assigning replaces whatever was open, however deep it was stacked; use <see cref="PushModal"/>
    /// to open one over another instead.
    /// </summary>
    public Modal? Modal
    {
        get => _modals.Count == 0 ? null : _modals[^1];
        set
        {
            _modals.Clear();

            if (value is not null)
            {
                _modals.Add(value);
            }

            _repaint.Request();
        }
    }

    /// <summary>
    /// Every open dialog, bottom first. Drawing goes through this so the ones underneath stay visible
    /// behind the top one.
    /// </summary>
    public IReadOnlyList<Modal> Modals => _modals;

    /// <summary>
    /// Opens a dialog over whatever is already open, which is how a callback asks a follow-up question
    /// without losing what the user was in the middle of. Closing it uncovers the one underneath.
    /// </summary>
    /// <param name="modal">The dialog to open.</param>
    public void PushModal(Modal modal)
    {
        _modals.Add(modal);
        _repaint.Request();
    }

    /// <summary>
    /// What the file picker should show. Fill it in, then navigate to <c>Routes.FilePicker</c>; it
    /// is cleared when the picker finishes either way.
    /// </summary>
    public FilePickerRequest? FilePicker
    {
        get => _filePicker;
        set
        {
            _filePicker = value;
            _repaint.Request();
        }
    }

    /// <summary>
    /// Folder the picker ended in. Pass it as the next starting path to resume where the user left
    /// off.
    /// </summary>
    public string PickerLastFolder { get; set; } = string.Empty;

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
    /// Asks for a colour with a swatch and three sliders. Channels are whole numbers, so a colour
    /// that goes in can come back shifted by a unit or two.
    /// </summary>
    /// <param name="title">Title of the dialog.</param>
    /// <param name="initial">Colour the sliders start on.</param>
    /// <param name="onPicked">Called with the chosen colour.</param>
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
    public void CloseModal()
    {
        if (_modals.Count == 0)
        {
            return;
        }

        _modals.RemoveAt(_modals.Count - 1);
        _repaint.Request();
    }

    /// <summary>Closes every open dialog at once, however deep they are stacked.</summary>
    public void CloseAllModals()
    {
        if (_modals.Count == 0)
        {
            return;
        }

        _modals.Clear();
        _repaint.Request();
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
