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

    private readonly Queue<KeyPress> _keys = new();
    private readonly Queue<MouseEvent> _mouse = new();
    private readonly WindowsInputTranslator _translator = new();
    private readonly IntPtr _input;
    private readonly uint _restoreMode;

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

            var raw = (mode | EnableMouseInput | EnableWindowInput | EnableExtendedFlags | EnableProcessedInput) &
                      ~(EnableQuickEdit | EnableLineInput | EnableEchoInput);

            return SetConsoleMode(input, raw) ? new WindowsConsoleInput(input, mode) : null;
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

    public KeyPress ReadKey()
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
            case KeyEventType when record.Key.IsKeyDown != 0:
                Take(Pressed(record.Key));
                return true;
            case MouseEventType:
                Translate(record.Mouse);
                return true;
            default:
                return true;
        }
    }

    private static KeyPress Pressed(KeyEventRecord key) =>
        WindowsInputTranslator.ToKeyPress(key.VirtualKeyCode, key.UnicodeChar, key.ControlKeyState);

    /// <summary>
    /// Takes one key press, and with it whatever is already waiting behind it. A run that arrived together is
    /// handed on wrapped in the paste markers a terminal elsewhere would have sent.
    /// </summary>
    /// <param name="first">The press that was just read.</param>
    private void Take(KeyPress first)
    {
        var run = new List<KeyPress> { first };
        var commanding = default(KeyPress?);

        while (WindowsPaste.Types(first) &&
               GetNumberOfConsoleInputEvents(_input, out var waiting) &&
               waiting > 0 &&
               ReadConsoleInput(_input, out var record, 1, out var read) &&
               read > 0)
        {
            if (record.EventType == MouseEventType)
            {
                Translate(record.Mouse);
                continue;
            }

            if (record.EventType != KeyEventType || record.Key.IsKeyDown == 0)
            {
                continue;
            }

            var next = Pressed(record.Key);

            if (!WindowsPaste.Types(next))
            {
                commanding = next;
                break;
            }

            run.Add(next);
        }

        foreach (var key in WindowsPaste.Reads(run) ? WindowsPaste.Wrapped(run) : run)
        {
            _keys.Enqueue(key);
        }

        if (commanding is { } pressed)
        {
            _keys.Enqueue(pressed);
        }
    }

    private void Translate(MouseEventRecord mouse)
    {
        var (row, column) = ToFrameCell(mouse.PositionY, mouse.PositionX);

        if (_translator.TryTranslateMouse(row,
                column,
                mouse.ButtonState,
                mouse.ControlKeyState,
                mouse.EventFlags,
                out var translated))
        {
            _mouse.Enqueue(translated);
        }
    }

    private static (int Row, int Column) ToFrameCell(short bufferRow, short bufferColumn)
    {
        var output = GetStdHandle(StandardOutputHandle);

        return GetConsoleScreenBufferInfo(output, out var info)
            ? (Math.Max(0, bufferRow - info.WindowTop), Math.Max(0, bufferColumn - info.WindowLeft))
            : (bufferRow, bufferColumn);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyEventRecord
    {
        public int IsKeyDown;
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
        [FieldOffset(0)] public ushort EventType;

        [FieldOffset(4)] public KeyEventRecord Key;

        [FieldOffset(4)] public MouseEventRecord Mouse;
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
    private static partial bool ReadConsoleInput(IntPtr handle, out InputRecord record, uint length, out uint count);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleScreenBufferInfo(IntPtr handle, out ScreenBufferInfo info);
}
