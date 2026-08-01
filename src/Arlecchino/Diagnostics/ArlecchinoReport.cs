using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Arlecchino.Atoms;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;

namespace Arlecchino.Diagnostics;

/// <summary>
/// What the application looks like right now, as text worth attaching to a bug report: the version,
/// the platform, what the terminal said it can do, the screen being shown and the modals above it.
/// Resolve it from the container and call <see cref="Describe"/> — a command that copies the result
/// to the clipboard costs three lines and makes a report from a user useful.
/// </summary>
public sealed class ArlecchinoReport
{
    private readonly IArlecchinoTerminal _terminal;
    private readonly Navigator _navigator;
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoOptions _options;
    private readonly Surface _surface;
    private readonly AtomHistory _history;

    /// <summary>Creates the report. Resolved from the container like any other service.</summary>
    /// <param name="terminal">The terminal being drawn to.</param>
    /// <param name="navigator">Where the application is.</param>
    /// <param name="state">Modals and the output row.</param>
    /// <param name="options">How the application was configured.</param>
    /// <param name="surface">The frame, for its size.</param>
    /// <param name="history">The undo stack, for its depth.</param>
    public ArlecchinoReport(
        IArlecchinoTerminal terminal,
        Navigator navigator,
        ArlecchinoState state,
        ArlecchinoOptions options,
        Surface surface,
        AtomHistory history)
    {
        _terminal = terminal;
        _navigator = navigator;
        _state = state;
        _options = options;
        _surface = surface;
        _history = history;
    }

    /// <summary>
    /// Builds the report. Nothing here is a secret: it is versions, sizes, the route names of the
    /// screens and the type names of the modals — no field values and no text the user typed.
    /// </summary>
    /// <returns>The report, as lines of <c>key: value</c>.</returns>
    public string Describe()
    {
        var report = new StringBuilder();

        Section(report, "Arlecchino");
        Line(report, "version", Version());
        Line(report, "runtime", RuntimeInformation.FrameworkDescription);
        Line(report, "platform", $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");

        Section(report, "Terminal");
        Line(report, "implementation", _terminal.GetType().Name);
        Line(report, "size", $"{_terminal.Width}×{_terminal.Height}");
        Line(report, "frame", $"{_surface.FrameWidth}×{_surface.FrameHeight}");
        Line(report, "colour", TerminalCapabilities.Color.ToString());
        Line(report, "TERM", Variable("TERM"));
        Line(report, "COLORTERM", Variable("COLORTERM"));
        Line(report, "NO_COLOR", Variable("NO_COLOR"));
        Line(report, "WT_SESSION", Variable("WT_SESSION"));
        Line(report, "redirected", $"in {Console.IsInputRedirected}, out {Console.IsOutputRedirected}");

        Section(report, "Screen");
        Line(report, "route", _navigator.CurrentRoute.IsNone ? "none" : _navigator.CurrentRoute.Name);
        Line(report, "can go back", _navigator.CanGoBack.ToString());
        Line(report, "can go forward", _navigator.CanGoForward.ToString());
        Line(report, "commands", _navigator.CurrentCommands.Count.ToString(CultureInfo.InvariantCulture));
        Line(report,
            "modals",
            _state.Modals.Count == 0
                ? "none"
                : string.Join(" over ", _state.Modals.Select(static modal => modal.GetType().Name)));
        Line(report, "undo depth", _history.Depth.ToString(CultureInfo.InvariantCulture));

        Section(report, "Options");
        Line(report, "minimum size", $"{_options.MinimumWidth}×{_options.MinimumHeight}");
        Line(report, "frames per second", _options.TargetFramesPerSecond.ToString(CultureInfo.InvariantCulture));
        Line(report, "alternate screen", _options.UseAlternateScreen.ToString());
        Line(report, "mouse", _options.MouseInput.ToString());
        Line(report, "bracketed paste", _options.BracketedPaste.ToString());
        Line(report, "text input", _options.TextInput.ToString());

        return report.ToString();
    }

    private static string Version() =>
        typeof(ArlecchinoReport).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
        typeof(ArlecchinoReport).Assembly.GetName().Version?.ToString() ??
        "unknown";

    private static string Variable(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : "unset";

    private static void Section(StringBuilder report, string name)
    {
        if (report.Length > 0)
        {
            report.AppendLine();
        }

        report.Append('[').Append(name).AppendLine("]");
    }

    private static void Line(StringBuilder report, string name, string value) =>
        report.Append(name).Append(": ").AppendLine(value);
}
