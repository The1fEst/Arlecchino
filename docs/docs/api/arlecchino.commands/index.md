---
title: Arlecchino.Commands
sidebar_label: Arlecchino.Commands
sidebar_position: 0
---

# Arlecchino.Commands

## Classes

| Type | Summary |
|---|---|
| [`CommandRegistry`](CommandRegistry.md) | The application commands registered with `AddCommand`. Take it in a view to list or run them yourself — the sample draws its menu straight from this. |
| [`ViewCommand`](ViewCommand.md) | A key a screen reacts to, declared as data rather than hidden in a switch. That is what lets the palette list it, the hints box label it, and the conflict check see it. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoCommand`](IArlecchinoCommand.md) | An application-wide command. It appears in the command palette from every screen, and fires globally when its binding carries a modifier — a plain letter would swallow typing, so those are only reachable through the palette. |

