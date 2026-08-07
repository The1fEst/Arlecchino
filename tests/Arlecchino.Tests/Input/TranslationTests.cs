using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class TranslationTests
{
    private static readonly string[] EnglishTheFrameworkDraws =
    [
        "Keys", "Commands", "empty", "nothing matches", "Filter", "move", "edit", "reset",
        "confirm", "cancel", "close", "back", "clear", "Terminal window is too small",
        "needed at least", "nothing logged yet", "Everywhere", "no commands registered",
    ];

    [Fact]
    public void NoEnglishSurvivesAFullTranslation()
    {
        using var app = new TestApplication(100, 30, static builder => builder.UseStrings(Translated()));

        var frames = new List<string> { app.Frame() };

        app.Navigator.Apply(Routes.Help);
        frames.Add(app.Frame());

        app.Navigator.Apply(Routes.Notifications);
        frames.Add(app.Frame());

        app.Navigator.Back();
        app.State.RequestText("«название»", "", null, static _ => { });
        frames.Add(app.Frame());

        app.State.CloseAllModals();
        app.State.RequestChoice("«выбор»", ["«альфа»", "«бета»"], static _ => { });
        frames.Add(app.Frame());

        foreach (var frame in frames)
        {
            foreach (var english in EnglishTheFrameworkDraws)
            {
                Assert.DoesNotContain(english, frame, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void ATranslationCanBePartial()
    {
        using var app = new TestApplication(100,
            30,
            static builder =>
                builder.UseStrings(new() { KeysTitle = static () => "«клавиши»" }));

        var frame = app.Frame();

        Assert.Contains("«клавиши»", frame, StringComparison.Ordinal);
        Assert.Contains("other", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryStringCanBeReplaced()
    {
        var translated = Translated();

        Assert.Empty(Untranslated(translated));
        Assert.Empty(Untranslated(translated.FilePicker));
    }

    private static ArlecchinoStrings Translated()
    {
        var strings = new ArlecchinoStrings();

        Replace(strings);
        Replace(strings.FilePicker);

        strings.HelpKeys = static keymap => [(keymap.Help, "«описание»")];

        return strings;
    }

    private static void Replace(object target)
    {
        var index = 0;

        foreach (var property in Delegates(target.GetType()))
        {
            var marker = $"«строка{index++}»";
            var parameters = property.PropertyType.GetGenericArguments()[..^1]
                .Select(static (type, index) => Expression.Parameter(type, $"argument{index}"))
                .ToArray();

            var lambda = Expression.Lambda(
                property.PropertyType,
                Expression.Constant(marker),
                parameters);

            property.SetValue(target, lambda.Compile());
        }
    }

    private static string[] Untranslated(object target) => Delegates(target.GetType())
        .Where(property => Invoke(property.GetValue(target)) is not { } value ||
                           !value.StartsWith('«'))
        .Select(static property => property.Name)
        .ToArray();

    private static IEnumerable<PropertyInfo> Delegates(Type type) => type
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(static property => property.PropertyType.IsGenericType &&
                                  property.PropertyType.Name.StartsWith("Func", StringComparison.Ordinal) &&
                                  property.PropertyType.GetGenericArguments()[^1] == typeof(string));

    private static string? Invoke(object? candidate)
    {
        if (candidate is not Delegate action)
        {
            return null;
        }

        var arguments = action.GetType().GetGenericArguments()[..^1]
            .Select(static type => type.IsValueType ? Activator.CreateInstance(type) : null)
            .ToArray();

        return action.DynamicInvoke(arguments) as string;
    }
}
