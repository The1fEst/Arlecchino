using System;
using Arlecchino.Modals;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

/// <summary>
/// A dialog the application draws itself. It goes in the same slot as the framework's own, so what has
/// to hold is that it is drawn where they are drawn and takes the keys they would have taken.
/// </summary>
public sealed class CustomModalTests
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

    private sealed class Painted : CustomModal
    {
        public int Keys { get; private set; }

        public override void Draw(SurfaceRegion screen) =>
            screen.Rows(1, 1).WriteLine(0, "drawn by the application", Theme.Accent);

        public override void Handle(ConsoleKeyInfo key) => Keys++;
    }
}
