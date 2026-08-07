using System;
using Arlecchino.Hosting;

using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// What to say when what has been typed will not do.
///
/// A complaint is worked out on every keystroke but only shown once the field has already said something.
/// Nothing is reported before the first attempt to submit, so a field never complains about being half-typed,
/// and a field that is complaining stops the moment it has no reason to.
/// </summary>
internal static class Complaints
{
    /// <summary>What is wrong with what is in a field, or nothing when it will do.</summary>
    /// <param name="modal">The field.</param>
    /// <param name="strings">The words the application says things in.</param>
    /// <returns>The complaint, or <c>null</c>.</returns>
    public static string? About(ITextEntryModal modal, ArlecchinoStrings strings) => modal switch
    {
        NumberModal number => AboutNumber(number, strings),
        TextModal text => AboutFormat(text, strings) ?? text.Validate?.Invoke(text.Text),
        _ => null,
    };

    /// <summary>What is wrong with a number: that it is not one, that it is out of range, or the caller's own.</summary>
    /// <param name="modal">The field.</param>
    /// <param name="strings">The words the application says things in.</param>
    /// <returns>The complaint, or <c>null</c>.</returns>
    public static string? AboutNumber(NumberModal modal, ArlecchinoStrings strings)
    {
        if (!modal.TryGetValue(out var value))
        {
            return strings.NotANumber();
        }

        if (value < modal.Minimum || value > modal.Maximum)
        {
            return strings.OutOfRange(modal.Display(modal.Minimum), modal.Display(modal.Maximum));
        }

        return modal.Validate?.Invoke(value);
    }

    /// <summary>Whether text is the shape the field was told to ask for.</summary>
    /// <param name="modal">The field.</param>
    /// <param name="strings">The words the application says things in.</param>
    /// <returns>The complaint, or <c>null</c>.</returns>
    public static string? AboutFormat(TextModal modal, ArlecchinoStrings strings) => modal.Format switch
    {
        TextFormat.Email when !IsEmailAddress(modal.Text) => strings.NotAnEmail(),
        TextFormat.Url when !IsWebLink(modal.Text) => strings.NotAUrl(),
        _ => null,
    };

    private static bool IsEmailAddress(string text)
    {
        var at = text.IndexOf('@');

        if (at <= 0 || at != text.LastIndexOf('@') || at == text.Length - 1 || text.Contains(' '))
        {
            return false;
        }

        var domain = text[(at + 1)..];

        return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
    }

    private static bool IsWebLink(string text) =>
        Uri.TryCreate(text, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
