---
title: Commands
sidebar_label: Commands
description: IArlecchinoCommand and ViewCommand, the command palette, the conflict check, and the registry a view can drive itself.
---

# Commands

A key a view or an application reacts to belongs in a command, not in a `switch`. That is what makes
it visible to the palette, to the hints box, to the [keys screen](keyboard.md#the-keys-screen) and to
the conflict check — and what lets it be relabelled and rebound without touching the screen.

## An application command

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

:::note[A plain letter does not fire globally]

A binding that carries a modifier — `new(ConsoleKey.S, ConsoleModifiers.Control)` — fires before the
key reaches the view. A plain letter does not: it would swallow typing. It stays reachable through the
palette and through whatever the view does with it.

:::

## Commands of a view

```csharp
private Mod? _selected;

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

| Member | Meaning |
|---|---|
| `Binding` | The key |
| `Label` | A delegate, so the text follows the current [language](localization.md) |
| `IsEnabled` | Optional; a disabled command is greyed in the palette |
| `Run` | Returns a route, so a command can navigate |
| `ViewCommand.For(key, label, action)` | Wraps an `Action` for a command that stays put |
| `ViewCommand.Navigating(key, label, route)` | Wraps a command whose whole job is to navigate |

A disabled command **swallows** its key rather than letting it fall through — the key is spoken for
either way, and a screen does not change behaviour depending on whether a row happens to be selected.

`Hints()` is optional for a view with commands: when it returns nothing, the hints box is built from
the command list, so a rebound key relabels itself there too.

The box also offers the palette itself — `: → commands` with the default keymap — whenever at least
one command is registered, which is the same condition under which the key does anything at all. The
key shown is `CommandPaletteKey`, so rebinding it relabels the line, and the wording is
`ArlecchinoStrings.HintCommands`. A view with no hints of its own gets a box with that one line.

## Conflicts are reported

A view command shadows an application command on the same key. That is by design, and it is said out
loud: when the route is first shown, `CommandConflicts` logs a warning naming both the view command
and the application command it hides, and another one if the view binds the same key twice.

That is exactly the case that used to hide silently — a `Pick a folder` command and a password field
both on `p`.

## The command palette

Pressing `:` opens a modal listing the commands of the current view first, then the application
commands, as `key  label`. The next key either runs the matching command or, if nothing matches,
closes the palette and writes `unknown command: <key>` to the output line. A click runs the command on
that row. `Esc` and `Enter` close it silently.

Change the key with `options.CommandPaletteKey = '/'`. The palette does not open while no command is
registered, which leaves the key free for views to handle.

## Driving the registry yourself

`CommandRegistry` is a service, for a view that wants to list or invoke commands itself:

| Member | Meaning |
|---|---|
| `Commands` | The registered set, in registration order |
| `TryFind(key, out command)` | Looks one up |
| `Send(key)` | Executes the match and returns its route |

That is what a custom launcher screen or a toolbar drawn from commands is built on.
