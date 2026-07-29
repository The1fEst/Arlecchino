using System;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Diagnostics;

/// <summary>
/// Everything the framework says to a log, in one place. The messages are generated rather than
/// formatted at the call site, so a template is written once and nothing is boxed or composed unless
/// the message is actually going somewhere.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Warning,
        Message = "{Route} binds {Binding} to both '{Kept}' and '{Shadowed}'; only the first one can run.")]
    public static partial void KeyBoundTwice(
        ILogger logger,
        ViewRoute route,
        KeyBinding binding,
        string kept,
        string shadowed);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "{Route} binds {Binding} to '{View}', shadowing the application command '{Global}'.")]
    public static partial void KeyShadowsCommand(
        ILogger logger,
        ViewRoute route,
        KeyBinding binding,
        string view,
        string global);

    [LoggerMessage(Level = LogLevel.Error, Message = "Arlecchino stopped after an unhandled error.")]
    public static partial void HostStopped(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "A store failed to load; the application carries on with what its atoms hold.")]
    public static partial void StoreFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Critical,
        Message = "Arlecchino restored the terminal after an unhandled error.")]
    public static partial void TerminalRestored(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handling {Key} failed at route {Route}.")]
    public static partial void KeyFailed(ILogger logger, Exception exception, ConsoleKey key, ViewRoute route);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handling a mouse event failed at route {Route}.")]
    public static partial void MouseFailed(ILogger logger, Exception exception, ViewRoute route);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handling a paste failed at route {Route}.")]
    public static partial void PasteFailed(ILogger logger, Exception exception, ViewRoute route);

    [LoggerMessage(Level = LogLevel.Error, Message = "Scheduled work failed.")]
    public static partial void TickFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "A posted action failed before the frame was drawn.")]
    public static partial void PostedWorkFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "The view at route {Route} failed to draw.")]
    public static partial void DrawFailed(ILogger logger, Exception exception, ViewRoute route);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "A collection shrank while it was being drawn at route {Route}; {Skipped} frame(s) were cut " +
                  "short. Change what a widget draws from the drawing thread — through FrameThread.Post when " +
                  "the change comes from somewhere else.")]
    public static partial void RowsVanished(ILogger logger, ViewRoute route, int skipped);
}
