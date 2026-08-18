using System;
using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Support;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests.Navigation;

/// <summary>
///     A screen with something being typed into it. Going back and forward is bound to the keys a caret
///     moves by word on, so the screen is asked before the history is walked.
/// </summary>
public sealed class TypingScreenTests
{
    [Fact]
    public void TheWordKeysReachAScreenBeingTypedInto()
    {
        using var app = Typing(true);

        app.Navigator.Apply(ViewKind.Typed);
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Alt);

        Assert.Equal(ViewKind.Typed, app.Navigator.CurrentRoute);
        Assert.Contains(ConsoleKey.LeftArrow, TypedView.Keys);
    }

    [Fact]
    public void TheHistoryIsWalkedWhenNothingIsBeingTypedInto()
    {
        using var app = Typing(false);

        app.Navigator.Apply(ViewKind.Typed);
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Alt);

        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);
        Assert.DoesNotContain(ConsoleKey.LeftArrow, TypedView.Keys);
    }

    [Fact]
    public void GoingForwardWaitsForTheTypingToEndToo()
    {
        using var app = Typing(true);

        app.Navigator.Apply(ViewKind.Typed);
        app.Press(ConsoleKey.RightArrow, KeyModifiers.Alt);

        Assert.Equal(ViewKind.Typed, app.Navigator.CurrentRoute);
        Assert.Contains(ConsoleKey.RightArrow, TypedView.Keys);
    }

    private static TestApplication Typing(bool typing)
    {
        TypedView.Keys.Clear();
        TypedView.Typing = typing;

        return new();
    }
}

/// <summary>A view that answers whether it is being typed into and remembers the keys it was given.</summary>
public sealed class TypedView : IArlecchinoView
{
    private readonly Surface _surface;

    public TypedView(Surface surface)
    {
        _surface = surface;
    }

    public static List<ConsoleKey> Keys { get; } = [];

    public static bool Typing { get; set; }

    public bool IsTyping => Typing;

    public void Draw()
    {
        _surface.AppendLine("typed", Theme.Default);
    }

    public ViewRoute Handle(KeyPress key)
    {
        Keys.Add(key.Key);

        return ViewRoute.None;
    }
}
