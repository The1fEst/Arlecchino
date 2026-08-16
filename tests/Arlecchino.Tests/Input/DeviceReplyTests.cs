using Arlecchino.Modals.Asking;
using Arlecchino.Tests.Support;
using Xunit;

namespace Arlecchino.Tests.Input;

/// <summary>
/// What a terminal says about itself, arriving after the probe gave up on it. A reply is not typing, so
/// none of it may reach whatever is being typed into.
/// </summary>
public sealed class DeviceReplyTests
{
    [Theory]
    [InlineData("\e[?65;4;6;18;22;52c")]
    [InlineData("\e[?61;4;6;7;14;21;22;23;24;28;32;42;52c")]
    [InlineData("\e[?62;22c")]
    [InlineData("\e[>0;10;1c")]
    [InlineData("\e[4;600;1200t")]
    [InlineData("\e[6;20;10t")]
    [InlineData("\e]11;rgb:1414/1313/1717\a")]
    [InlineData("\e_Gi=31;OK\e\\")]
    public void NothingOfItIsTypedIntoTheLine(string reply)
    {
        using var app = new TestApplication();

        app.State.RequestText("Rename", "", null, static _ => { });
        app.ReadFromTerminal(reply);

        Assert.Equal("", ((TextModal)app.State.Modal!).Text);
    }

    /// <summary>
    /// The reply is swallowed rather than merely unreadable: the modal it lands on is still open, since an
    /// escape read on its own would have closed it.
    /// </summary>
    [Fact]
    public void TheEscapeInFrontOfItDoesNotCancel()
    {
        using var app = new TestApplication();

        app.State.RequestText("Rename", "", null, static _ => { });
        app.ReadFromTerminal("\e[?65;4;6;18;22;52c");

        Assert.NotNull(app.State.Modal);
    }

    [Fact]
    public void AKeyAfterOneIsStillRead()
    {
        using var app = new TestApplication();

        app.State.RequestText("Rename", "", null, static _ => { });
        app.ReadFromTerminal("\e[?65;4;6;18;22;52c");
        app.ReadFromTerminal("hi");

        Assert.Equal("hi", ((TextModal)app.State.Modal!).Text);
    }
}
