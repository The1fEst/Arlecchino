using System;
using System.Collections.Generic;

namespace Arlecchino.Pictures.Formats.Svg;

/// <summary>
/// Where along a dash pattern a line has got to. It is carried from line to line, and every step it
/// hands out has a length, a pattern being free to hold a nought that would be walked forever.
/// </summary>
internal sealed class SvgDashes
{
    /// <summary>The shortest dash there is any drawing in, below which one counts as none at all.</summary>
    private const double Least = 0.01d;

    private readonly IReadOnlyList<double> _pattern;

    private int _piece;
    private double _left;

    /// <summary>Starts at the beginning of a pattern.</summary>
    /// <param name="pattern">The dashes and the gaps between them, one after another.</param>
    public SvgDashes(IReadOnlyList<double> pattern)
    {
        _pattern = pattern;
        Solid = Whole(pattern) <= Least;
        _left = double.MaxValue;

        if (Solid)
        {
            return;
        }

        _left = pattern[0];

        if (_left <= Least)
        {
            Advance();
        }
    }

    /// <summary>Whether the pattern says nothing, so the line is drawn from end to end.</summary>
    public bool Solid { get; }

    /// <summary>Whether what is being walked now is drawn rather than left open.</summary>
    public bool Drawn => _piece % 2 == 0;

    /// <summary>How much of it is left to walk.</summary>
    public double Left => _left;

    /// <summary>Walks part of the pattern, stepping to the next piece once this one is walked out.</summary>
    /// <param name="distance">How far was walked, which is never more than <see cref="Left"/>.</param>
    public void Take(double distance)
    {
        _left -= distance;

        if (_left <= Least)
        {
            Advance();
        }
    }

    /// <summary>
    /// Steps to the next piece there is anything to walk. A pattern of nothing but noughts is ruled out
    /// by <see cref="Solid"/>, so one of the pieces is always long enough to be stepped into.
    /// </summary>
    private void Advance()
    {
        var attempts = 0;

        while (attempts < _pattern.Count)
        {
            _piece++;
            attempts++;

            var length = _pattern[_piece % _pattern.Count];

            if (length <= Least)
            {
                continue;
            }

            _left = length;

            return;
        }

        _left = double.MaxValue;
    }

    /// <summary>How long the whole pattern is.</summary>
    /// <param name="pattern">The pattern.</param>
    /// <returns>Its length.</returns>
    private static double Whole(IReadOnlyList<double> pattern)
    {
        var whole = 0d;

        foreach (var dash in pattern)
        {
            whole += Math.Max(0d, dash);
        }

        return whole;
    }
}
