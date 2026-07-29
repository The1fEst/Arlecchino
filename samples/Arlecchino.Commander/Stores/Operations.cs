using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms;
using Arlecchino.Commander.Files;
using Arlecchino.Commander.Model;
using Arlecchino.Diagnostics;
using Arlecchino.State;

namespace Arlecchino.Commander.Stores;

/// <summary>
/// The copy, move and delete that are running, kept outside any one screen. Work started from the
/// panels outlives them — opening the notifications and coming back builds the commander again — so a
/// store holds it, and the screen reads what is running rather than owning it.
/// </summary>
public sealed class Operations : IArlecchinoStore
{
    private const int RedrawInterval = 120;

    private readonly ArlecchinoState _state;

    private CancellationTokenSource? _cancelling;
    private Outcome? _running;
    private Notification? _reporting;
    private string _busy = "";

    public Operations(ArlecchinoState state) => _state = state;

    /// <summary>Bumped whenever work finishes, so a screen knows its panels are out of date.</summary>
    public Atom<int> Revision { get; } = new LocalAtom<int>(0);

    /// <summary>Whether a copy, move or delete is still running in the background.</summary>
    public bool IsBusy => _running is not null;

    /// <summary>Whether the work has been sized up, which is when a bar for it means anything.</summary>
    public bool IsMeasured => _running?.IsMeasured ?? false;

    /// <summary>How full that bar should be, from <c>0</c> to <c>1</c>.</summary>
    public double Share => _running?.Share ?? 0;

    /// <summary>What to show while the work runs: what it is doing and how far it has got.</summary>
    /// <returns>The line, or an empty string when nothing is running.</returns>
    public string Progress() => _running is { } outcome ? $"{_busy} {outcome.Progress()}" : "";

    /// <summary>Asks the running operation to stop; what it already did stays done.</summary>
    public void Cancel() => _cancelling?.Cancel();

    public void Copy(IFileSource from, IReadOnlyList<FileEntry> sources, IFileSource to, string target) =>
        Start(
            (outcome, token) => FileTasks.Copy(from, sources, to, target, outcome, token),
            "Copied",
            "Copying",
            Sizing(from, sources));

    public void Move(IFileSource from, IReadOnlyList<FileEntry> sources, IFileSource to, string target) =>
        Start(
            (outcome, token) => Moving(from, sources, to, target, outcome, token),
            "Moved",
            "Moving",
            Sizing(from, sources));

    public void Rename(IFileSource source, FileEntry entry, string target) =>
        Start((outcome, _) => FileTasks.Rename(source, entry, target, outcome), "Renamed", "Renaming");

    public void Delete(IFileSource source, IReadOnlyList<FileEntry> entries) =>
        Start(
            (outcome, token) => FileTasks.Delete(source, entries, outcome, token),
            "Deleted",
            "Deleting",
            Sizing(source, entries));

    private static void Moving(
        IFileSource from,
        IReadOnlyList<FileEntry> sources,
        IFileSource to,
        string target,
        Outcome outcome,
        CancellationToken token)
    {
        if (sources.Count == 1 && !to.FolderExists(target))
        {
            if (ReferenceEquals(from, to))
            {
                FileTasks.Rename(from, sources[0], target, outcome);
                return;
            }

            FileTasks.Move(from, sources, to, to.Parent(target) ?? target, outcome, token);
            return;
        }

        FileTasks.Move(from, sources, to, target, outcome, token);
    }

    /// <summary>
    /// The counting pass, or nothing when counting would cost more than the work. Walking a tree on a
    /// server is a request per folder, and a delete there is one command anyway, so the bar is worth
    /// less than the wait it would add.
    /// </summary>
    /// <param name="source">Where the entries live.</param>
    /// <param name="entries">What is about to be worked on.</param>
    /// <returns>How to count, or <c>null</c> to skip counting.</returns>
    private static Func<CancellationToken, Tally>? Sizing(IFileSource source, IReadOnlyList<FileEntry> entries) =>
        source.IsRemote ? null : token => FileTasks.Measure(source, entries, token);

    /// <summary>
    /// Runs the work off the drawing thread — even on a local disk, where a folder deep enough would
    /// otherwise freeze the frame — and reports it as a notification that counts up while it goes and
    /// turns into what came of it at the end.
    /// </summary>
    private void Start(
        Action<Outcome, CancellationToken> work,
        string verb,
        string busy,
        Func<CancellationToken, Tally>? measure = null)
    {
        if (IsBusy)
        {
            _state.Output = $"{_busy} still, Esc stops it";
            return;
        }

        var outcome = new Outcome();
        var cancelling = new CancellationTokenSource();

        _running = outcome;
        _busy = busy;
        _cancelling = cancelling;
        _reporting = _state.Notifications.Raise(new(DateTimeOffset.Now, NotificationLevel.Information, busy)
        {
            Progress = Progress,
            Share = () => outcome.IsMeasured ? outcome.Share : null,
            Detail = Progress,
            Actions = [new(static () => "Stop", Cancel)],
        });

        Redraw(cancelling.Token);

        Task.Run(() =>
        {
            try
            {
                outcome.Planning(measure?.Invoke(cancelling.Token) ?? default);

                work(outcome, cancelling.Token);
            }
            finally
            {
                var stopped = cancelling.Token.IsCancellationRequested;

                FrameThread.Post(() => Finish(outcome, verb, stopped));
            }
        });
    }

    private void Redraw(CancellationToken token) => Task.Run(async () =>
    {
        while (!token.IsCancellationRequested && IsBusy)
        {
            await Task.Delay(RedrawInterval, CancellationToken.None);

            FrameThread.Post(_state.Invalidate);
        }
    });

    private void Finish(Outcome outcome, string verb, bool stopped)
    {
        var errors = outcome.Errors;
        var said = stopped ? $"{outcome.Describe(verb)} — stopped" : outcome.Describe(verb);

        _running = null;
        _cancelling?.Dispose();
        _cancelling = null;

        if (_reporting is { } reporting)
        {
            reporting.Detail = errors.Count > 0 ? () => string.Join(Environment.NewLine, errors) : null;

            _state.Notifications.Settle(
                reporting,
                errors.Count > 0 ? $"{said} — Enter reads them" : said,
                errors.Count > 0 ? NotificationLevel.Failure : NotificationLevel.Information);

            _reporting = null;
        }

        Revision.Value++;
    }
}
