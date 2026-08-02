using Arlecchino.State;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;

namespace Arlecchino.Tests.Support;

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
