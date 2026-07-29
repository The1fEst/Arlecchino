using System;
using System.Collections.Generic;
using System.Globalization;

namespace Arlecchino.Commander.Frames;

public static class KeyScript
{
    public static IReadOnlyList<ConsoleKeyInfo> Parse(string script)
    {
        var keys = new List<ConsoleKeyInfo>();

        foreach (var piece in script.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            keys.Add(One(piece));
        }

        return keys;
    }

    private static ConsoleKeyInfo One(string piece)
    {
        var modifiers = (ConsoleModifiers)0;
        var name = piece;

        while (Prefix(name) is { } split)
        {
            modifiers |= split.Modifier;
            name = split.Tail;
        }

        return Named(name) is { } key
            ? new((char)0, key, modifiers.HasFlag(ConsoleModifiers.Shift), modifiers.HasFlag(ConsoleModifiers.Alt),
                modifiers.HasFlag(ConsoleModifiers.Control))
            : Typed(name, modifiers);
    }

    private static (ConsoleModifiers Modifier, string Tail)? Prefix(string name)
    {
        if (name.StartsWith("Ctrl+", StringComparison.OrdinalIgnoreCase))
        {
            return (ConsoleModifiers.Control, name[5..]);
        }

        if (name.StartsWith("Alt+", StringComparison.OrdinalIgnoreCase))
        {
            return (ConsoleModifiers.Alt, name[4..]);
        }

        return name.StartsWith("Shift+", StringComparison.OrdinalIgnoreCase)
            ? (ConsoleModifiers.Shift, name[6..])
            : null;
    }

    private static ConsoleKey? Named(string name)
    {
        if (name.Length > 1 &&
            (name[0] == 'f' || name[0] == 'F') &&
            int.TryParse(name[1..], NumberStyles.None, CultureInfo.InvariantCulture, out var number) &&
            number is >= 1 and <= 12)
        {
            return ConsoleKey.F1 + (number - 1);
        }

        return name.ToLowerInvariant() switch
        {
            "enter" => ConsoleKey.Enter,
            "esc" => ConsoleKey.Escape,
            "tab" => ConsoleKey.Tab,
            "space" => ConsoleKey.Spacebar,
            "up" => ConsoleKey.UpArrow,
            "down" => ConsoleKey.DownArrow,
            "left" => ConsoleKey.LeftArrow,
            "right" => ConsoleKey.RightArrow,
            "home" => ConsoleKey.Home,
            "end" => ConsoleKey.End,
            "pageup" => ConsoleKey.PageUp,
            "pagedown" => ConsoleKey.PageDown,
            "backspace" => ConsoleKey.Backspace,
            "delete" => ConsoleKey.Delete,
            "insert" => ConsoleKey.Insert,
            _ => null,
        };
    }

    private static ConsoleKeyInfo Typed(string name, ConsoleModifiers modifiers)
    {
        var character = name.Length > 0 ? name[0] : ' ';
        var key = char.IsAsciiLetter(character)
            ? Enum.Parse<ConsoleKey>(char.ToUpperInvariant(character).ToString())
            : char.IsAsciiDigit(character)
                ? ConsoleKey.D0 + (character - '0')
                : default;

        return new(character, key, modifiers.HasFlag(ConsoleModifiers.Shift), modifiers.HasFlag(ConsoleModifiers.Alt),
            modifiers.HasFlag(ConsoleModifiers.Control));
    }
}
