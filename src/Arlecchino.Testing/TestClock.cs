using System;

namespace Arlecchino.Testing;

/// <summary>
/// A clock a test moves by hand. Scheduled work runs when the clock passes its due time, so a second is
/// moved rather than waited for.
/// </summary>
public sealed class TestClock : TimeProvider
{
    private DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>The time the clock reads now.</summary>
    /// <returns>The current time.</returns>
    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>Moves the clock forward.</summary>
    /// <param name="amount">How far ahead; a negative amount leaves the clock where it is.</param>
    public void Advance(TimeSpan amount)
    {
        if (amount > TimeSpan.Zero)
        {
            _now += amount;
        }
    }
}
