using Arlecchino.State;

namespace Arlecchino.Tests;

public sealed class ProbeStore : IStore
{
    public State<string> Name { get; } = new LocalState<string>("probe");
}

public sealed class ScopedProbeStore : IScopedStore
{
    public ScopedProbeStore(TuiState state)
    {
        State = state;
    }

    public TuiState State { get; }
}
