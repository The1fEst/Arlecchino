using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Arlecchino.Atoms.History.Hosting;

/// <summary>
/// Throws away the scratch file a killed run left behind and empties the stack, so that what the stores
/// wrote while loading is not the first thing an undo takes back. It runs before the first frame.
/// </summary>
internal sealed class HistoryStartup : IHostedService
{
    private readonly AtomHistory _history;
    private readonly HistoryFile _file;

    public HistoryStartup(AtomHistory history, HistoryFile file)
    {
        _history = history;
        _file = file;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _file.Prepare();
        _history.Clear();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
