using System;
using Arlecchino.Atoms.History;
using Arlecchino.Atoms.History.Collections;
using Arlecchino.Forms;
using Arlecchino.Navigation;
using Arlecchino.Rendering.Colors;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

public sealed class FieldTests
{
    private const string SecretKey = "Field.Secret";
    private const string NumberKey = "Field.Number";
    private const string HeldKey = "Field.Held";
    private const string SliderKey = "Field.Slider";
    private const string ChoiceKey = "Field.Choice";
    private const string ColumnsKey = "Field.Columns";
    private const string DateKey = "Field.Date";
    private const string TimeKey = "Field.Time";
    private const string ColorKey = "Field.Color";
    private const string TextKey = "Field.Text";

    private static readonly string[] _options = ["alpha", "beta", "gamma"];

    [Fact]
    public void ASecretIsShownAsDotsAndTypedInFull()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<string>("hunter2", SecretKey);
        var field = Fields.Secret(static () => "Password", value);

        Assert.Equal("•••••••", field.Template!());

        Open(app, field);
        app.Type("opensesame");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("opensesame", value.Value);
    }

    [Fact]
    public void ANumberIsSteppedAndWrittenBack()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<decimal>(12, NumberKey);

        Assert.Equal("12", value.ToString());

        Open(app, Fields.Number(static () => "Count", value));
        app.Press(ConsoleKey.PageUp);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(22, value.Value);
    }

    /// <summary>
    /// The values a number may hold stand for its range as well, which is the one place a form says what a
    /// number is allowed to be.
    /// </summary>
    [Fact]
    public void ANumberIsHeldBetweenTheEndsOfWhatItMayBe()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<decimal>(95, HeldKey);

        Open(app, Fields.Number(static () => "Count", value).WithItems(static () => [0, 100]));
        app.Press(ConsoleKey.PageUp);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(100, value.Value);
    }

    [Fact]
    public void ASliderWritesWhatItWasDraggedTo()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<decimal>(50, SliderKey);

        Open(app, Fields.Slider(static () => "Volume", value));
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.Enter);

        Assert.True(value.Value > 50);
    }

    [Fact]
    public void AChoiceWritesWhatWasPicked()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<string>("alpha", ChoiceKey);

        Open(app, Fields.Choice(static () => "Kind", _options, value));
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal("beta", value.Value);
    }

    [Fact]
    public void AMultiChoiceWritesEverythingThatWasTicked()
    {
        using var app = new TestApplication();
        var value = new HistoryAtomsList<string>(ColumnsKey, ["alpha"]);

        Assert.Equal("alpha", value.ToString());

        Open(app, Fields.MultiChoice(static () => "Columns", _options, value));
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Spacebar);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(["alpha", "beta"], value.Value);
    }

    [Fact]
    public void ADateIsShownAndWritten()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<DateOnly>(new(2026, 7, 26), DateKey);

        Assert.Contains("2026", value.ToString(), StringComparison.Ordinal);

        Open(app, Fields.Date(static () => "When", value));
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(2026, 7, 27), value.Value);
    }

    [Fact]
    public void ATimeIsShownAndWritten()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<TimeOnly>(new(9, 30), TimeKey);

        Assert.Contains("30", value.ToString(), StringComparison.Ordinal);

        Open(app, Fields.Time(static () => "At", value));
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Enter);

        Assert.NotEqual(new(9, 30), value.Value);
    }

    [Fact]
    public void AColourIsShownAsItsHexAndWritten()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<RGB>(new(255, 0, 0), ColorKey);

        Assert.Contains("#", value.ToString(), StringComparison.Ordinal);

        Open(app, Fields.Color(static () => "Accent", value));
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(255, 0, 0), value.Value);
    }

    /// <summary>
    /// A field says which values it takes rather than why the rest are refused, so the dialog stays open on
    /// the one thing it can say.
    /// </summary>
    [Fact]
    public void AValueTheFieldDoesNotAllowLeavesTheDialogOpen()
    {
        using var app = new TestApplication();
        var value = new HistoryAtom<string>("", TextKey);

        Open(app, Fields.Text(static () => "Name", value).WithValidation(static typed => typed.Length > 3));
        app.Type("ab");
        app.Press(ConsoleKey.Enter);

        Assert.NotNull(app.State.Modal);
        Assert.Equal("", value.Value);
    }

    [Fact]
    public void HelpIsNotThereUnlessItIsGiven()
    {
        var value = new HistoryAtom<string>("", TextKey);

        Assert.Null(Fields.Text(static () => "Name", value).Help);
        Assert.Equal("the one on the badge",
            Fields.Text(static () => "Name", value).WithHelp(static () => "the one on the badge").Help!());
    }

    [Fact]
    public void AButtonCarriesNoValueAndHasNothingToPutBack()
    {
        var field = Fields.Action(static () => "Apply", static () => ViewRoute.None);

        Assert.Null(field.Value);
        Assert.False(field.TryReset());
    }

    private static void Open(TestApplication app, IField field)
    {
        var form = new Form(app.State, app.Options) { Fields = [field] };

        form.Handle(new(ConsoleKey.Enter));
    }
}
