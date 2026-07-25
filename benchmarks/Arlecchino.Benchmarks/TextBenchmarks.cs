using BenchmarkDotNet.Attributes;
using Arlecchino.Rendering;

namespace Arlecchino.Benchmarks;

[MemoryDiagnoser]
public class TextBenchmarks
{
    private const string Ascii = "a plain line of latin text, the common case by far";
    private const string Wide = "density 全角文字, emoji 👩‍👩‍👧 and a café in one line";

    [Benchmark(Description = "Measure latin text")]
    public int MeasureAscii() => TextWidth.Of(Ascii);

    [Benchmark(Description = "Measure wide and combining text")]
    public int MeasureWide() => TextWidth.Of(Wide);

    [Benchmark(Description = "Truncate latin text")]
    public string TruncateAscii() => TextWidth.Truncate(Ascii, 20);

    [Benchmark(Description = "Truncate wide text")]
    public string TruncateWide() => TextWidth.Truncate(Wide, 20);
}
