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
    public void EveryBuiltInWidgetAnswersPlaceItselfRatherThanInheritingTheDefault()
    {
        var offenders = new List<string>();

        foreach (var type in typeof(IArlecchinoWidget).Assembly.GetTypes()
                     .Where(static type => type.IsClass && !type.IsAbstract &&
                                           typeof(IArlecchinoWidget).IsAssignableFrom(type)))
        {
            var place = type.GetMethod(
                nameof(IArlecchinoWidget.Place),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [typeof(SurfaceRegion)],
                null);

            if (place is null || place.DeclaringType != type)
            {
                offenders.Add(type.Name);
            }
        }

        Assert.Empty(offenders);
    }

    [Fact]
    public void AWidgetThatImplementsOnlyPlaceStillAnswersTheObsoleteDraw()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();

        IArlecchinoWidget widget = new PlaceOnlyWidget();

#pragma warning disable ARL0001
        widget.Draw(surface.Frame);
#pragma warning restore ARL0001

        surface.Build();

        Assert.StartsWith("placed", FrameText.Lines(terminal.Written)[0], StringComparison.Ordinal);
    }

    [Fact]
    public void AWidgetThatImplementsOnlyDrawStillAnswersPlace()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();

        IArlecchinoWidget widget = new DrawOnlyWidget();
        var rest = widget.Place(surface.Frame);

        surface.Build();

        Assert.StartsWith("drawn", FrameText.Lines(terminal.Written)[0], StringComparison.Ordinal);
        Assert.True(rest.IsEmpty);
    }

    [Fact]
    public void AWidgetThatOwnsOneRowHandsBackTheRowsBelowIt()
    {
        var terminal = new FakeTerminal(20, 6);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();

        var bar = new StatusBar { Left = [static () => "ready"] };
        var rest = bar.Place(surface.Frame);

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
        var rest = list.Place(surface.Frame);

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

    private sealed class PlaceOnlyWidget : IArlecchinoWidget
    {
        public SurfaceRegion Place(SurfaceRegion region)
        {
            region.WriteLine(0, "placed", Theme.Default);

            return region.Rows(1, region.Height - 1);
        }
    }

    private sealed class DrawOnlyWidget : IArlecchinoWidget
    {
        public void Draw(SurfaceRegion region) => region.WriteLine(0, "drawn", Theme.Default);
    }

    private sealed class Badge : IArlecchinoInteractiveWidget
    {
        private readonly ArlecchinoKeymap _keymap;
        private SurfaceRegion _drawn;

        public Badge(ArlecchinoKeymap keymap) => _keymap = keymap;

        public required Func<string> Label { get; init; }
        public bool IsFocused { get; set; }

        public void Draw(SurfaceRegion region)
        {
            _drawn = region;
            region.WriteLine(0, Label(), IsFocused ? Theme.Active : Theme.Muted);
        }

        public FocusResult Handle(ConsoleKeyInfo key) =>
            _keymap.Confirm.Matches(key) ? FocusResult.Handled : FocusResult.Ignored;

        public FocusResult HandleMouse(MouseEvent mouse) =>
            mouse.IsLeftClick && _drawn.Contains(mouse.Row, mouse.Column)
                ? FocusResult.Handled
                : FocusResult.Ignored;
    }
}
