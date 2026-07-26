using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arlecchino.Focus;
using Arlecchino.Forms;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class WidgetContractTests
{
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
