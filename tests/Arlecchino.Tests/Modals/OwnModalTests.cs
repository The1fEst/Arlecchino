using System;
using Arlecchino.Input;
using Arlecchino.Modals;
using Arlecchino.Rendering.Colors;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

/// <summary>
/// A dialog the application draws itself. It derives from <c>Modal</c> like every dialog the framework
/// brings, so what has to hold is that it is drawn where they are drawn and takes the keys they would
/// have taken — no branch anywhere knows this type exists.
/// </summary>
public sealed class OwnModalTests
{
    [Fact]
    public void ItIsDrawnOverTheView()
    {
        using var app = new TestApplication();

        app.State.Modal = new Painted { Title = "Mine" };

        Assert.Contains("drawn by the application", app.Frame(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Every key reaches it and none reach the view behind it, which is the whole of what makes a
    /// dialog a dialog rather than a picture.
    /// </summary>
    [Fact]
    public void ItTakesEveryKeyWhileItIsOpen()
    {
        using var app = new TestApplication();
        var modal = new Painted { Title = "Mine" };

        app.State.Modal = modal;
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.A);

        Assert.Equal(2, modal.Keys);
    }

    [Fact]
    public void ClosingItUncoversWhatWasUnderneath()
    {
        using var app = new TestApplication();

        app.State.Modal = new Painted { Title = "Mine" };
        app.Frame();

        app.State.CloseModal();

        Assert.Null(app.State.Modal);
        Assert.DoesNotContain("drawn by the application", app.Frame(), StringComparison.Ordinal);
    }

    private sealed class Painted : Modal
    {
        public int Keys { get; private set; }

        public override void Draw(ModalFrame frame) =>
            frame.Screen.Rows(1, 1).WriteLine(0, "drawn by the application", Theme.Accent);

        public override void Handle(ModalFrame frame, KeyPress key) => Keys++;
    }
}
