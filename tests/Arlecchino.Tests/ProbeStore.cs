using Arlecchino.State;

namespace Arlecchino.Tests;

public sealed class ProbeStore : IArlecchinoStore
{
    public Atom<string> Name { get; } = new LocalAtom<string>("probe");
}

public sealed class ScopedProbeStore : IArlecchinoScopedStore
{
    public ScopedProbeStore(TuiState state)
    {
        State = state;
    }

    public TuiState State { get; }
}
