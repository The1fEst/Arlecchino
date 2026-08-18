using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using BenchmarkDotNet.Attributes;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;

namespace Arlecchino.Benchmarks;

[MemoryDiagnoser]
public class StateBenchmarks
{
    private const int Subscribers = 20;

    private readonly List<IDisposable> _subscriptions = [];
    private readonly LocalAtom<int> _plain = new(0);
    private readonly TrackedAtom<int> _trackedAtom = new(0);
    private readonly LocalAtom<int> _watchedAtom = new(0);
    private readonly LocalAtom<string> _first = new("first");
    private readonly LocalAtom<string> _second = new("second");

    private AtomHistory _history = null!;
    private Computed<int> _computedAtom = null!;
    private int _next;
    private int _notifications;

    [GlobalSetup]
    public void Setup()
    {
        _history = new() { Capacity = 1000 };
        _computedAtom = new(() => _first.Value.Length + _second.Value.Length);
        _ = _computedAtom.Value;

        for (var subscriber = 0; subscriber < Subscribers; subscriber++)
        {
            _subscriptions.Add(_watchedAtom.Subscribe(() => _notifications++));
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }

        _history.Dispose();
    }

    [Benchmark(Description = "Write an atom nothing listens to")]
    public int WriteUnwatched()
    {
        _plain.Value = _next++;
        return _plain.Value;
    }

    [Benchmark(Description = "Write the same value again")]
    public int WriteUnchanged()
    {
        var current = _plain.Value;
        _plain.Value = current;
        return _plain.Value;
    }

    [Benchmark(Description = "Write an atom 20 things listen to")]
    public int WriteWatched()
    {
        _watchedAtom.Value = _next++;
        return _notifications;
    }

    [Benchmark(Description = "Write an atom that records history")]
    public int WriteTracked()
    {
        _trackedAtom.Value = _next++;
        return _history.Depth;
    }

    [Benchmark(Description = "Read a computed value that did not change")]
    public int ReadComputedCached() => _computedAtom.Value;

    [Benchmark(Description = "Read a computed value after a dependency changed")]
    public int ReadComputedInvalidated()
    {
        _first.Value = _next++ % 2 == 0 ? "first" : "another first";
        return _computedAtom.Value;
    }

    [Benchmark(Description = "Undo and redo one edit")]
    public bool UndoRedo()
    {
        _trackedAtom.Value = _next++;
        _history.Undo();
        return _history.Redo();
    }
}
