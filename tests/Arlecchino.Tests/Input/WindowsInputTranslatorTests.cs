using System;
using Arlecchino.Input;
using Xunit;

namespace Arlecchino.Tests.Input;

public sealed class WindowsInputTranslatorTests
{
    private const uint Moved = 0x0001;
    private const uint Wheeled = 0x0004;

    private const uint Left = 0x0001;
    private const uint Right = 0x0002;
    private const uint Middle = 0x0004;

    private const uint AltHeld = 0x0002;
    private const uint ControlHeld = 0x0008;
    private const uint ShiftHeld = 0x0010;

    private const uint WheelUp = 0x00780000;
    private const uint WheelDown = 0xFF880000;

    private readonly WindowsInputTranslator _translator = new();

    [Fact]
    public void KeyCarriesCharacterKeyAndModifiers()
    {
        var key = WindowsInputTranslator.ToKeyPress((ushort)ConsoleKey.A, 'a', ControlHeld | ShiftHeld);

        Assert.Equal('a', key.Character);
        Assert.Equal(ConsoleKey.A, key.Key);
        Assert.Equal(KeyModifiers.Control | KeyModifiers.Shift, key.Modifiers);
    }

    [Fact]
    public void ShiftThatOnlyTypedTheCharacterIsNotHeld()
    {
        var colon = WindowsInputTranslator.ToKeyPress((ushort)ConsoleKey.Oem1, ':', ShiftHeld);

        Assert.Equal(':', colon.Character);
        Assert.Equal(ConsoleKey.Oem1, colon.Key);
        Assert.Equal(KeyModifiers.None, colon.Modifiers);

        var capital = WindowsInputTranslator.ToKeyPress((ushort)ConsoleKey.J, 'J', ShiftHeld);

        Assert.Equal(KeyModifiers.None, capital.Modifiers);
    }

    [Fact]
    public void ShiftOnAKeyThatTypesNothingIsHeld()
    {
        var function = WindowsInputTranslator.ToKeyPress((ushort)ConsoleKey.F6, '\0', ShiftHeld);

        Assert.Equal(KeyModifiers.Shift, function.Modifiers);

        var tab = WindowsInputTranslator.ToKeyPress((ushort)ConsoleKey.Tab, '\t', ShiftHeld);

        Assert.Equal(KeyModifiers.Shift, tab.Modifiers);
    }

    [Fact]
    public void KeyWithoutModifiersCarriesNone()
    {
        var key = WindowsInputTranslator.ToKeyPress((ushort)ConsoleKey.Enter, '\r', 0);

        Assert.Equal(ConsoleKey.Enter, key.Key);
        Assert.Equal(default, key.Modifiers);
    }

    [Fact]
    public void PressBecomesPressedAtTheCellItHappenedIn()
    {
        Assert.True(_translator.TryTranslateMouse(4, 7, Left, 0, 0, out var mouse));

        Assert.Equal(MouseAction.Pressed, mouse.Action);
        Assert.Equal(MouseButton.Left, mouse.Button);
        Assert.Equal(4, mouse.Row);
        Assert.Equal(7, mouse.Column);
    }

    [Fact]
    public void ReleaseIsTheButtonThatWentAway()
    {
        _translator.TryTranslateMouse(0, 0, Right, 0, 0, out _);

        Assert.True(_translator.TryTranslateMouse(0, 0, 0, 0, 0, out var mouse));

        Assert.Equal(MouseAction.Released, mouse.Action);
        Assert.Equal(MouseButton.Right, mouse.Button);
    }

    [Fact]
    public void HoldingTheSameButtonReportsNothingTwice()
    {
        _translator.TryTranslateMouse(0, 0, Middle, 0, 0, out _);

        Assert.False(_translator.TryTranslateMouse(0, 0, Middle, 0, 0, out _));
    }

    [Fact]
    public void MovementWithAButtonHeldIsADrag()
    {
        Assert.True(_translator.TryTranslateMouse(2, 3, Left, 0, Moved, out var mouse));

        Assert.Equal(MouseAction.Moved, mouse.Action);
        Assert.Equal(MouseButton.Left, mouse.Button);
    }

    [Fact]
    public void MovementWithNoButtonIsNotAnEvent()
    {
        Assert.False(_translator.TryTranslateMouse(2, 3, 0, 0, Moved, out _));
    }

    [Fact]
    public void ADragDoesNotTurnIntoAPressWhenTheButtonStaysDown()
    {
        _translator.TryTranslateMouse(0, 0, Left, 0, Moved, out _);

        Assert.False(_translator.TryTranslateMouse(1, 1, Left, 0, 0, out _));
    }

    [Fact]
    public void ReleaseAfterADragIsStillAReleaseOfThatButton()
    {
        _translator.TryTranslateMouse(0, 0, Left, 0, Moved, out _);

        Assert.True(_translator.TryTranslateMouse(1, 1, 0, 0, 0, out var mouse));

        Assert.Equal(MouseAction.Released, mouse.Action);
        Assert.Equal(MouseButton.Left, mouse.Button);
    }

    [Fact]
    public void WheelAwayFromTheUserScrollsUp()
    {
        Assert.True(_translator.TryTranslateMouse(0, 0, WheelUp, 0, Wheeled, out var mouse));

        Assert.Equal(MouseAction.ScrolledUp, mouse.Action);
        Assert.Equal(MouseButton.None, mouse.Button);
    }

    [Fact]
    public void WheelTowardsTheUserScrollsDown()
    {
        Assert.True(_translator.TryTranslateMouse(0, 0, WheelDown, 0, Wheeled, out var mouse));

        Assert.Equal(MouseAction.ScrolledDown, mouse.Action);
    }

    [Fact]
    public void WheelDoesNotCountAsAHeldButton()
    {
        _translator.TryTranslateMouse(0, 0, WheelUp, 0, Wheeled, out _);

        Assert.True(_translator.TryTranslateMouse(0, 0, Left, 0, 0, out var mouse));

        Assert.Equal(MouseAction.Pressed, mouse.Action);
    }

    [Fact]
    public void MouseEventsCarryTheModifiersThatWereHeld()
    {
        Assert.True(_translator.TryTranslateMouse(0, 0, Left, AltHeld | ShiftHeld, 0, out var mouse));

        Assert.Equal(KeyModifiers.Alt | KeyModifiers.Shift, mouse.Modifiers);
    }
}
