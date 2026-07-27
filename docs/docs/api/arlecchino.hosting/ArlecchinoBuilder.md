---
title: ArlecchinoBuilder
sidebar_label: ArlecchinoBuilder
---

# ArlecchinoBuilder class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Configures an application while its services are being registered. Every method returns the builder, so a whole application is described in one chain at startup.

```csharp
public sealed class ArlecchinoBuilder
```

## Properties

| Member | Summary |
|---|---|
| [`Options`](#options) | The settings gathered so far, for anything the builder has no method for. |
| [`Services`](#services) | The service collection being built, for registering whatever the views depend on. |

## Methods

| Member | Summary |
|---|---|
| [`AddCommand<TCommand>()`](#addcommand-tcommand) | Registers a command available everywhere. Its key has to carry a modifier, since plain letters belong to whatever is being typed. |
| [`AddStartup<TStartup>()`](#addstartup-tstartup) | Registers work to run once the container is ready but before the first frame, for loading what the opening view expects to find. |
| [`AddStore<TStore>()`](#addstore-tstore) | Registers one store, resolved by its own type: a singleton, or scoped to the screen when it implements [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md). An alternative to `AddGeneratedStores()` for a store the generator cannot see — one from another assembly — rather than a layer on top of it. |
| [`AddView<T>(string)`](#addview-t-string) | Registers a view at a route, built from the container so it can take whatever it needs in its constructor. Views are created on demand rather than at startup. |
| [`AddView(string, Func<IServiceProvider, IArlecchinoView>)`](#addview-string-func-iserviceprovider-iarlecchinoview) | Registers a view built by hand, for the cases the container cannot cover on its own, such as a view that needs a value known only at startup. |
| [`AddViewFactory<TFactory>()`](#addviewfactory-tfactory) | Registers a source of views that decides at run time which routes it serves. This is what the generated factory is registered through, and how a plugin adds views the host never listed. |
| [`AddWidget<TWidget>()`](#addwidget-twidget) | Registers one widget as a singleton, resolved by its own type. An alternative to `AddGeneratedWidgets()` for a widget the generator cannot see — one from another assembly — rather than a layer on top of it; registering the same type both ways puts it in the container twice. A singleton widget is shared by every screen that resolves it, state and focus included, so it suits a panel the application has one of. A widget each screen needs its own copy of is built in the view. |
| [`StartAt(ViewRoute)`](#startat-viewroute) | Sets the view the application opens on. |
| [`StartAt(string)`](#startat-string) | Sets the view the application opens on, by name. |
| [`UseKeymap(ArlecchinoKeymap)`](#usekeymap-arlecchinokeymap) | Replaces the key bindings, which every widget then follows. |
| [`UseLatinOnlyInput()`](#uselatinonlyinput) | Accepts only Latin letters and digits, in exchange for keys that always read correctly. |
| [`UseMouse()`](#usemouse) | Turns the mouse on. It stays off by default because the terminal then stops handling selection itself, and copying text with the mouse no longer works the way the user expects. Windows reads the console's event queue for this, which also means quick-edit selection is off while it runs. |
| [`UseNativeInput()`](#usenativeinput) | Accepts whatever the terminal reports, so any language can be typed. |
| [`UseNotifications(Nullable<ConsoleKeyInfo>, Nullable<TimeSpan>, Nullable<TimeSpan>)`](#usenotifications-nullable-consolekeyinfo-nullable-timespan-nullable-timespan) | Turns the output row on and says how long a message lives. The row shows the newest notification until `timeout` is up; the message stays readable on the notifications screen — the `Notifications` key, or a click on the row — until `lifetime` is up. |
| [`UseStrings(ArlecchinoStrings)`](#usestrings-arlecchinostrings) | Replaces the wording the framework itself shows. This is the only way it is localised: nothing is looked up from resources. |
| [`UseTerminal<TTerminal>()`](#useterminal-tterminal) | Draws to something other than the console, replacing whatever terminal was registered. This is how tests capture frames instead of writing them. |
| [`UseTextInput(TextInputMode)`](#usetextinput-textinputmode) | Chooses how typed characters are read. This is a trade-off rather than a preference: reading the terminal's own characters accepts any language but can misread keys on some terminals. |
| [`UseTheme(ThemePalette)`](#usetheme-themepalette) | Replaces the colours. What actually reaches the screen still depends on what the terminal supports. |
| [`WithoutHostedService()`](#withouthostedservice) | Stops the application from taking over the terminal when the host starts. Everything stays registered, so a test can drive the loop itself frame by frame. |
| [`WithoutNotifications()`](#withoutnotifications) | Leaves the output row off, so nothing the application says is drawn on the frame. |

## Properties in detail

### `Options` {#options}

```csharp
public ArlecchinoOptions Options { get; }
```

The settings gathered so far, for anything the builder has no method for.

**Type** [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md)

### `Services` {#services}

```csharp
public IServiceCollection Services { get; }
```

The service collection being built, for registering whatever the views depend on.

**Type** `IServiceCollection`

## Methods in detail

### `AddCommand<TCommand>()` {#addcommand-tcommand}

```csharp
public ArlecchinoBuilder AddCommand<TCommand>();
```

Registers a command available everywhere. Its key has to carry a modifier, since plain letters belong to whatever is being typed.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `AddStartup<TStartup>()` {#addstartup-tstartup}

```csharp
public ArlecchinoBuilder AddStartup<TStartup>();
```

Registers work to run once the container is ready but before the first frame, for loading what the opening view expects to find.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `AddStore<TStore>()` {#addstore-tstore}

```csharp
public ArlecchinoBuilder AddStore<TStore>();
```

Registers one store, resolved by its own type: a singleton, or scoped to the screen when it implements [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md). An alternative to `AddGeneratedStores()` for a store the generator cannot see — one from another assembly — rather than a layer on top of it.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `AddView<T>(string)` {#addview-t-string}

```csharp
public ArlecchinoBuilder AddView<T>(string route);
```

Registers a view at a route, built from the container so it can take whatever it needs in its constructor. Views are created on demand rather than at startup.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `route` | `string` | The route it answers to. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `AddView(string, Func<IServiceProvider, IArlecchinoView>)` {#addview-string-func-iserviceprovider-iarlecchinoview}

```csharp
public ArlecchinoBuilder AddView(string route, Func<IServiceProvider, IArlecchinoView> factory);
```

Registers a view built by hand, for the cases the container cannot cover on its own, such as a view that needs a value known only at startup.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `route` | `string` | The route it answers to. |
| `factory` | `Func<T, TResult>`&lt;`IServiceProvider`, [`IArlecchinoView`](../arlecchino.navigation/IArlecchinoView.md)&gt; | Builds the view. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `AddViewFactory<TFactory>()` {#addviewfactory-tfactory}

```csharp
public ArlecchinoBuilder AddViewFactory<TFactory>();
```

Registers a source of views that decides at run time which routes it serves. This is what the generated factory is registered through, and how a plugin adds views the host never listed.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `AddWidget<TWidget>()` {#addwidget-twidget}

```csharp
public ArlecchinoBuilder AddWidget<TWidget>();
```

Registers one widget as a singleton, resolved by its own type. An alternative to `AddGeneratedWidgets()` for a widget the generator cannot see — one from another assembly — rather than a layer on top of it; registering the same type both ways puts it in the container twice. A singleton widget is shared by every screen that resolves it, state and focus included, so it suits a panel the application has one of. A widget each screen needs its own copy of is built in the view.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `StartAt(ViewRoute)` {#startat-viewroute}

```csharp
public ArlecchinoBuilder StartAt(ViewRoute route);
```

Sets the view the application opens on.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `route` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The opening route. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `StartAt(string)` {#startat-string}

```csharp
public ArlecchinoBuilder StartAt(string route);
```

Sets the view the application opens on, by name.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `route` | `string` | The opening route. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseKeymap(ArlecchinoKeymap)` {#usekeymap-arlecchinokeymap}

```csharp
public ArlecchinoBuilder UseKeymap(ArlecchinoKeymap keymap);
```

Replaces the key bindings, which every widget then follows.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | The bindings to use. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseLatinOnlyInput()` {#uselatinonlyinput}

```csharp
public ArlecchinoBuilder UseLatinOnlyInput();
```

Accepts only Latin letters and digits, in exchange for keys that always read correctly.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseMouse()` {#usemouse}

```csharp
public ArlecchinoBuilder UseMouse();
```

Turns the mouse on. It stays off by default because the terminal then stops handling selection itself, and copying text with the mouse no longer works the way the user expects. Windows reads the console's event queue for this, which also means quick-edit selection is off while it runs.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseNativeInput()` {#usenativeinput}

```csharp
public ArlecchinoBuilder UseNativeInput();
```

Accepts whatever the terminal reports, so any language can be typed.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseNotifications(Nullable<ConsoleKeyInfo>, Nullable<TimeSpan>, Nullable<TimeSpan>)` {#usenotifications-nullable-consolekeyinfo-nullable-timespan-nullable-timespan}

```csharp
public ArlecchinoBuilder UseNotifications(
    Nullable<ConsoleKeyInfo> key,
    Nullable<TimeSpan> timeout,
    Nullable<TimeSpan> lifetime);
```

Turns the output row on and says how long a message lives. The row shows the newest notification until `timeout` is up; the message stays readable on the notifications screen — the `Notifications` key, or a click on the row — until `lifetime` is up.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `Nullable<T>`&lt;`ConsoleKeyInfo`&gt; | Key that opens the notifications screen, modifiers and all. Omit it for `Ctrl+N`. |
| `timeout` | `Nullable<T>`&lt;`TimeSpan`&gt; | How long a message holds the output row; omit to keep the default. |
| `lifetime` | `Nullable<T>`&lt;`TimeSpan`&gt; | How long it stays in the list; omit to keep the default. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseStrings(ArlecchinoStrings)` {#usestrings-arlecchinostrings}

```csharp
public ArlecchinoBuilder UseStrings(ArlecchinoStrings strings);
```

Replaces the wording the framework itself shows. This is the only way it is localised: nothing is looked up from resources.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `strings` | [`ArlecchinoStrings`](../arlecchino.hosting/ArlecchinoStrings.md) | The wording to use. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseTerminal<TTerminal>()` {#useterminal-tterminal}

```csharp
public ArlecchinoBuilder UseTerminal<TTerminal>();
```

Draws to something other than the console, replacing whatever terminal was registered. This is how tests capture frames instead of writing them.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseTextInput(TextInputMode)` {#usetextinput-textinputmode}

```csharp
public ArlecchinoBuilder UseTextInput(TextInputMode mode);
```

Chooses how typed characters are read. This is a trade-off rather than a preference: reading the terminal's own characters accepts any language but can misread keys on some terminals.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mode` | [`TextInputMode`](../arlecchino.input/TextInputMode.md) | The mode to use. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `UseTheme(ThemePalette)` {#usetheme-themepalette}

```csharp
public ArlecchinoBuilder UseTheme(ThemePalette palette);
```

Replaces the colours. What actually reaches the screen still depends on what the terminal supports.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `palette` | [`ThemePalette`](../arlecchino.rendering/ThemePalette.md) | The colours to use. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `WithoutHostedService()` {#withouthostedservice}

```csharp
public ArlecchinoBuilder WithoutHostedService();
```

Stops the application from taking over the terminal when the host starts. Everything stays registered, so a test can drive the loop itself frame by frame.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

### `WithoutNotifications()` {#withoutnotifications}

```csharp
public ArlecchinoBuilder WithoutNotifications();
```

Leaves the output row off, so nothing the application says is drawn on the frame.

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder.

