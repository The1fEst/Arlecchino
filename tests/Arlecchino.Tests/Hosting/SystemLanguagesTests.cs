using System;
using System.Collections.Generic;
using Xunit;

namespace Arlecchino.Tests.Hosting;

public sealed class SystemLanguagesTests
{
    [Theory]
    [InlineData("LANG=ru_RU.UTF-8", "ru-RU")]
    [InlineData("LANG=ru_RU.UTF-8@euro", "ru-RU")]
    [InlineData("LANG=en", "en")]
    [InlineData("LC_ALL=de_DE.UTF-8;LANG=ru_RU.UTF-8", "de-DE")]
    [InlineData("LC_MESSAGES=fr_FR.UTF-8;LANG=ru_RU.UTF-8", "fr-FR")]
    [InlineData("LC_ALL=;LANG=ru_RU.UTF-8", "ru-RU")]
    [InlineData("LANG=ru_RU.UTF-8;LANGUAGE=de:en", "de,en,ru-RU")]
    [InlineData("LANG=ru_RU.UTF-8;LANGUAGE=ru_RU:ru:RU-ru", "ru-RU,ru")]
    [InlineData("LANG=C;LANGUAGE=de:en", "")]
    [InlineData("LANG=POSIX;LANGUAGE=de", "")]
    [InlineData("LANG=C.UTF-8;LANGUAGE=de", "")]
    [InlineData("LC_ALL=C;LANG=ru_RU.UTF-8", "")]
    [InlineData("LANG=ru_RU.UTF-8;LANGUAGE=C:de", "de,ru-RU")]
    [InlineData("LANG=;LANGUAGE=de", "")]
    [InlineData("", "")]
    public void TheEnvironmentIsReadWithGettextPrecedence(string environment, string expected)
    {
        var variables = new Dictionary<string, string>();
        foreach (var pair in environment.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=');
            variables[parts[0]] = parts[1];
        }

        var languages = SystemLanguages.FromEnvironment(name => variables.GetValueOrDefault(name));

        Assert.Equal(expected, string.Join(",", languages));
    }

    [Theory]
    [InlineData("ru-RU\0en-US\0\0", "ru-RU,en-US")]
    [InlineData("ru-RU\0RU-ru\0en\0\0", "ru-RU,en")]
    [InlineData("en\0\0", "en")]
    [InlineData("\0", "")]
    [InlineData("", "")]
    public void ADoubleNullTerminatedListFromWindowsIsSplit(string list, string expected) =>
        Assert.Equal(expected, string.Join(",", SystemLanguages.FromWindowsList(list)));
}
