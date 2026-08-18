using System.Collections.Generic;

namespace Arlecchino.Rendering.Terminals;

/// <summary>
/// Which of the characters that came back from a probe belong to an answer, and which were typed while it
/// waited. Every answer is an escape sequence, so whatever falls outside one was typed.
/// </summary>
internal static class TerminalReply
{
    /// <summary>Finds the characters that no answer accounts for.</summary>
    /// <param name="reply">Everything that was read while the probe waited.</param>
    /// <returns>Where each typed character sits in <paramref name="reply"/>, in the order it was read.</returns>
    public static IReadOnlyList<int> Typing(string reply)
    {
        var places = new List<int>();
        var reading = Reading.Ground;
        var spoken = false;

        for (var at = 0; at < reply.Length; at++)
        {
            var letter = reply[at];

            if (reading == Reading.Ground && letter != '\e' && (spoken || letter != '\\'))
            {
                places.Add(at);

                continue;
            }

            spoken = spoken || letter == '\e';
            reading = Next(reading, letter);
        }

        return places;
    }

    private static Reading Next(Reading reading, char letter) => reading switch
    {
        Reading.Ground => letter == '\e' ? Reading.Escaped : Reading.Ground,
        Reading.Escaped => Opens(letter),
        Reading.Single => Reading.Ground,
        Reading.Control => letter is >= '@' and <= '~' ? Reading.Ground : Reading.Control,
        Reading.String when letter == '\a' => Reading.Ground,
        Reading.String => letter == '\e' ? Reading.Closing : Reading.String,
        Reading.Closing when letter == '\\' => Reading.Ground,
        _ => letter == '\e' ? Reading.Closing : Reading.String,
    };

    /// <summary>
    /// What an escape opened: a control sequence, a string that runs to its terminator, a single character
    /// after an <c>O</c>, or a two-character sequence that is already over.
    /// </summary>
    /// <param name="letter">What followed the escape.</param>
    /// <returns>What is being read now.</returns>
    private static Reading Opens(char letter) => letter switch
    {
        '[' => Reading.Control,
        ']' or '_' or 'P' or '^' or 'X' => Reading.String,
        'O' => Reading.Single,
        _ => Reading.Ground,
    };

    private enum Reading
    {
        Ground,
        Escaped,
        Single,
        Control,
        String,
        Closing,
    }
}
