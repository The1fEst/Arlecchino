using System;
using System.Collections.Generic;
using System.Globalization;

namespace Arlecchino.Input;

/// <summary>
/// Reads the escape sequences a terminal sends for mouse reports and for keys that have no
/// character — arrows, function keys, and their modified forms.
/// </summary>
internal sealed class EscapeSequenceParser
{
    private const int WheelFlag = 64;
    private const int MotionFlag = 32;
    private const int ShiftFlag = 4;
    private const int AltFlag = 8;
    private const int ControlFlag = 16;
    private const int PrivateUseFirst = 0xE000;
    private const int PrivateUseLast = 0xF8FF;

    /// <summary>Reads an SGR mouse report, the <c>&lt;flags;column;rowM</c> form.</summary>
    /// <param name="sequence">Body of the sequence, without the leading escape and bracket.</param>
    /// <param name="mouse">The event, with coordinates converted to zero-based cells.</param>
    /// <returns><c>true</c> when the sequence was a mouse report.</returns>
    public static bool TryParseMouse(string sequence, out MouseEvent mouse)
    {
        mouse = default;

        if (!sequence.StartsWith('<') || sequence.Length < 6)
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

        mouse = new(action,
            action == MouseAction.Moved && buttonBits == 3 ? MouseButton.None : button,
            row - 1,
            column - 1,
            modifiers);
        return true;
    }

    /// <summary>
    /// Reads a cursor or function key sequence, including the <c>1;5C</c> form that carries
    /// modifiers and the <c>106;9u</c> form a terminal falls back to for a key the older shapes cannot
    /// spell — which is every key held with Command.
    /// </summary>
    /// <param name="sequence">Body of the sequence, without the leading escape and bracket.</param>
    /// <param name="key">
    /// The key it stands for, or nothing for a sequence that was a key press but not one that can be
    /// named here. Nothing is still an answer: the bytes were understood, so they must not be replayed
    /// into the application as text.
    /// </param>
    /// <returns><c>true</c> when the sequence was a key press.</returns>
    public static bool TryParseKey(string sequence, out KeyPress key)
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

        if (IsRelease(body))
        {
            return true;
        }

        if (final == 'u')
        {
            return TryParseUnicodeKey(parameters, modifiers, out key);
        }

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
            modifiers |= KeyModifiers.Shift;
        }

        key = new(consoleKey, modifiers);
        return true;
    }

    /// <summary>
    /// Reads the <c>CSI code ; modifiers u</c> form, where the key is named by the character it would
    /// have typed. A terminal reaches for it when the older shapes have nowhere to put what happened —
    /// there is no legacy spelling for a letter held with Command, so <c>Cmd+J</c> arrives here or not
    /// at all.
    /// </summary>
    /// <param name="parameters">The numbers in front of the final byte.</param>
    /// <param name="modifiers">What was held, already read off the second number.</param>
    /// <param name="key">The key, or nothing for a code with no name here.</param>
    /// <returns><c>true</c>, since the shape itself says a key was pressed.</returns>
    private static bool TryParseUnicodeKey(List<int> parameters, KeyModifiers modifiers, out KeyPress key)
    {
        key = default;

        if (parameters.Count == 0 || parameters[0] <= 0)
        {
            return true;
        }

        var code = parameters[0];
        var named = KeyOfCode(code);

        if (named == default && !IsTypeable(code))
        {
            return true;
        }

        var character = IsTypeable(code) && !char.IsControl((char)code) ? (char)code : '\0';
        key = new(named, modifiers, character);
        return true;
    }

    /// <summary>
    /// Whether the code stands for a character at all. A terminal names the keys with nothing to type —
    /// the keypad, the media keys, the modifiers themselves — with codes out of the private use area,
    /// which is a range no keyboard types and no field should be handed.
    /// </summary>
    /// <param name="code">The number in front of the final byte.</param>
    /// <returns><c>true</c> when the code is a character.</returns>
    private static bool IsTypeable(int code) =>
        code <= char.MaxValue && code is < PrivateUseFirst or > PrivateUseLast;

    private static ConsoleKey KeyOfCode(int code) => code switch
    {
        >= 'a' and <= 'z' => ConsoleKey.A + (code - 'a'),
        >= 'A' and <= 'Z' => ConsoleKey.A + (code - 'A'),
        >= '0' and <= '9' => ConsoleKey.D0 + (code - '0'),
        ' ' => ConsoleKey.Spacebar,
        '\t' => ConsoleKey.Tab,
        '\r' or '\n' => ConsoleKey.Enter,
        '\e' => ConsoleKey.Escape,
        8 or 127 => ConsoleKey.Backspace,
        _ => default,
    };

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

    /// <summary>
    /// The numbers in front of the final byte. A number may carry extra parts of its own after a colon
    /// — the key a shifted press would have typed, or whether the key went down or came back up — and
    /// only the first part is the number itself.
    /// </summary>
    /// <param name="body">The sequence without its final byte.</param>
    /// <returns>One number per parameter, zero for anything unreadable.</returns>
    private static List<int> SplitParameters(string body)
    {
        var parameters = new List<int>();
        foreach (var part in body.Split(';'))
        {
            parameters.Add(int.TryParse(Head(part), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : 0);
        }

        return parameters;
    }

    /// <summary>
    /// Whether the sequence says a key came back up rather than went down. Only terminals asked for
    /// release events send them, and nothing here asks — but a key that arrived twice would act twice,
    /// so the answer is worth reading rather than assuming.
    /// </summary>
    /// <param name="body">The sequence without its final byte.</param>
    /// <returns><c>true</c> when this is a release.</returns>
    private static bool IsRelease(string body)
    {
        var parts = body.Split(';');

        if (parts.Length < 2)
        {
            return false;
        }

        var colon = parts[1].IndexOf(':', StringComparison.Ordinal);

        return colon >= 0 && parts[1][(colon + 1)..] == "3";
    }

    private static string Head(string part)
    {
        var colon = part.IndexOf(':', StringComparison.Ordinal);

        return colon < 0 ? part : part[..colon];
    }

    private static KeyModifiers ModifiersOfParameter(int parameter)
    {
        var bits = Math.Max(0, parameter - 1);
        var modifiers = default(KeyModifiers);

        if ((bits & 1) != 0)
        {
            modifiers |= KeyModifiers.Shift;
        }

        if ((bits & 2) != 0)
        {
            modifiers |= KeyModifiers.Alt;
        }

        if ((bits & 4) != 0)
        {
            modifiers |= KeyModifiers.Control;
        }

        if ((bits & 8) != 0)
        {
            modifiers |= KeyModifiers.Super;
        }

        return modifiers;
    }

    private static KeyModifiers ModifiersOf(int flags)
    {
        var modifiers = default(KeyModifiers);

        if ((flags & ShiftFlag) != 0)
        {
            modifiers |= KeyModifiers.Shift;
        }

        if ((flags & AltFlag) != 0)
        {
            modifiers |= KeyModifiers.Alt;
        }

        if ((flags & ControlFlag) != 0)
        {
            modifiers |= KeyModifiers.Control;
        }

        return modifiers;
    }
}
