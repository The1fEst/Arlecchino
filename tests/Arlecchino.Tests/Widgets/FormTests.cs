using System;
using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Setting;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

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

        form.Handle(new(ConsoleKey.DownArrow));
        Assert.Equal(1, form.Selected);

        form.Handle(new(ConsoleKey.UpArrow));
        Assert.Equal(0, form.Selected);
    }

    [Fact]
    public void ConfirmOpensTheModalOfTheField()
    {
        using var app = new TestApplication();
        var name = new TrackedAtom<string>("");
        var form = CreateForm(app, name, new TrackedAtom<bool>(false));

        form.Handle(new(ConsoleKey.Enter));

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

        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.Enter));

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

        form.Handle(new(ConsoleKey.Backspace));

        Assert.Equal("", name.Value);
    }

    [Fact]
    public void ActionRunsAndCanNavigate()
    {
        using var app = new TestApplication();
        var form = CreateForm(app, new TrackedAtom<string>(""), new TrackedAtom<bool>(false), () => ViewKind.Other);

        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.DownArrow));

        Assert.Equal(ViewKind.Other, form.Handle(new(ConsoleKey.Enter)).Route);
    }

    [Fact]
    public void DisabledActionDoesNothing()
    {
        using var app = new TestApplication();
        var ran = false;
        var form = CreateForm(app,
            new TrackedAtom<string>(""),
            new TrackedAtom<bool>(false),
            () =>
            {
                ran = true;
                return ViewRoute.None;
            },
            static () => false);

        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.Enter));

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

        form.Handle(new(ConsoleKey.Enter));
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

        form.Handle(new(ConsoleKey.DownArrow));
        Assert.Contains("help for second", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void FieldsWithoutHelpAreDrawnOneAfterTheOther()
    {
        using var app = new TestApplication();
        var form = new Form(app.State, app.Options)
        {
            Fields =
            [
                Field.Text(static () => "First", new TrackedAtom<string>("one")),
                Field.Text(static () => "Second", new TrackedAtom<string>("two")),
            ],
        };

        var lines = Show(app, form).Split("\r\n");
        var first = IndexOfLineContaining(lines, "First");

        Assert.Equal("Second", lines[first + 1].Trim().Split(' ')[0]);
    }

    [Fact]
    public void TheHelpRowIsOnlyThereWhenTheSelectedFieldHasHelp()
    {
        using var app = new TestApplication();
        var form = new Form(app.State, app.Options)
        {
            Fields =
            [
                Field.Text(static () => "First", new TrackedAtom<string>(""), help: static () => "help for first"),
                Field.Text(static () => "Second", new TrackedAtom<string>("")),
            ],
        };

        var withHelp = Show(app, form).Split("\r\n");
        var firstRow = IndexOfLineContaining(withHelp, "First");

        Assert.Contains("help for first", withHelp[firstRow + 1], StringComparison.Ordinal);

        form.Handle(new(ConsoleKey.DownArrow));

        var withoutHelp = app.Frame().Split("\r\n");
        var secondRow = IndexOfLineContaining(withoutHelp, "Second");

        Assert.Equal("", withoutHelp[secondRow + 1].Trim());
        Assert.Equal(secondRow - 1, IndexOfLineContaining(withoutHelp, "First"));
    }

    private static int IndexOfLineContaining(string[] lines, string text)
    {
        for (var index = 0; index < lines.Length; index++)
        {
            if (lines[index].Contains(text, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
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

        var result = form.Handle(new(ConsoleKey.Enter));

        Assert.Equal(Routes.FilePicker, result.Route);
        Assert.NotNull(app.State.FilePicker);
    }

    [Fact]
    public void AnEmptyPathFieldOpensThePickerWhereItWasTold()
    {
        using var app = new TestApplication();
        var folder = new TrackedAtom<string>("");

        Open(app, Field.PathFrom(static () => "Folder", folder, ViewKind.Probe, true, static () => "D:/projects"));

        Assert.Equal("D:/projects", app.State.FilePicker!.InitialPath);
    }

    [Fact]
    public void APathFieldThatHasAValueOpensThere()
    {
        using var app = new TestApplication();
        var folder = new TrackedAtom<string>("D:/games/saves");

        Open(app, Field.PathFrom(static () => "Folder", folder, ViewKind.Probe, true, static () => "D:/projects"));

        Assert.Equal("D:/games/saves", app.State.FilePicker!.InitialPath);
    }

    [Fact]
    public void WhereToStartIsAskedForWhenThePickerOpens()
    {
        using var app = new TestApplication();
        var folder = new TrackedAtom<string>("");
        var last = new[] { "D:/first" };

        var field = Field.PathFrom(static () => "Folder", folder, ViewKind.Probe, true, () => last[0]);

        Open(app, field);
        Assert.Equal("D:/first", app.State.FilePicker!.InitialPath);

        last[0] = "D:/second";
        app.State.FilePicker = null;

        Open(app, field);
        Assert.Equal("D:/second", app.State.FilePicker!.InitialPath);
    }

    [Fact]
    public void APathFieldWithNowhereToStartIsStillOpened()
    {
        using var app = new TestApplication();
        var folder = new TrackedAtom<string>("");

        Open(app, Field.Path(static () => "Folder", folder, ViewKind.Probe, true));

        Assert.Equal("", app.State.FilePicker!.InitialPath);
    }

    private static void Open(TestApplication app, Field field)
    {
        var form = new Form(app.State, app.Options) { Fields = [field] };

        form.Handle(new(ConsoleKey.Enter));
    }
}
