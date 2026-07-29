using System;
using System.Globalization;

namespace Arlecchino.Commander.Model;

public static class Sizes
{
    private const long Kilobyte = 1024;

    private static readonly string[] Units = ["B", "K", "M", "G", "T"];

    public static string Short(long bytes)
    {
        double size = bytes;
        var unit = 0;

        while (size >= Kilobyte && unit < Units.Length - 1)
        {
            size /= Kilobyte;
            unit++;
        }

        return unit == 0
            ? bytes.ToString(CultureInfo.InvariantCulture)
            : size.ToString(size >= 10 ? "0" : "0.0", CultureInfo.InvariantCulture) + Units[unit];
    }

    public static string Grouped(long bytes) => bytes.ToString("N0", CultureInfo.InvariantCulture);

    public static string Stamp(DateTime moment) => moment == default
        ? ""
        : moment.ToString("dd.MM.yy HH:mm", CultureInfo.InvariantCulture);
}
