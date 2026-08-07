using System;

namespace Arlecchino.Sample.Probes;

internal static class KeyReport
{
    public static void Run()
    {
        Console.WriteLine("Press keys to see what the terminal reports. Esc twice to leave.");

        var lastWasEscape = false;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            var character = key.KeyChar == '\0' ? "none" : $"'{key.KeyChar}' U+{(int)key.KeyChar:X4}";

            Console.WriteLine($"Key={key.Key} ({(int)key.Key})   Char={character}   Modifiers={key.Modifiers}");

            if (key.Key == ConsoleKey.Escape || key.KeyChar == '\e')
            {
                if (lastWasEscape)
                {
                    return;
                }

                lastWasEscape = true;
                continue;
            }

            lastWasEscape = false;
        }
    }
}
