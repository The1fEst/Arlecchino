using System;

namespace Arlecchino.Packages.Model;

public static class VersionOrder
{
    public static int Compare(string first, string second)
    {
        var (firstNumbers, firstTag) = Split(first);
        var (secondNumbers, secondTag) = Split(second);

        var parts = Math.Max(firstNumbers.Length, secondNumbers.Length);
        for (var i = 0; i < parts; i++)
        {
            var left = i < firstNumbers.Length ? firstNumbers[i] : 0;
            var right = i < secondNumbers.Length ? secondNumbers[i] : 0;

            if (left != right)
            {
                return left.CompareTo(right);
            }
        }

        if (firstTag.Length == 0 && secondTag.Length == 0)
        {
            return 0;
        }

        if (firstTag.Length == 0)
        {
            return 1;
        }

        return secondTag.Length == 0 ? -1 : string.CompareOrdinal(firstTag, secondTag);
    }

    private static (int[] Numbers, string Tag) Split(string version)
    {
        var body = version;
        var tag = "";

        var dash = body.IndexOf('-', StringComparison.Ordinal);
        if (dash >= 0)
        {
            tag = body[(dash + 1)..];
            body = body[..dash];
        }

        var pieces = body.Split('.');
        var numbers = new int[pieces.Length];

        for (var i = 0; i < pieces.Length; i++)
        {
            numbers[i] = int.TryParse(pieces[i], out var number) ? number : 0;
        }

        return (numbers, tag);
    }
}
