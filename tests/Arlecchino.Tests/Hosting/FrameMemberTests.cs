using Arlecchino.Atoms;
using Arlecchino.Atoms.Collections;
using Arlecchino.Atoms.History;
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
        Assert.Equal("Writing Atom`1", FrameMembers.Writing(new Atom<int>(0)));
        Assert.Equal("Changing AtomsList`1", FrameMembers.Changing(new AtomsList<int>()));
    }

    [Fact]
    public void EveryCollectionFamilyReachesTheSameNaming()
    {
        Assert.Equal("Changing AtomsQueue`1", FrameMembers.Changing(new AtomsQueue<int>()));
        Assert.Equal("Changing AtomsStack`1", FrameMembers.Changing(new AtomsStack<int>()));
        Assert.Equal("Changing AtomsSet`1", FrameMembers.Changing(new AtomsSet<int>()));
        Assert.Equal("Changing AtomsMap`2", FrameMembers.Changing(new AtomsMap<string, int>()));
    }

    [Fact]
    public void AnAtomIsNamedByTheTypeItTurnsOutToBeRatherThanTheBaseItIsCheckedIn()
    {
        Atom<int> atom = new HistoryAtom<int>(0, "FrameMember.Count");

        Assert.Equal("Writing HistoryAtom`1", FrameMembers.Writing(atom));
    }
}
