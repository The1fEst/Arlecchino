using System;
using System.IO;

namespace Arlecchino.Tests;

public sealed class ConsoleOutputScope : IDisposable
{
    private readonly TextWriter _previous = Console.Out;
    private readonly StringWriter _writer = new();

    public ConsoleOutputScope()
    {
        Console.SetOut(_writer);
    }

    public string Written => _writer.GetStringBuilder().ToString();

    public void Clear() => _writer.GetStringBuilder().Clear();

    public void Dispose()
    {
        Console.SetOut(_previous);
        _writer.Dispose();
    }
}
