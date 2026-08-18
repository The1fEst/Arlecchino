using System;

namespace Arlecchino.Input;

internal sealed class WindowsInputTranslator
{
    private const uint MouseMove = 0x0001;
    private const uint MouseWheel = 0x0004;

    private const uint LeftButton = 0x0001;
    private const uint RightButton = 0x0002;
    private const uint MiddleButton = 0x0004;

    private const uint RightAlt = 0x0001;
    private const uint LeftAlt = 0x0002;
    private const uint RightControl = 0x0004;
    private const uint LeftControl = 0x0008;
    private const uint ShiftKey = 0x0010;

    private uint _heldButtons;

    /// <summary>
    /// Reads one key event from the console, dropping Shift where it did nothing but type the character the
    /// event already carries. Shift on a key that types nothing stays, as the modifier it is.
    /// </summary>
    /// <param name="virtualKeyCode">The virtual key the console reported.</param>
    /// <param name="character">The character that key typed, or zero.</param>
    /// <param name="controlKeyState">The modifier flags the console reported.</param>
    /// <returns>The key press, as the rest of the library reads one.</returns>
    public static KeyPress ToKeyPress(ushort virtualKeyCode, ushort character, uint controlKeyState) =>
        new(Named(virtualKeyCode, character), Held(character, controlKeyState), (char)character);

    /// <summary>
    /// The key behind an event. What a terminal answers with is relayed character by character with no key
    /// named, so an escape among it is named here.
    /// </summary>
    /// <param name="virtualKeyCode">The virtual key the console reported, which is zero for what it relays.</param>
    /// <param name="character">The character the event carries.</param>
    /// <returns>The key to hand on.</returns>
    private static ConsoleKey Named(ushort virtualKeyCode, ushort character) =>
        virtualKeyCode == 0 && character == '\e' ? ConsoleKey.Escape : (ConsoleKey)virtualKeyCode;

    private static KeyModifiers Held(ushort character, uint controlKeyState)
    {
        var modifiers = ToModifiers(controlKeyState);
        var typedCharacter = (char)character;

        return modifiers == KeyModifiers.Shift && typedCharacter != '\0' && !char.IsControl(typedCharacter)
            ? KeyModifiers.None
            : modifiers;
    }

    public bool TryTranslateMouse(int row,
        int column,
        uint buttonState,
        uint controlKeyState,
        uint eventFlags,
        out MouseEvent mouse)
    {
        var modifiers = ToModifiers(controlKeyState);

        if ((eventFlags & MouseWheel) != 0)
        {
            var up = (int)buttonState >> 16 > 0;
            mouse = new(up ? MouseAction.ScrolledUp : MouseAction.ScrolledDown,
                MouseButton.None,
                row,
                column,
                modifiers);
            return true;
        }

        var heldButtons = buttonState & (LeftButton | RightButton | MiddleButton);

        if ((eventFlags & MouseMove) != 0)
        {
            var dragging = heldButtons != 0;
            mouse = dragging
                ? new(MouseAction.Moved, ToButton(heldButtons), row, column, modifiers)
                : default;
            _heldButtons = heldButtons;
            return dragging;
        }

        var pressedButtons = heldButtons & ~_heldButtons;
        var releasedButtons = _heldButtons & ~heldButtons;
        _heldButtons = heldButtons;

        if (pressedButtons != 0)
        {
            mouse = new(MouseAction.Pressed, ToButton(pressedButtons), row, column, modifiers);
            return true;
        }

        if (releasedButtons != 0)
        {
            mouse = new(MouseAction.Released, ToButton(releasedButtons), row, column, modifiers);
            return true;
        }

        mouse = default;
        return false;
    }

    private static MouseButton ToButton(uint buttons) => buttons switch
    {
        _ when (buttons & LeftButton) != 0 => MouseButton.Left,
        _ when (buttons & RightButton) != 0 => MouseButton.Right,
        _ when (buttons & MiddleButton) != 0 => MouseButton.Middle,
        _ => MouseButton.None,
    };

    private static KeyModifiers ToModifiers(uint state)
    {
        var modifiers = default(KeyModifiers);

        if ((state & ShiftKey) != 0)
        {
            modifiers |= KeyModifiers.Shift;
        }

        if ((state & (LeftAlt | RightAlt)) != 0)
        {
            modifiers |= KeyModifiers.Alt;
        }

        if ((state & (LeftControl | RightControl)) != 0)
        {
            modifiers |= KeyModifiers.Control;
        }

        return modifiers;
    }
}
