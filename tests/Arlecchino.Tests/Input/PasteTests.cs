using System;
using Arlecchino.Input;
using Xunit;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Choosing;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class PasteTests
{
    [Fact]
    public void PastedTextGoesIntoTheFieldAtTheCaret()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "ac", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow);
        app.ReadFromTerminal("\e[200~b\e[201~");

        Assert.Equal("abc", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void OnlyTheFirstPastedLineReachesASingleLineField()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.ReadFromTerminal("\e[200~one\r\ntwo\e[201~");

        Assert.Equal("one", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void AFieldRefusesPastedCharactersItWouldNotAcceptTyped()
    {
        using var app = new TestApplication();

        app.State.RequestNumber("Count", 0m, 0m, 100m, static _ => { });
        app.Press(ConsoleKey.U, KeyModifiers.Control);
        app.ReadFromTerminal("\e[200~1a2\e[201~");

        Assert.Equal("12", ((NumberModal)app.State.Modal!).Text);
    }

    [Fact]
    public void PastingIntoAChoiceModalExtendsTheFilter()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["alpha", "beta"], static _ => { });
        app.ReadFromTerminal("\e[200~bet\e[201~");

        Assert.Equal("bet", ((ChoiceModal)app.State.Modal!).Text);
    }

    [Fact]
    public void PasteMarkersAreNotTypedAsText()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", null, static _ => { });
        app.ReadFromTerminal("\e[200~hi\e[201~");

        Assert.Equal("hi", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void CopyingAFieldReachesTheTerminalClipboard()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "secret-token", null, static _ => { });
        app.Press(ConsoleKey.Insert, KeyModifiers.Control);

        Assert.Equal("secret-token", app.Terminal.Copied);
    }
}
