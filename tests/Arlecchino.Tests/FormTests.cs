using System;
using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.State;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class FormTests
{
    private static Form CreateForm(
        TestApplication app,
        Atom<string> name,
        Atom<bool> flag,
        Func<ViewRoute>? action = null,
        Func<bool>? enabled = null) => new(app.State, app.Options)
    {
        Fields =
        [
            Field.Text(static () => "Name", name),
            Field.Toggle(static () => "Flag", flag, static value => value ? "Yes" : "No"),
            Field.Action(static () => "Apply", action ?? (static () => ViewRoute.None), enabled),
        ],
    };

    private static string Show(TestApplication app, Form form)
    {
        FormHostView.Hosted = form;
        app.Navigator.Apply(ViewKind.FormHost);
        return app.Frame();
    }

    [Fact]
    public void FieldsAreDrawnAsLabelAndValue()
    {
        using var app = new TestApplication();
        var frame = Show(app, CreateForm(app, new TrackedAtom<string>(""), new TrackedAtom<bool>(true)));

        Assert.Contains("Name = ", frame, StringComparison.Ordinal);
        Assert.Contains("Flag = Yes", frame, StringComparison.Ordinal);
        Assert.Contains("> Apply", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyValuesFallBackToTheEmptyString()
    {
        using var app = new TestApplication();
        var frame = Show(app, CreateForm(app, new TrackedAtom<string>(""), new TrackedAtom<bool>(false)));

        Assert.Contains($"Name = {app.Options.Strings.Empty()}", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void ArrowsMoveTheSelection()
    {
        using var app = new TestApplication();
        var form = CreateForm(app, new TrackedAtom<string>(""), new TrackedAtom<bool>(false));

        Assert.Equal(0, form.Selected);

        form.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        Assert.Equal(1, form.Selected);

        form.Handle(new('\0', ConsoleKey.UpArrow, false, false, false));
        Assert.Equal(0, form.Selected);
    }

    [Fact]
    public void ConfirmOpensTheModalOfTheField()
    {
        using var app = new TestApplication();
        var name = new TrackedAtom<string>("");
        var form = CreateForm(app, name, new TrackedAtom<bool>(false));

        form.Handle(new('\0', ConsoleKey.Enter, false, false, false));

        Assert.IsType<TextModal>(app.State.Modal);

        app.Type("fEst");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("fEst", name.Value);
    }

    [Fact]
    public void ToggleFieldWritesBackThroughItsModal()
    {
        using var app = new TestApplication();
        var flag = new TrackedAtom<bool>(true);
        var form = CreateForm(app, new TrackedAtom<string>(""), flag);

        form.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        form.Handle(new('\0', ConsoleKey.Enter, false, false, false));

        Assert.IsType<ToggleModal>(app.State.Modal);

        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.Enter);

        Assert.False(flag.Value);
    }

    [Fact]
    public void EraseResetsTheField()
    {
        using var app = new TestApplication();
        var name = new TrackedAtom<string>("filled");
        var form = CreateForm(app, name, new TrackedAtom<bool>(false));

        form.Handle(new('\0', ConsoleKey.Backspace, false, false, false));

        Assert.Equal("", name.Value);
    }

    [Fact]
    public void ActionRunsAndCanNavigate()
    {
        using var app = new TestApplication();
        var form = CreateForm(app, new TrackedAtom<string>(""), new TrackedAtom<bool>(false), () => ViewKind.Other);

        form.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        form.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));

        Assert.Equal(ViewKind.Other, form.Handle(new('\0', ConsoleKey.Enter, false, false, false)));
    }

    [Fact]
    public void DisabledActionDoesNothing()
    {
        using var app = new TestApplication();
        var ran = false;
        var form = CreateForm(app, new TrackedAtom<string>(""), new TrackedAtom<bool>(false),
            () =>
            {
                ran = true;
                return ViewRoute.None;
            },
            static () => false);

        form.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        form.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        form.Handle(new('\0', ConsoleKey.Enter, false, false, false));

        Assert.False(ran);
    }

    [Fact]
    public void ClickSelectsAndTheSecondClickActivates()
    {
        using var app = new TestApplication();
        var name = new TrackedAtom<string>("");
        var form = CreateForm(app, name, new TrackedAtom<bool>(false));

        var lines = Show(app, form).Split("\r\n");
        var flagRow = Array.FindIndex(lines, line => line.Contains("Flag", StringComparison.Ordinal));
        var column = FormHostView.Rows.Left;

        form.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, flagRow, column, default));
        Assert.Equal(1, form.Selected);
        Assert.Null(app.State.Modal);

        form.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, flagRow, column, default));
        Assert.IsType<ToggleModal>(app.State.Modal);
    }

    [Fact]
    public void EditingThroughAFieldCanBeUndone()
    {
        using var app = new TestApplication();
        var name = new TrackedAtom<string>("before");

        var form = CreateForm(app, name, new TrackedAtom<bool>(false));

        form.Handle(new('\0', ConsoleKey.Enter, false, false, false));
        app.Type("after");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("beforeafter", name.Value);

        Assert.True(app.History.Undo());
        Assert.Equal("before", name.Value);
    }

    [Fact]
    public void HelpOfTheSelectedFieldIsShown()
    {
        using var app = new TestApplication();
        var form = new Form(app.State, app.Options)
        {
            Fields =
            [
                Field.Text(static () => "First", new TrackedAtom<string>(""), help: static () => "help for first"),
                Field.Text(static () => "Second", new TrackedAtom<string>(""), help: static () => "help for second"),
            ],
        };

        Assert.Contains("help for first", Show(app, form), StringComparison.Ordinal);

        form.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        Assert.Contains("help for second", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void PathFieldOpensTheFilePicker()
    {
        using var app = new TestApplication();
        var folder = new TrackedAtom<string>("");
        var form = new Form(app.State, app.Options)
        {
            Fields = [Field.Path(static () => "Folder", folder, ViewKind.Probe, pickFolder: true)],
        };

        var route = form.Handle(new('\0', ConsoleKey.Enter, false, false, false));

        Assert.Equal(Routes.FilePicker, route);
        Assert.NotNull(app.State.FilePicker);
    }
}
