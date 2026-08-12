using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;
using Arlecchino.Modals.Setting;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// The keys of the dialogs that hold a value with a step: a slider, a color, a number in words. Going up,
/// down and to the end is one gesture for all of them.
/// </summary>
internal sealed class StepKeys
{
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;

    /// <summary>Reads keys for the dialogs that step a value.</summary>
    /// <param name="state">Where the dialog on top lives, so that it can be closed.</param>
    /// <param name="keymap">Keys to obey.</param>
    public StepKeys(ArlecchinoState state, ArlecchinoKeymap keymap)
    {
        _state = state;
        _keymap = keymap;
    }

    /// <summary>The four keys that move a bounded value.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when it was one of them.</returns>
    public bool Stepped(IBoundedModal modal, KeyPress key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            modal.Add(modal.Step);

            return true;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            modal.Add(-modal.Step);

            return true;
        }

        if (_keymap.JumpUp.Matches(key))
        {
            modal.Add(modal.LargeStep);

            return true;
        }

        if (!_keymap.JumpDown.Matches(key))
        {
            return false;
        }

        modal.Add(-modal.LargeStep);

        return true;
    }

    /// <summary>Reads a key for a slider.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    public void Slider(SliderModal modal, KeyPress key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();

            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            _state.CloseModal();
            modal.OnSubmit(modal.Value);

            return;
        }

        if (_keymap.MoveRight.Matches(key))
        {
            modal.Add(modal.Step);

            return;
        }

        if (_keymap.MoveLeft.Matches(key))
        {
            modal.Add(-modal.Step);

            return;
        }

        if (_keymap.First.Matches(key))
        {
            modal.MoveToMinimum();

            return;
        }

        if (_keymap.Last.Matches(key))
        {
            modal.MoveToMaximum();

            return;
        }

        Stepped(modal, key);
    }

    /// <summary>
    /// Reads a key for a color. Up and down walk the three channels rather than the value, since a
    /// color is three sliders stacked and which of them is being moved has to be sayable.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    public void Color(ColorModal modal, KeyPress key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();

            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            var picked = modal.Value;

            _state.CloseModal();
            modal.OnPicked(picked);

            return;
        }

        if (_keymap.MoveUp.Matches(key) || _keymap.PreviousField.Matches(key))
        {
            modal.MoveChannel(-1);

            return;
        }

        if (_keymap.MoveDown.Matches(key) || _keymap.NextField.Matches(key))
        {
            modal.MoveChannel(1);

            return;
        }

        if (_keymap.MoveLeft.Matches(key))
        {
            modal.Add(-modal.Step);

            return;
        }

        if (_keymap.MoveRight.Matches(key))
        {
            modal.Add(modal.Step);

            return;
        }

        if (_keymap.JumpUp.Matches(key))
        {
            modal.Add(modal.LargeStep);

            return;
        }

        if (_keymap.JumpDown.Matches(key))
        {
            modal.Add(-modal.LargeStep);

            return;
        }

        if (_keymap.First.Matches(key))
        {
            modal.MoveToMinimum();

            return;
        }

        if (_keymap.Last.Matches(key))
        {
            modal.MoveToMaximum();
        }
    }
}
