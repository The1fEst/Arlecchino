[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md) · [Migrating to 2.0](migrating-to-2.0.md)

# Commands and input

## How a key travels

The hosted service polls `IArlecchinoTerminal` every `InputPollInterval` (8 ms by default) and hands each key to
`InputRouter`, which decides in this order:

1. a modal is open → the key goes to the modal, and nothing else sees it;
2. the key resolves to `CommandPaletteKey` (`:` by default) and at least one command is registered →
   the palette opens;
3. otherwise `Navigator` gets it: `Alt+←` / `Alt+→` walk the history, everything else reaches
   `IArlecchinoView.Handle`.

So a view never has to check for the palette key or guard against typing into a modal.

## Commands

```csharp
public sealed class QuitCommand : IArlecchinoCommand
{
    private readonly IHostApplicationLifetime _lifetime;

    public QuitCommand(IHostApplicationLifetime lifetime) => _lifetime = lifetime;

    public KeyBinding Binding => new(ConsoleKey.Q);
    public string Icon => "×";
    public string Label => "Quit";

    public ViewRoute Execute()
    {
        _lifetime.StopApplication();
        return ViewRoute.None;
    }
}
```

Nothing registers it by hand: `.AddGeneratedCommands()` picks up every `IArlecchinoCommand` in the
project — see [Source generator](source-generator.md#commands). `.AddCommand<QuitCommand>()` is there
for a command that comes from another assembly. Either way commands are singletons resolved from the
container, so they can take any service — application state, the navigator, `ArlecchinoState`.

`Execute` returns a route: navigate by returning one, stay put with `ViewRoute.None`. `Icon` and
`Label` are yours to render; the palette shows the binding and the label.

A binding that carries a modifier — `new(ConsoleKey.S, ConsoleModifiers.Control)` — also fires
globally, before the key reaches the view. A plain letter does not: it would swallow typing, so it is
reachable through the palette and through whatever the view does with it.

`CommandRegistry` is available as a service if a view wants to list or invoke commands itself:
`Commands` is the registered set in registration order, `TryFind(key, out command)` looks one up, and
`Send(key)` executes the match and returns its route.

## Commands of a view

A key a view reacts to belongs in its command list, not in a `switch` — that is what makes it visible
to the palette, to the hints box, and to the conflict check:

```csharp
public IReadOnlyList<ViewCommand> Commands() =>
[
    ViewCommand.For(ConsoleKey.N, () => Loc(LocString.Rename), Rename),
    ViewCommand.Navigating(ConsoleKey.S, () => Loc(LocString.Settings), () => ViewKind.Settings),
    new()
    {
        Binding = new KeyBinding(ConsoleKey.D, ConsoleModifiers.Control),
        Label = () => Loc(LocString.Delete),
        IsEnabled = () => _selected is not null,
        Run = () => Delete(),
    },
];
```

`Run` returns a route, so a command can navigate; `ViewCommand.For` wraps an `Action` for commands
that stay put. A disabled command swallows its key rather than letting it fall through — the key is
spoken for either way.

Keys are resolved in this order:

1. an open modal;
2. the command-palette key;
3. history keys (`Back` / `Forward`);
4. **commands of the current view**;
5. application commands with a modifier;
6. `IArlecchinoView.Handle` — everything else: typing, arrows, list filters.

So a view command shadows an application command on the same key, and that is reported: when the
route is first shown, `CommandConflicts` logs a warning naming both the view command and the
application command it hides, and another one if the view binds the same key twice. That is exactly
the case that used to hide silently — a `Pick a folder` command and a password field both on `p`.

`Hints()` is optional for a view with commands: when it returns nothing, the hints box is built from
the command list, so a rebound key relabels itself there too.

## The command palette

Pressing `:` opens a modal listing the commands of the current view first, then the application
commands, as `key  label`. The next key either runs the matching command or, if nothing matches,
closes the palette and writes `unknown command: <key>` to the output line. A click runs the command
on that row. `Esc` and `Enter` close it silently.

Change the key with `options.CommandPaletteKey = '/'`. The palette does not open while no command is
registered, which leaves the key free for views to handle.

## Keymap

Every key the framework itself reacts to is a `KeyBinding` on `ArlecchinoKeymap`, not a constant buried
in the router:

| Action | Default | Used by |
|---|---|---|
| `Back` / `Forward` | `Alt+←` / `Alt+→` | History |
| `Confirm` / `Cancel` | `Enter` / `Esc` | Every modal, the file picker |
| `NextField` / `PreviousField` | `Tab` / `Shift+Tab` | Segments, colour channels, picker panes |
| `MoveUp` / `MoveDown` / `MoveLeft` / `MoveRight` | arrows | Lists, sliders, number steps, segments |
| `JumpUp` / `JumpDown` | `PgUp` / `PgDn` | Large steps and page moves |
| `First` / `Last` | `Home` / `End` | Ends of a slider, channel or list |
| `Erase` | `Backspace` | Text, filters, typed segments |
| `DeleteForward` | `Delete` | Text fields |
| `EraseWord` / `EraseToStart` | `Ctrl+Backspace` / `Ctrl+U` | Text fields |
| `WordLeft` / `WordRight` | `Ctrl+←` / `Ctrl+→` | Text fields |
| `Copy` | `Ctrl+Insert` or `Ctrl+Shift+C` | Text fields and the multi-line dialog |
| `Submit` | `Ctrl+Enter` | Confirms the [multi-line text dialog](modals-and-state.md), where `Enter` breaks the line |
| `ToggleLog` | `Ctrl+L` | The [log overlay](hosting-and-options.md) |
| `Notifications` | `Ctrl+N` | The [notifications screen](modals-and-state.md) |
| `Help` | `F1` | The keys screen below |
| `Mark` | `Space` | Multi-choice, toggle |
| `PickCurrentFolder` | `Ctrl+Enter` | File picker |

```csharp
builder.Services
    .AddArlecchino()
    .UseKeymap(new ArlecchinoKeymap
    {
        Back = new KeyBinding(ConsoleKey.Backspace),
        Cancel = new KeyBinding(ConsoleKey.Q, ConsoleModifiers.Control),
    });
```

`KeyBinding` matches the key *and* the exact modifiers, so `Ctrl+S` never fires on a bare `S`. Its
`ToString()` is what the palette and the file-picker legend display — `Ctrl+S`, `Alt+←`, `Esc` — so a
remapped key relabels itself everywhere it is shown.

The command-palette key stays a character (`options.CommandPaletteKey`, `:` by default) rather than a
binding: it is resolved through `KeyText`, so it keeps working on a layout where `:` sits somewhere
else.

## The keys screen

The hints box has room for a handful of keys and the palette lists commands only, so there is a screen
that lists everything: `F1` — the `Help` binding — opens `Routes.Help`. It shows every key the
framework answers to with what it does, then the commands of the screen it was opened from under that
screen's route, then the application's own commands with their icon and label — and says so plainly
when none are registered. `Esc` or `F1` again goes back.

The middle section is the one worth knowing about: a view's `Commands()` are the keys that only work
there, so they are the ones somebody pressing `F1` is usually looking for. A screen that registers
none gets no section at all rather than an empty heading.

The wording is localisable like everything else: `HelpKeys` on
[`ArlecchinoStrings`](localization.md) is a delegate that receives the keymap and returns the pairs to
list, so the descriptions can be translated or the order changed without touching the screen.

## Keyboard layouts

Text input — modal fields, list filters, the palette key — goes through `KeyText`, which turns a
`ConsoleKeyInfo` into a character.

| Mode | Behaviour |
|---|---|
| `TextInputMode.LatinOnly` (default) | ASCII characters are taken as typed; anything else falls back to the physical key position, so a Cyrillic layout still produces `q` for the `Q` key |
| `TextInputMode.Native` | Any non-control character is taken as typed |

```csharp
.UseNativeInput()      // or .UseLatinOnlyInput(), or options.TextInput = TextInputMode.Native
```

The fallback covers letters, digits (with shifted symbols), the numpad, space and the OEM punctuation
keys. `KeyText` is registered as a singleton, so a view that reads typed characters itself should take
it as a constructor parameter rather than reading `ConsoleKeyInfo.KeyChar` — that is what keeps
filters working on a non-latin layout.

## Mouse

Mouse reporting is off until you ask for it:

```csharp
builder.Services.AddArlecchino().UseMouse();   // or options.MouseInput = true
```

The hosted service then turns mouse reporting on while it runs and off on the way out — button
presses, releases, drags and the wheel.

How that is done differs by platform, and only `SystemTerminal` knows the difference. Elsewhere it is
SGR reporting (`?1000`, `?1002`, `?1006`) mixed into the key stream. On Windows the console cannot do
that: turning on virtual-terminal input is what delivers SGR reports, and with that flag
`Console.ReadKey` stops delivering keys at all. So Windows reads the console's own event queue
instead — `ReadConsoleInput` with `ENABLE_MOUSE_INPUT`, keys and mouse records out of the same
stream, translated into the same `MouseEvent`. Quick-edit mode is switched off while it runs,
otherwise the console swallows clicks as text selection, and the previous mode is put back when the
mouse is turned off.

That is the one place `IArlecchinoTerminal.MouseAvailable` and `ReadMouse()` matter: they exist for terminals
that deliver the mouse outside the key stream. `TerminalInputReader.ReadPending()` drains both.

Events arrive as `MouseEvent`:

| Member | Meaning |
|---|---|
| `Action` | `Pressed`, `Released`, `Moved` (drag), `ScrolledUp`, `ScrolledDown` |
| `Button` | `Left`, `Middle`, `Right`, or `None` for the wheel |
| `Row`, `Column` | Zero-based cell in the frame — the same coordinates `Surface.WriteAt` takes |
| `Modifiers` | Shift, Alt, Control held at the time |

A view opts in by implementing one method, and navigates by returning a route just as `Handle` does:

```csharp
public ViewRoute HandleMouse(MouseEvent mouse)
{
    if (mouse.IsScroll)
    {
        _offset += mouse.Action == MouseAction.ScrolledDown ? 1 : -1;
        return ViewRoute.None;
    }

    return mouse.IsLeftClick && mouse.Row == _runRow ? ViewKind.Run : ViewRoute.None;
}
```

Because `Row` and `Column` are frame cells, a view that draws with absolute coordinates already knows
where its rows are — hit-testing is comparing numbers. While a list or choice modal is open the wheel
scrolls it; other events are swallowed rather than reaching the view behind the modal.

`TerminalInputReader` is what turns the raw stream into events: it collects escape sequences, hands
mouse reports to the escape-sequence parser inside the package, decodes cursor and function keys itself, and
replays anything it does not recognise as plain keys. A lone `Esc` with nothing behind it stays a
plain `Escape`, so cancelling a modal still works.

The rest of a sequence does not always arrive with its escape — over ssh or a loaded terminal an
arrow can land a few milliseconds later, and reading only what is already buffered turns it into
`Esc`, `[`, `A`. So the reader waits `options.EscapeTimeout` (25 ms) for the continuation. That wait
is also what a lone `Esc` costs before it is delivered, which is the trade every terminal editor
makes; shorten it on a local terminal, lengthen it on a slow link.

## Paste and copy

Bracketed paste is on by default (`options.BracketedPaste`). The terminal wraps pasted text in
markers, `TerminalInputReader` reads the whole block, and it arrives as one edit rather than a burst
of key presses — so a pasted token cannot trip a shortcut or fire validation halfway through.

Where it lands follows what typing would do: a text or number field takes it at the caret, dropping
characters the field would refuse anyway; a choice modal extends its filter; with no modal open it
goes to the view through `IArlecchinoView.HandlePaste`, which does nothing unless the view overrides it. Only
the first line reaches a single-line field.

```csharp
public ViewRoute HandlePaste(string text)
{
    _query.Value += text;
    return ViewRoute.None;
}
```

A binding can carry a second combination for actions the platforms disagree about — that is what
`AlsoKey` and `AlsoModifiers` on `KeyBinding` are for, and why `Copy` answers to both `Ctrl+Insert` and
`Ctrl+Shift+C`. `ToString()` shows the first one, so hints stay short.

`Ctrl+Insert` and `Ctrl+Shift+C` (the `Copy` binding) copy the field being edited, or the whole text of
the [multi-line dialog](modals-and-state.md). It goes through `IArlecchinoTerminal`, which
encodes it as an OSC 52 sequence — that reaches the clipboard of the machine the user is sitting at
even over SSH. `Ctrl+C` is deliberately left alone: it is how the application is stopped. Terminals
never acknowledge a copy, and many have the feature switched off, so there is nothing to report back.

## Replacing the terminal

`IArlecchinoTerminal` is the whole surface between Arlecchino and the console — size, key availability, `ReadKey`,
`Write`, entering or leaving the alternate screen, the mouse, bracketed paste and the clipboard.
`SystemTerminal` is the default; swap it with `.UseTerminal<T>()` to drive a test harness or a remote
session. See [Hosting and options](hosting-and-options.md).
