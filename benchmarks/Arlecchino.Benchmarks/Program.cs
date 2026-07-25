using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Arlecchino.Benchmarks.RenderBenchmarks).Assembly).Run(args);
