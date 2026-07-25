using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.State;

namespace Arlecchino.Processes;

public sealed record ProcessRow(int Id, string Name, long Memory, int Threads, TimeSpan Cpu, DateTime? Started);

public sealed class ProcessTable
{
    private readonly AsyncState<IReadOnlyList<ProcessRow>> _rows;
    private readonly LocalState<string> _filter = new("");

    public ProcessTable(UiDispatcher dispatcher)
    {
        _rows = new(dispatcher, []);
    }

    public AsyncState<IReadOnlyList<ProcessRow>> Rows => _rows;

    public State<string> Filter => _filter;

    public State<ProcessRow?> Selected { get; } = new LocalState<ProcessRow?>(null);

    public DateTimeOffset LoadedAt { get; private set; }

    public void Refresh() => _rows.Load(token => Task.Run(() => Read(token), token));

    public IReadOnlyList<ProcessRow> Visible()
    {
        var all = _rows.Value ?? [];
        if (_filter.Value.Length == 0)
        {
            return all;
        }

        var matching = new List<ProcessRow>();
        foreach (var row in all)
        {
            if (row.Name.Contains(_filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                matching.Add(row);
            }
        }

        return matching;
    }

    private IReadOnlyList<ProcessRow> Read(CancellationToken token)
    {
        var rows = new List<ProcessRow>();

        foreach (var process in Process.GetProcesses())
        {
            token.ThrowIfCancellationRequested();

            using (process)
            {
                rows.Add(Describe(process));
            }
        }

        rows.Sort(static (first, second) => second.Memory.CompareTo(first.Memory));
        LoadedAt = DateTimeOffset.Now;

        return rows;
    }

    private static ProcessRow Describe(Process process)
    {
        var id = process.Id;
        var name = process.ProcessName;

        try
        {
            return new(id, name, process.WorkingSet64, process.Threads.Count, process.TotalProcessorTime,
                process.StartTime);
        }
        catch (Exception exception) when (exception is InvalidOperationException or SystemException)
        {
            return new(id, name, 0, 0, TimeSpan.Zero, null);
        }
    }
}
