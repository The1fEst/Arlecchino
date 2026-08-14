---
title: "Handover"
sidebar_label: "Handover"
---

# Handover class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Lends the terminal to a full-screen program of its own — an editor, a pager, a shell — and takes it back afterward. It runs on the drawing thread and blocks it, so no frame lands on top of the other program.

```csharp
public sealed class Handover
```

## Properties

| Member | Summary |
|---|---|
| [`IsAway`](#isaway) | Whether another program has the terminal at this moment. |

## Methods

| Member | Summary |
|---|---|
| [`Give(Action)`](#give-action) | Hands the terminal over for the length of a call and takes it back however that call ends, error included. What is taken back is the terminal that was in force to begin with. |
| [`Run(ProcessStartInfo)`](#run-processstartinfo) | Runs a program with the terminal to itself and waits for it to end. None of its three streams is redirected, so what it writes and what is typed into it go straight to the terminal. |

## Properties in detail

### `IsAway` {#isaway}

```csharp
public bool IsAway { get; }
```

Whether another program has the terminal at this moment.

**Type** `bool`

## Methods in detail

### `Give(Action)` {#give-action}

```csharp
public void Give(Action work);
```

Hands the terminal over for the length of a call and takes it back however that call ends, error included. What is taken back is the terminal that was in force to begin with.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `work` | `Action` | What to do while the terminal belongs to the other program. |

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

### `Run(ProcessStartInfo)` {#run-processstartinfo}

```csharp
public int Run(ProcessStartInfo start);
```

Runs a program with the terminal to itself and waits for it to end. None of its three streams is redirected, so what it writes and what is typed into it go straight to the terminal.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `start` | `ProcessStartInfo` | The program and its arguments. |

**Returns** `int` — What it exited with.

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Nothing could be started from what was asked for. |

