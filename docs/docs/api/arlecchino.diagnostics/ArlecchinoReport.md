---
title: "ArlecchinoReport"
sidebar_label: "ArlecchinoReport"
---

# ArlecchinoReport class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

What the application looks like right now, as text for a bug report: the version, the platform, what the terminal can do, and the screen with the modals above it. Resolve it and call [`ArlecchinoReport.Describe`](../arlecchino.diagnostics/ArlecchinoReport.md#describe).

```csharp
public sealed class ArlecchinoReport
```

## Constructors

| Member | Summary |
|---|---|
| [`ArlecchinoReport(IArlecchinoTerminal, Navigator, ArlecchinoState, ArlecchinoOptions, Surface, AtomHistory)`](#arlecchinoreport-iarlecchinoterminal-navigator-arlecchinostate-arlecchinooptions-surface-atomhistory) | Creates the report. Resolved from the container like any other service. |

## Methods

| Member | Summary |
|---|---|
| [`Describe()`](#describe) | Builds the report. Nothing here is a secret: it is versions, sizes, the route names of the screens and the type names of the modals — no field values and no text the user typed. |

## Constructors in detail

### `ArlecchinoReport(IArlecchinoTerminal, Navigator, ArlecchinoState, ArlecchinoOptions, Surface, AtomHistory)` {#arlecchinoreport-iarlecchinoterminal-navigator-arlecchinostate-arlecchinooptions-surface-atomhistory}

```csharp
public ArlecchinoReport(
    IArlecchinoTerminal terminal,
    Navigator navigator,
    ArlecchinoState state,
    ArlecchinoOptions options,
    Surface surface,
    AtomHistory history);
```

Creates the report. Resolved from the container like any other service.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `terminal` | [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md) | The terminal being drawn to. |
| `navigator` | [`Navigator`](../arlecchino.navigation/Navigator.md) | Where the application is. |
| `state` | [`ArlecchinoState`](../arlecchino.state/ArlecchinoState.md) | Modals and the output row. |
| `options` | [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md) | How the application was configured. |
| `surface` | [`Surface`](../arlecchino.rendering/Surface.md) | The frame, for its size. |
| `history` | [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) | The undo stack, for its depth. |

## Methods in detail

### `Describe()` {#describe}

```csharp
public string Describe();
```

Builds the report. Nothing here is a secret: it is versions, sizes, the route names of the screens and the type names of the modals — no field values and no text the user typed.

**Returns** `string` — The report, as lines of `key: value`.

