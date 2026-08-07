using System;

namespace Arlecchino.Input;

internal sealed class WindowsInputTranslator
{
    private const uint MouseMoved = 0x0001;
    private const uint MouseWheeled = 0x0004;

    private const uint LeftButton = 0x0001;
    private const uint RightButton = 0x0002;
    private const uint MiddleButton = 0x0004;

    private const uint RightAltPressed = 0x0001;
    private const uint LeftAltPressed = 0x0002;
    private const uint RightControlPressed = 0x0004;
    private const uint LeftControlPressed = 0x0008;
    private const uint ShiftPressed = 0x0010;

    private uint _heldButtons;

    public static KeyPress ToKeyPress(ushort virtualKeyCode, ushort character, uint controlKeyState) =>
        new((ConsoleKey)virtualKeyCode, ToModifiers(controlKeyState), (char)character);

    public bool TryTranslateMouse(int row,
        int column,
        uint buttonState,
        uint controlKeyState,
        uint eventFlags,
        out MouseEvent mouse)
    {
        var modifiers = ToModifiers(controlKeyState);

        if ((eventFlags & MouseWheeled) != 0)
        {
            var up = (int)buttonState >> 16 > 0;
            mouse = new(up ? MouseAction.ScrolledUp : MouseAction.ScrolledDown,
                MouseButton.None,
                row,
                column,
                modifiers);
            return true;
        }

        var held = buttonState & (LeftButton | RightButton | MiddleButton);

        if ((eventFlags & MouseMoved) != 0)
        {
            var dragging = held != 0;
            mouse = dragging
                ? new(MouseAction.Moved, ToButton(held), row, column, modifiers)
                : default;
            _heldButtons = held;
            return dragging;
        }

        var pressed = held & ~_heldButtons;
        var released = _heldButtons & ~held;
        _heldButtons = held;

        if (pressed != 0)
        {
            mouse = new(MouseAction.Pressed, ToButton(pressed), row, column, modifiers);
            return true;
        }

        if (released != 0)
        {
            mouse = new(MouseAction.Released, ToButton(released), row, column, modifiers);
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

        if ((state & ShiftPressed) != 0)
        {
            modifiers |= KeyModifiers.Shift;
        }

        if ((state & (LeftAltPressed | RightAltPressed)) != 0)
        {
            modifiers |= KeyModifiers.Alt;
        }

        if ((state & (LeftControlPressed | RightControlPressed)) != 0)
        {
            modifiers |= KeyModifiers.Control;
        }

        return modifiers;
    }
}
