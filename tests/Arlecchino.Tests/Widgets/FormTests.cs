using System;
using Arlecchino.Forms;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Setting;
using Arlecchino.Atoms;
using Arlecchino.Atoms.History;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

public sealed class FormTests
{
    private const string TextKey = "Form.Text";
    private const string FlagKey = "Form.Flag";

    private static Form CreateForm(
        TestApplication app,
        Atom<string> name,
        Atom<bool> flag,
        Func<ViewRoute>? action = null,
        bool enabled = true) => new(app.State, app.Options)
    {
        Fields =
        [
            Fields.Text(static () => "Name", name),
            Fields.Toggle(static () => "Flag", flag),
            new ActionField(static () => "Apply", action ?? (static () => ViewRoute.None)) { IsEnabled = enabled },
        ],
    };

    private static string Show(TestApplication app, Form form)
    {
        FormHostView.Form = form;
        app.Navigator.Apply(ViewKind.FormHost);
        return app.Frame();
    }

    [Fact]
    public void FieldsAreDrawnAsLabelAndValue()
    {
        using var app = new TestApplication();
        var frame = Show(app, CreateForm(app, new HistoryAtom<string>("", TextKey), new HistoryAtom<bool>(true, FlagKey)));

        Assert.Contains("Name = ", frame, StringComparison.Ordinal);
        Assert.Contains("Flag = Yes", frame, StringComparison.Ordinal);
        Assert.Contains("> Apply", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyValuesFallBackToTheEmptyString()
    {
        using var app = new TestApplication();
        var frame = Show(app, CreateForm(app, new HistoryAtom<string>("", TextKey), new HistoryAtom<bool>(false, FlagKey)));

        Assert.Contains($"Name = {app.Options.Strings.Empty()}", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void ArrowsMoveTheSelection()
    {
        using var app = new TestApplication();
        var form = CreateForm(app, new HistoryAtom<string>("", TextKey), new HistoryAtom<bool>(false, FlagKey));

        Assert.Equal(0, form.SelectedIndex);

        form.Handle(new(ConsoleKey.DownArrow));
        Assert.Equal(1, form.SelectedIndex);

        form.Handle(new(ConsoleKey.UpArrow));
        Assert.Equal(0, form.SelectedIndex);
    }

    [Fact]
    public void ConfirmOpensTheModalOfTheField()
    {
        using var app = new TestApplication();
        var name = new HistoryAtom<string>("", TextKey);
        var form = CreateForm(app, name, new HistoryAtom<bool>(false, FlagKey));

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
        var flag = new HistoryAtom<bool>(true, FlagKey);
        var form = CreateForm(app, new HistoryAtom<string>("", TextKey), flag);

        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.Enter));

        Assert.IsType<ToggleModal>(app.State.Modal);

        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.Enter);

        Assert.False(flag.Value);
    }

    [Fact]
    public void EraseTakesTheFieldBackToWhatItStartedAt()
    {
        using var app = new TestApplication();
        var name = new HistoryAtom<string>("start", TextKey);
        var form = CreateForm(app, name, new HistoryAtom<bool>(false, FlagKey));

        name.Value = "filled";
        form.Handle(new(ConsoleKey.Backspace));

        Assert.Equal("start", name.Value);
    }

    [Fact]
    public void ActionRunsAndCanNavigate()
    {
        using var app = new TestApplication();
        var form = CreateForm(app, new HistoryAtom<string>("", TextKey), new HistoryAtom<bool>(false, FlagKey), () => ViewKind.Other);

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
            new HistoryAtom<string>("", TextKey),
            new HistoryAtom<bool>(false, FlagKey),
            () =>
            {
                ran = true;
                return ViewRoute.None;
            },
            enabled: false);

        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.DownArrow));
        form.Handle(new(ConsoleKey.Enter));

        Assert.False(ran);
    }

    [Fact]
    public void ClickSelectsAndTheSecondClickActivates()
    {
        using var app = new TestApplication();
        var name = new HistoryAtom<string>("", TextKey);
        var form = CreateForm(app, name, new HistoryAtom<bool>(false, FlagKey));

        var lines = Show(app, form).Split("\r\n");
        var flagRow = Array.FindIndex(lines, line => line.Contains("Flag", StringComparison.Ordinal));
        var column = FormHostView.Rows.Left;

        form.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, flagRow, column, default));
        Assert.Equal(1, form.SelectedIndex);
        Assert.Null(app.State.Modal);

        form.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, flagRow, column, default));
        Assert.IsType<ToggleModal>(app.State.Modal);
    }

    [Fact]
    public void EditingThroughAFieldCanBeUndone()
    {
        using var app = new TestApplication();
        var name = new HistoryAtom<string>("before", TextKey);

        var form = CreateForm(app, name, new HistoryAtom<bool>(false, FlagKey));

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
                Fields.Text(static () => "First", new HistoryAtom<string>("", TextKey))
                    .WithHelp(static () => "help for first"),
                Fields.Text(static () => "Second", new HistoryAtom<string>("", TextKey))
                    .WithHelp(static () => "help for second"),
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
                Fields.Text(static () => "First", new HistoryAtom<string>("one", TextKey)),
                Fields.Text(static () => "Second", new HistoryAtom<string>("two", TextKey)),
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
                Fields.Text(static () => "First", new HistoryAtom<string>("", TextKey))
                    .WithHelp(static () => "help for first"),
                Fields.Text(static () => "Second", new HistoryAtom<string>("", TextKey)),
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
        var folder = new HistoryAtom<string>("", TextKey);
        var form = new Form(app.State, app.Options)
        {
            Fields = [Fields.Path(static () => "Folder", folder, ViewKind.Probe, pickFolder: true)],
        };

        var result = form.Handle(new(ConsoleKey.Enter));

        Assert.Equal(Routes.FilePicker, result.Route);
        Assert.NotNull(app.State.FilePicker);
    }

    [Fact]
    public void AnEmptyPathFieldOpensThePickerWhereItWasTold()
    {
        using var app = new TestApplication();
        var folder = new HistoryAtom<string>("", TextKey);

        Open(app,
            Fields.Path(static () => "Folder", folder, ViewKind.Probe, pickFolder: true)
                .WithStart(static () => "D:/projects"));

        Assert.Equal("D:/projects", app.State.FilePicker!.InitialPath);
    }

    [Fact]
    public void APathFieldThatHasAValueOpensThere()
    {
        using var app = new TestApplication();
        var folder = new HistoryAtom<string>("D:/games/saves", TextKey);

        Open(app,
            Fields.Path(static () => "Folder", folder, ViewKind.Probe, pickFolder: true)
                .WithStart(static () => "D:/projects"));

        Assert.Equal("D:/games/saves", app.State.FilePicker!.InitialPath);
    }

    [Fact]
    public void WhereToStartIsAskedForWhenThePickerOpens()
    {
        using var app = new TestApplication();
        var folder = new HistoryAtom<string>("", TextKey);
        var last = new[] { "D:/first" };

        var field = Fields.Path(static () => "Folder", folder, ViewKind.Probe, pickFolder: true)
            .WithStart(() => last[0]);

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
        var folder = new HistoryAtom<string>("", TextKey);

        Open(app, Fields.Path(static () => "Folder", folder, ViewKind.Probe, pickFolder: true));

        Assert.Equal("", app.State.FilePicker!.InitialPath);
    }

    private static void Open(TestApplication app, IField field)
    {
        var form = new Form(app.State, app.Options) { Fields = [field] };

        form.Handle(new(ConsoleKey.Enter));
    }
}
