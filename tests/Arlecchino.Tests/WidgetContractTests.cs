using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arlecchino.Focus;
using Arlecchino.Forms;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class WidgetContractTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    public static TheoryData<Type> InteractiveWidgets =>
    [
        typeof(ListBox<string>),
        typeof(Table<string>),
        typeof(Tree<string>),
        typeof(Tabs),
        typeof(Form),
    ];

    public static TheoryData<Type> PassiveWidgets =>
    [
        typeof(ProgressBar),
        typeof(StatusBar),
        typeof(Spinner),
    ];

    [Theory]
    [MemberData(nameof(InteractiveWidgets))]
    public void AnInteractiveWidgetDrawsAndTakesInput(Type widget)
    {
        Assert.True(typeof(IArlecchinoInteractiveWidget).IsAssignableFrom(widget));
    }

    [Theory]
    [MemberData(nameof(PassiveWidgets))]
    public void APassiveWidgetOnlyDraws(Type widget)
    {
        Assert.True(typeof(IArlecchinoWidget).IsAssignableFrom(widget));
        Assert.False(typeof(IArlecchinoInteractiveWidget).IsAssignableFrom(widget));
    }

    [Fact]
    public void EveryWidgetDrawsThroughTheContractRatherThanAnOverloadOfItsOwn()
    {
        var offenders = new List<string>();

        foreach (var type in typeof(IArlecchinoWidget).Assembly.GetTypes()
                     .Where(static type => type.IsClass && !type.IsAbstract &&
                                           typeof(IArlecchinoWidget).IsAssignableFrom(type)))
        {
            var extra = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(static method => method.Name == "Draw")
                .Where(static method => method.GetParameters() is not [{ ParameterType: var only }] ||
                                        only != typeof(SurfaceRegion));

            offenders.AddRange(extra.Select(method => $"{type.Name}.{method}"));
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void EveryBuiltInWidgetDrawsItselfRatherThanInheritingItFromABase()
    {
        var offenders = new List<string>();

        foreach (var type in typeof(IArlecchinoWidget).Assembly.GetTypes()
                     .Where(static type => type.IsClass && !type.IsAbstract &&
                                           typeof(IArlecchinoWidget).IsAssignableFrom(type)))
        {
            var draw = type.GetMethod(
                nameof(IArlecchinoWidget.Draw),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [typeof(SurfaceRegion)],
                null);

            if (draw is null || draw.DeclaringType != type)
            {
                offenders.Add(type.Name);
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void AWidgetOfYourOwnDrawsAndSaysWhatIsLeftUnderIt()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();

        IArlecchinoWidget widget = new OneRowWidget();
        var rest = widget.Draw(surface.Frame);

        surface.Build();

        Assert.StartsWith("drawn", FrameText.Lines(terminal.Written)[0], StringComparison.Ordinal);
        Assert.Equal(1, rest.Top);
        Assert.Equal(3, rest.Height);
    }

    [Fact]
    public void AWidgetThatOwnsOneRowHandsBackTheRowsBelowIt()
    {
        var terminal = new FakeTerminal(20, 6);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();

        var bar = new StatusBar { Left = [static () => "ready"] };
        var rest = bar.Draw(surface.Frame);

        Assert.Equal(1, rest.Top);
        Assert.Equal(5, rest.Height);
    }

    [Fact]
    public void AWidgetThatFillsItsRegionHandsBackNothing()
    {
        var terminal = new FakeTerminal(20, 6);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();

        var list = new ListBox<string>(Keymap) { Render = static item => item, Items = ["one", "two"] };
        var rest = list.Draw(surface.Frame);

        Assert.True(rest.IsEmpty);
    }

    [Fact]
    public void AWidgetOfYourOwnJoinsTheFocusRingLikeABuiltInOne()
    {
        using var app = new TestApplication();
        var badge = new Badge(app.Options.Keymap) { Label = static () => "ready" };
        var ring = new FocusRing(app.Options.Keymap);

        ring.Add(badge);

        Assert.True(badge.IsFocused);
        Assert.Same(badge, ring.Current);
        Assert.Equal(FocusResult.Handled, ring.Current!.Handle(new('\r', ConsoleKey.Enter, false, false, false)));
    }

    private sealed class OneRowWidget : IArlecchinoWidget
    {
        public SurfaceRegion Draw(SurfaceRegion region)
        {
            region.WriteLine(0, "drawn", Theme.Default);

            return region.Rows(1, region.Height - 1);
        }
    }

    private sealed class Badge : IArlecchinoInteractiveWidget
    {
        private readonly ArlecchinoKeymap _keymap;
        private SurfaceRegion _drawn;

        public Badge(ArlecchinoKeymap keymap) => _keymap = keymap;

        public required Func<string> Label { get; init; }
        public bool IsFocused { get; set; }

        public SurfaceRegion Draw(SurfaceRegion region)
        {
            _drawn = region;
            region.WriteLine(0, Label(), IsFocused ? Theme.Active : Theme.Muted);

            return region.Rows(1, region.Height - 1);
        }

        public FocusResult Handle(ConsoleKeyInfo key) =>
            _keymap.Confirm.Matches(key) ? FocusResult.Handled : FocusResult.Ignored;

        public FocusResult HandleMouse(MouseEvent mouse) =>
            mouse.IsLeftClick && _drawn.Contains(mouse.Row, mouse.Column)
                ? FocusResult.Handled
                : FocusResult.Ignored;
    }
}
