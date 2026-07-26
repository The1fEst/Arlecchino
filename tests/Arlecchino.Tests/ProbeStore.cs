using Arlecchino.State;
using Arlecchino.Atoms;

namespace Arlecchino.Tests;

public sealed class ProbeStore : IArlecchinoStore
{
    public Atom<string> Name { get; } = new LocalAtom<string>("probe");
}

public sealed class ScopedProbeStore : IArlecchinoScopedStore
{
    public ScopedProbeStore(ArlecchinoState state)
    {
        State = state;
    }

    public ArlecchinoState State { get; }
}
