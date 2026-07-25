using System;
using System.Collections.Generic;
using System.Globalization;

namespace Arlecchino.Input;

/// <summary>
/// Reads the escape sequences a terminal sends for mouse reports and for keys that have no
/// character — arrows, function keys, and their modified forms.
/// </summary>
public sealed class EscapeSequenceParser
{
    private const int WheelFlag = 64;
    private const int MotionFlag = 32;
    private const int ShiftFlag = 4;
    private const int AltFlag = 8;
    private const int ControlFlag = 16;

    /// <summary>Reads an SGR mouse report, the <c>&lt;flags;column;rowM</c> form.</summary>
    /// <param name="sequence">Body of the sequence, without the leading escape and bracket.</param>
    /// <param name="mouse">The event, with coordinates converted to zero-based cells.</param>
    /// <returns><c>true</c> when the sequence was a mouse report.</returns>
    public static bool TryParseMouse(string sequence, out MouseEvent mouse)
    {
        mouse = default;

        if (!sequence.StartsWith("<", StringComparison.Ordinal) || sequence.Length < 6)
        {
            return false;
        }

        var final = sequence[^1];
        if (final is not ('M' or 'm'))
        {
            return false;
        }

        var parts = sequence[1..^1].Split(';');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var flags) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var column) ||
            !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var row))
        {
            return false;
        }

        var modifiers = ModifiersOf(flags);
        var buttonBits = flags & 3;

        if ((flags & WheelFlag) != 0)
        {
            mouse = new(
                buttonBits == 0 ? MouseAction.ScrolledUp : MouseAction.ScrolledDown,
                MouseButton.None,
                row - 1,
                column - 1,
                modifiers);
            return true;
        }

        var button = buttonBits switch
        {
            0 => MouseButton.Left,
            1 => MouseButton.Middle,
            _ => MouseButton.Right,
        };

        var action = final == 'm'
            ? MouseAction.Released
            : (flags & MotionFlag) != 0
                ? MouseAction.Moved
                : MouseAction.Pressed;

        mouse = new(action, action == MouseAction.Moved && buttonBits == 3 ? MouseButton.None : button,
            row - 1, column - 1, modifiers);
        return true;
    }

    /// <summary>
    /// Reads a cursor or function key sequence, including the <c>1;5C</c> form that carries
    /// modifiers.
    /// </summary>
    /// <param name="sequence">Body of the sequence, without the leading escape and bracket.</param>
    /// <param name="key">The key it stands for.</param>
    /// <returns><c>true</c> when the sequence was a known key.</returns>
    public static bool TryParseKey(string sequence, out ConsoleKeyInfo key)
    {
        key = default;

        if (sequence.Length == 0)
        {
            return false;
        }

        var final = sequence[^1];
        var body = sequence[..^1];
        var parameters = SplitParameters(body);
        var modifiers = parameters.Count > 1 ? ModifiersOfParameter(parameters[1]) : default;

        var consoleKey = final switch
        {
            'A' => ConsoleKey.UpArrow,
            'B' => ConsoleKey.DownArrow,
            'C' => ConsoleKey.RightArrow,
            'D' => ConsoleKey.LeftArrow,
            'H' => ConsoleKey.Home,
            'F' => ConsoleKey.End,
            'P' => ConsoleKey.F1,
            'Q' => ConsoleKey.F2,
            'R' => ConsoleKey.F3,
            'S' => ConsoleKey.F4,
            'Z' => ConsoleKey.Tab,
            '~' => KeyOfNumber(parameters.Count > 0 ? parameters[0] : 0),
            _ => default,
        };

        if (consoleKey == default)
        {
            return false;
        }

        if (final == 'Z')
        {
            modifiers |= ConsoleModifiers.Shift;
        }

        key = new('\0', consoleKey, modifiers.HasFlag(ConsoleModifiers.Shift),
            modifiers.HasFlag(ConsoleModifiers.Alt), modifiers.HasFlag(ConsoleModifiers.Control));
        return true;
    }

    private static ConsoleKey KeyOfNumber(int number) => number switch
    {
        1 or 7 => ConsoleKey.Home,
        2 => ConsoleKey.Insert,
        3 => ConsoleKey.Delete,
        4 or 8 => ConsoleKey.End,
        5 => ConsoleKey.PageUp,
        6 => ConsoleKey.PageDown,
        11 => ConsoleKey.F1,
        12 => ConsoleKey.F2,
        13 => ConsoleKey.F3,
        14 => ConsoleKey.F4,
        15 => ConsoleKey.F5,
        17 => ConsoleKey.F6,
        18 => ConsoleKey.F7,
        19 => ConsoleKey.F8,
        20 => ConsoleKey.F9,
        21 => ConsoleKey.F10,
        23 => ConsoleKey.F11,
        24 => ConsoleKey.F12,
        _ => default,
    };

    private static List<int> SplitParameters(string body)
    {
        var parameters = new List<int>();
        foreach (var part in body.Split(';'))
        {
            parameters.Add(int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : 0);
        }

        return parameters;
    }

    private static ConsoleModifiers ModifiersOfParameter(int parameter)
    {
        var bits = Math.Max(0, parameter - 1);
        var modifiers = default(ConsoleModifiers);

        if ((bits & 1) != 0)
        {
            modifiers |= ConsoleModifiers.Shift;
        }

        if ((bits & 2) != 0)
        {
            modifiers |= ConsoleModifiers.Alt;
        }

        if ((bits & 4) != 0)
        {
            modifiers |= ConsoleModifiers.Control;
        }

        return modifiers;
    }

    private static ConsoleModifiers ModifiersOf(int flags)
    {
        var modifiers = default(ConsoleModifiers);

        if ((flags & ShiftFlag) != 0)
        {
            modifiers |= ConsoleModifiers.Shift;
        }

        if ((flags & AltFlag) != 0)
        {
            modifiers |= ConsoleModifiers.Alt;
        }

        if ((flags & ControlFlag) != 0)
        {
            modifiers |= ConsoleModifiers.Control;
        }

        return modifiers;
    }
}
