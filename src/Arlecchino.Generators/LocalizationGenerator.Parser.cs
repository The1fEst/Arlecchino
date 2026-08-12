using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Arlecchino.Generators;

public sealed partial class LocalizationGenerator
{
    /// <summary>
    /// Reads one localization file: a <c>[localization]</c> table naming the language and a
    /// <c>[strings]</c> table of quoted values, which is as much TOML as this needs.
    /// </summary>
    /// <param name="source">The file and what is in it.</param>
    /// <returns>What it said, and everything wrong with it.</returns>
    private static LocalizationFile Parse(LocalizationSource source)
    {
        var language = "";
        var section = "";
        var entries = new List<LocalizationEntry>();
        var errors = new List<string>();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var lines = source.Text.Replace("\r\n", "\n").Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();

            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
            {
                section = line.Substring(1, line.Length - 2).Trim();

                continue;
            }

            var equals = line.IndexOf('=');

            if (equals <= 0)
            {
                errors.Add(Complaint(source.Path, index, "expected name = \"text\""));

                continue;
            }

            var key = line.Substring(0, equals).Trim();

            if (!TryReadString(line.Substring(equals + 1).Trim(), out var value))
            {
                errors.Add(Complaint(source.Path, index, $"'{key}' must be a quoted string"));

                continue;
            }

            if (section.Equals("localization", StringComparison.Ordinal) && key == "language")
            {
                language = value;

                continue;
            }

            if (!section.Equals("strings", StringComparison.Ordinal))
            {
                continue;
            }

            if (!IsName(key))
            {
                errors.Add(Complaint(source.Path, index, $"'{key}' is not a name a C# member can have"));

                continue;
            }

            if (!keys.Add(key))
            {
                errors.Add(Complaint(source.Path, index, $"'{key}' is in this file twice"));

                continue;
            }

            entries.Add(new(key, value));
        }

        if (language.Length == 0)
        {
            errors.Add($"{source.Path}: [localization] must say which language this is");
        }

        if (entries.Count == 0)
        {
            errors.Add($"{source.Path}: [strings] holds nothing");
        }

        return new(language, entries, errors);
    }

    private static bool TryReadString(string raw, out string value)
    {
        value = "";

        if (raw.Length < 2 || raw[0] != '"' || raw[raw.Length - 1] != '"')
        {
            return false;
        }

        var text = new StringBuilder();

        for (var index = 1; index < raw.Length - 1; index++)
        {
            var character = raw[index];

            if (character != '\\')
            {
                text.Append(character);

                continue;
            }

            if (++index >= raw.Length - 1 || !TryEscape(raw, ref index, text))
            {
                return false;
            }
        }

        value = text.ToString();

        return true;
    }

    private static bool TryEscape(string raw, ref int index, StringBuilder text)
    {
        switch (raw[index])
        {
            case 'n':
                text.Append('\n');

                return true;
            case 'r':
                text.Append('\r');

                return true;
            case 't':
                text.Append('\t');

                return true;
            case '"':
                text.Append('"');

                return true;
            case '\\':
                text.Append('\\');

                return true;
            case 'u' when index + 4 < raw.Length - 1:
                var digits = raw.Substring(index + 1, 4);

                if (!ushort.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var point))
                {
                    return false;
                }

                text.Append((char)point);
                index += 4;

                return true;
            default:
                return false;
        }
    }

    private static bool IsName(string key)
    {
        if (key.Length == 0 || !(char.IsLetter(key[0]) || key[0] == '_'))
        {
            return false;
        }

        for (var index = 1; index < key.Length; index++)
        {
            if (!char.IsLetterOrDigit(key[index]) && key[index] != '_')
            {
                return false;
            }
        }

        return true;
    }

    private static string Complaint(string path, int line, string message) => $"{path}({line + 1}): {message}";

    private sealed class LocalizationSource
    {
        public LocalizationSource(string path, string text)
        {
            Path = path;
            Text = text;
        }

        public string Path { get; }

        public string Text { get; }
    }

    private sealed class LocalizationEntry
    {
        public LocalizationEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }

    private sealed class LocalizationFile
    {
        public LocalizationFile(
            string language,
            IReadOnlyList<LocalizationEntry> entries,
            IReadOnlyList<string> errors)
        {
            Language = language;
            Entries = entries;
            Errors = errors;
        }

        public string Language { get; }

        public IReadOnlyList<LocalizationEntry> Entries { get; }

        public IReadOnlyList<string> Errors { get; }
    }
}
