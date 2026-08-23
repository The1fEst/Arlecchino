using Arlecchino.State;
using Arlecchino.Atoms;

namespace Arlecchino.Tests.Support;

public sealed class ProbeStore : IArlecchinoStore
{
    public Atom<string> Name { get; } = new("probe");
}

public sealed class ScopedProbeStore : IArlecchinoScopedStore
{
    public ScopedProbeStore(ArlecchinoState state)
    {
        State = state;
    }

    public ArlecchinoState State { get; }
}
