using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Arlecchino.Rendering;

namespace Arlecchino.Benchmarks;

[MemoryDiagnoser]
public class TextBenchmarks
{
    private const string Ascii = "a plain line of latin text, the common case by far";
    private const string Wide = "density 全角文字, emoji 👩‍👩‍👧 and a café in one line";

    private const string Paragraph =
        "A block of the kind a text view is asked to show: several sentences of plain prose, long " +
        "enough that the width matters and the words have to be moved onto lines of their own.\n" +
        "A second paragraph, so that the line breaks already in the text are kept as they are.";

    [Benchmark(Description = "Measure latin text")]
    public int MeasureAscii() => TextWidth.Of(Ascii);

    [Benchmark(Description = "Measure wide and combining text")]
    public int MeasureWide() => TextWidth.Of(Wide);

    [Benchmark(Description = "Truncate latin text")]
    public string TruncateAscii() => TextWidth.Truncate(Ascii, 20);

    [Benchmark(Description = "Truncate wide text")]
    public string TruncateWide() => TextWidth.Truncate(Wide, 20);

    [Benchmark(Description = "Wrap a paragraph")]
    public List<string> Wrap() => TextWidth.Wrap(Paragraph, 60);
}
