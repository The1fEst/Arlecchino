using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;

namespace Arlecchino.Processes;

public sealed record ProcessRow(int Id, string Name, long Memory, int Threads, TimeSpan Cpu, DateTime? StartTime);

public sealed class ProcessTable : IArlecchinoStore
{
    private readonly AsyncAtom<IReadOnlyList<ProcessRow>> _rows = new([]);
    private readonly LocalAtom<string> _filter = new("");

    public AsyncAtom<IReadOnlyList<ProcessRow>> Rows => _rows;

    public Atom<string> Filter => _filter;

    public Atom<ProcessRow?> SelectedRow { get; } = new LocalAtom<ProcessRow?>(null);

    public DateTimeOffset LoadedAt { get; private set; }

    public void Refresh() => _rows.Load(token => Task.Run<IReadOnlyList<ProcessRow>>(() => Read(token), token));

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

    private List<ProcessRow> Read(CancellationToken token)
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
            return new(id,
                name,
                process.WorkingSet64,
                process.Threads.Count,
                process.TotalProcessorTime,
                process.StartTime);
        }
        catch (Exception exception) when (exception is InvalidOperationException or SystemException)
        {
            return new(id, name, 0, 0, TimeSpan.Zero, null);
        }
    }
}
