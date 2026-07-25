using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Arlecchino.Input;

namespace Arlecchino;

internal sealed partial class WindowsConsoleInput
{
    private const int StandardInputHandle = -10;
    private const int StandardOutputHandle = -11;

    private const uint EnableProcessedInput = 0x0001;
    private const uint EnableLineInput = 0x0002;
    private const uint EnableEchoInput = 0x0004;
    private const uint EnableWindowInput = 0x0008;
    private const uint EnableMouseInput = 0x0010;
    private const uint EnableQuickEdit = 0x0040;
    private const uint EnableExtendedFlags = 0x0080;

    private const ushort KeyEventType = 0x0001;
    private const ushort MouseEventType = 0x0002;

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

    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly Queue<MouseEvent> _mouse = new();
    private readonly IntPtr _input;
    private readonly uint _restoreMode;

    private uint _heldButtons;

    private WindowsConsoleInput(IntPtr input, uint restoreMode)
    {
        _input = input;
        _restoreMode = restoreMode;
    }

    [SupportedOSPlatform("windows")]
    public static WindowsConsoleInput? TryStart()
    {
        try
        {
            var input = GetStdHandle(StandardInputHandle);
            if (input == IntPtr.Zero || input == new IntPtr(-1) || !GetConsoleMode(input, out var mode))
            {
                return null;
            }

            var wanted = (mode | EnableMouseInput | EnableWindowInput | EnableExtendedFlags | EnableProcessedInput) &
                         ~(EnableQuickEdit | EnableLineInput | EnableEchoInput);

            return SetConsoleMode(input, wanted) ? new WindowsConsoleInput(input, mode) : null;
        }
        catch (DllNotFoundException)
        {
            return null;
        }
        catch (EntryPointNotFoundException)
        {
            return null;
        }
    }

    public bool KeyAvailable
    {
        get
        {
            DrainPending();
            return _keys.Count > 0;
        }
    }

    public bool MouseAvailable
    {
        get
        {
            DrainPending();
            return _mouse.Count > 0;
        }
    }

    public ConsoleKeyInfo ReadKey()
    {
        while (_keys.Count == 0)
        {
            if (!ReadOne())
            {
                return default;
            }
        }

        return _keys.Dequeue();
    }

    public MouseEvent ReadMouse() => _mouse.Dequeue();

    public void Stop() => SetConsoleMode(_input, _restoreMode);

    private void DrainPending()
    {
        while (GetNumberOfConsoleInputEvents(_input, out var waiting) && waiting > 0)
        {
            if (!ReadOne())
            {
                return;
            }
        }
    }

    private bool ReadOne()
    {
        if (!ReadConsoleInput(_input, out var record, 1, out var read) || read == 0)
        {
            return false;
        }

        switch (record.EventType)
        {
            case KeyEventType when record.Key.KeyDown != 0:
                _keys.Enqueue(ToKeyInfo(record.Key));
                return true;
            case MouseEventType:
                Translate(record.Mouse);
                return true;
            default:
                return true;
        }
    }

    private static ConsoleKeyInfo ToKeyInfo(KeyEventRecord key) =>
        new(
            (char)key.UnicodeChar,
            (ConsoleKey)key.VirtualKeyCode,
            (key.ControlKeyState & ShiftPressed) != 0,
            (key.ControlKeyState & (LeftAltPressed | RightAltPressed)) != 0,
            (key.ControlKeyState & (LeftControlPressed | RightControlPressed)) != 0);

    private void Translate(MouseEventRecord mouse)
    {
        var (row, column) = ToFrameCell(mouse.PositionY, mouse.PositionX);
        var modifiers = ToModifiers(mouse.ControlKeyState);

        if ((mouse.EventFlags & MouseWheeled) != 0)
        {
            var up = (int)mouse.ButtonState >> 16 > 0;
            _mouse.Enqueue(new(up ? MouseAction.ScrolledUp : MouseAction.ScrolledDown, MouseButton.None,
                row, column, modifiers));
            return;
        }

        var held = mouse.ButtonState & (LeftButton | RightButton | MiddleButton);

        if ((mouse.EventFlags & MouseMoved) != 0)
        {
            if (held != 0)
            {
                _mouse.Enqueue(new(MouseAction.Moved, ToButton(held), row, column, modifiers));
            }

            _heldButtons = held;
            return;
        }

        var pressed = held & ~_heldButtons;
        var released = _heldButtons & ~held;
        _heldButtons = held;

        if (pressed != 0)
        {
            _mouse.Enqueue(new(MouseAction.Pressed, ToButton(pressed), row, column, modifiers));
            return;
        }

        if (released != 0)
        {
            _mouse.Enqueue(new(MouseAction.Released, ToButton(released), row, column, modifiers));
        }
    }

    private static (int Row, int Column) ToFrameCell(short bufferRow, short bufferColumn)
    {
        var output = GetStdHandle(StandardOutputHandle);

        return GetConsoleScreenBufferInfo(output, out var info)
            ? (Math.Max(0, bufferRow - info.WindowTop), Math.Max(0, bufferColumn - info.WindowLeft))
            : (bufferRow, bufferColumn);
    }

    private static MouseButton ToButton(uint buttons) => buttons switch
    {
        _ when (buttons & LeftButton) != 0 => MouseButton.Left,
        _ when (buttons & RightButton) != 0 => MouseButton.Right,
        _ when (buttons & MiddleButton) != 0 => MouseButton.Middle,
        _ => MouseButton.None,
    };

    private static ConsoleModifiers ToModifiers(uint state)
    {
        var modifiers = default(ConsoleModifiers);

        if ((state & ShiftPressed) != 0)
        {
            modifiers |= ConsoleModifiers.Shift;
        }

        if ((state & (LeftAltPressed | RightAltPressed)) != 0)
        {
            modifiers |= ConsoleModifiers.Alt;
        }

        if ((state & (LeftControlPressed | RightControlPressed)) != 0)
        {
            modifiers |= ConsoleModifiers.Control;
        }

        return modifiers;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyEventRecord
    {
        public int KeyDown;
        public ushort RepeatCount;
        public ushort VirtualKeyCode;
        public ushort VirtualScanCode;
        public ushort UnicodeChar;
        public uint ControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseEventRecord
    {
        public short PositionX;
        public short PositionY;
        public uint ButtonState;
        public uint ControlKeyState;
        public uint EventFlags;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputRecord
    {
        [FieldOffset(0)]
        public ushort EventType;

        [FieldOffset(4)]
        public KeyEventRecord Key;

        [FieldOffset(4)]
        public MouseEventRecord Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ScreenBufferInfo
    {
        public short BufferWidth;
        public short BufferHeight;
        public short CursorX;
        public short CursorY;
        public ushort Attributes;
        public short WindowLeft;
        public short WindowTop;
        public short WindowRight;
        public short WindowBottom;
        public short MaximumWindowWidth;
        public short MaximumWindowHeight;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int handle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr handle, out uint mode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr handle, uint mode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetNumberOfConsoleInputEvents(IntPtr handle, out uint count);

    [LibraryImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ReadConsoleInput(IntPtr handle, out InputRecord record, uint length, out uint read);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleScreenBufferInfo(IntPtr handle, out ScreenBufferInfo info);
}
