using System.Threading;

namespace Arlecchino.Widgets;

internal static class DrawFaults
{
    private static int _skippedRows;

    public static void RowVanishedWhileDrawing() => Interlocked.Increment(ref _skippedRows);

    public static int TakeSkippedRows() => Interlocked.Exchange(ref _skippedRows, 0);
}
