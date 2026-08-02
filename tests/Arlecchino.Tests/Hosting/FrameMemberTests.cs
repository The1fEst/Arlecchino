using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;
using Arlecchino.State;
using Xunit;

namespace Arlecchino.Tests.Hosting;

public sealed class FrameMemberTests
{
    [Fact]
    public void AMemberIsNamedByItsTypeAndItsOwnName()
    {
        Assert.Equal(
            "ArlecchinoState.CloseAllModals",
            FrameMembers.Of<ArlecchinoState>(nameof(ArlecchinoState.CloseAllModals)));
    }

    [Fact]
    public void AnAtomIsNamedByWhatIsBeingDoneToIt()
    {
        Assert.Equal("Writing LocalAtom`1", FrameMembers.Writing(new LocalAtom<int>(0)));
        Assert.Equal("Changing LocalAtomsList`1", FrameMembers.Changing(new LocalAtomsList<int>()));
    }

    [Fact]
    public void EveryCollectionFamilyReachesTheSameNaming()
    {
        Assert.Equal("Changing LocalAtomsQueue`1", FrameMembers.Changing(new LocalAtomsQueue<int>()));
        Assert.Equal("Changing LocalAtomsStack`1", FrameMembers.Changing(new LocalAtomsStack<int>()));
        Assert.Equal("Changing LocalAtomsSet`1", FrameMembers.Changing(new LocalAtomsSet<int>()));
        Assert.Equal("Changing LocalAtomsMap`2", FrameMembers.Changing(new LocalAtomsMap<string, int>()));
    }

    [Fact]
    public void AnAtomIsNamedByTheTypeItTurnsOutToBeRatherThanTheBaseItIsCheckedIn()
    {
        Atom<int> tracked = new TrackedAtom<int>(0);

        Assert.Equal("Writing TrackedAtom`1", FrameMembers.Writing(tracked));
    }
}
