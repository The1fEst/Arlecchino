---
title: "ColorModal"
sidebar_label: "ColorModal"
---

# ColorModal class

**Namespace:** `Arlecchino.Modals.Setting` &middot; **Assembly:** `Arlecchino`

A color picked on three sliders: hue, saturation and lightness, converted to [`Rgb`](../arlecchino.rendering.colors/Rgb.md) on the way out. Both directions round to whole units, so feeding a color back in can shift it by one.

```csharp
public sealed class ColorModal : Modal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)

## Constructors

| Member | Summary |
|---|---|
| [`ColorModal()`](#colormodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Channel`](#channel) | Which of the three sliders the arrows move. |
| [`ChannelMaximum`](#channelmaximum) | Upper end of the slider the arrows move. |
| [`ChannelRows`](#channelrows) | Where each slider's row was drawn last frame, used to turn a click into a channel. |
| [`ChannelTracks`](#channeltracks) | Where each slider's track was drawn last frame, used to turn a click into a value. |
| [`ChannelValue`](#channelvalue) | Value of the slider the arrows move. |
| [`Hue`](#hue) | Position on the color wheel, from `0` to `359`. It wraps rather than stopping. |
| [`LargeStep`](#largestep) | How far the page keys move the active slider. |
| [`Lightness`](#lightness) | Distance from black toward white, in percent. Fifty is the pure color. |
| [`OnPicked`](#onpicked) | Called with the color that was confirmed. |
| [`Saturation`](#saturation) | Distance from gray, in percent. |
| [`Step`](#step) | How far the arrow keys move the active slider. |
| [`Value`](#value) | The three sliders resolved into a color, as drawn in the swatch. |

## Methods

| Member | Summary |
|---|---|
| [`Add(int)`](#add-int) | Moves the active slider. Hue wraps around the wheel; the other two halt at their ends. |
| [`Draw(ModalFrame)`](#draw-modalframe) |  |
| [`Handle(ModalFrame, KeyPress)`](#handle-modalframe-keypress) |  |
| [`HandleMouse(ModalFrame, MouseEvent)`](#handlemouse-modalframe-mouseevent) |  |
| [`MaximumOf(ColorChannel)`](#maximumof-colorchannel) | Upper end of a slider, since hue counts degrees and the others count percent. |
| [`MoveChannel(int)`](#movechannel-int) | Moves between the sliders, stopping at the first and the last. |
| [`MoveToMaximum()`](#movetomaximum) | Jumps the active slider to its right end. |
| [`MoveToMinimum()`](#movetominimum) | Jumps the active slider to its left end. |
| [`SetChannelFromFraction(ColorChannel, decimal)`](#setchannelfromfraction-colorchannel-decimal) | Places a slider's handle at a position along its track. |
| [`SetValue(Rgb)`](#setvalue-rgb) | Loads a color into the three sliders, which is how an existing value is edited. |
| [`ValueOf(ColorChannel)`](#valueof-colorchannel) | Value of a slider, for drawing all three at once. |

## Constructors in detail

### `ColorModal()` {#colormodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public ColorModal();
```

## Properties in detail

### `Channel` {#channel}

```csharp
public ColorChannel Channel { get; set; }
```

Which of the three sliders the arrows move.

**Type** [`ColorChannel`](../arlecchino.modals.setting/ColorChannel.md)

### `ChannelMaximum` {#channelmaximum}

```csharp
public int ChannelMaximum { get; }
```

Upper end of the slider the arrows move.

**Type** `int`

### `ChannelRows` {#channelrows}

```csharp
public SurfaceRegion[] ChannelRows { get; set; }
```

Where each slider's row was drawn last frame, used to turn a click into a channel.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)\[\]

### `ChannelTracks` {#channeltracks}

```csharp
public SurfaceRegion[] ChannelTracks { get; set; }
```

Where each slider's track was drawn last frame, used to turn a click into a value.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)\[\]

### `ChannelValue` {#channelvalue}

```csharp
public int ChannelValue { get; }
```

Value of the slider the arrows move.

**Type** `int`

### `Hue` {#hue}

```csharp
public int Hue { get; set; }
```

Position on the color wheel, from `0` to `359`. It wraps rather than stopping.

**Type** `int`

### `LargeStep` {#largestep}

```csharp
public int LargeStep { get; init; }
```

How far the page keys move the active slider.

**Type** `int`

### `Lightness` {#lightness}

```csharp
public int Lightness { get; set; }
```

Distance from black toward white, in percent. Fifty is the pure color.

**Type** `int`

### `OnPicked` {#onpicked}

```csharp
public Action<Rgb> OnPicked { get; init; }
```

Called with the color that was confirmed.

**Type** `Action<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt;

### `Saturation` {#saturation}

```csharp
public int Saturation { get; set; }
```

Distance from gray, in percent.

**Type** `int`

### `Step` {#step}

```csharp
public int Step { get; init; }
```

How far the arrow keys move the active slider.

**Type** `int`

### `Value` {#value}

```csharp
public Rgb Value { get; }
```

The three sliders resolved into a color, as drawn in the swatch.

**Type** [`Rgb`](../arlecchino.rendering.colors/Rgb.md)

## Methods in detail

### `Add(int)` {#add-int}

```csharp
public void Add(int delta);
```

Moves the active slider. Hue wraps around the wheel; the other two halt at their ends.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `int` | How far to move; negative goes left. |

### `Draw(ModalFrame)` {#draw-modalframe}

```csharp
public override void Draw(ModalFrame frame);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |

### `Handle(ModalFrame, KeyPress)` {#handle-modalframe-keypress}

```csharp
public override void Handle(ModalFrame frame, KeyPress key);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) |  |

### `HandleMouse(ModalFrame, MouseEvent)` {#handlemouse-modalframe-mouseevent}

```csharp
public override void HandleMouse(ModalFrame frame, MouseEvent mouse);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`ModalFrame`](../arlecchino.modals/ModalFrame.md) |  |
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) |  |

### `MaximumOf(ColorChannel)` {#maximumof-colorchannel}

```csharp
public static int MaximumOf(ColorChannel channel);
```

Upper end of a slider, since hue counts degrees and the others count percent.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `channel` | [`ColorChannel`](../arlecchino.modals.setting/ColorChannel.md) | The slider to ask about. |

**Returns** `int` — The largest value it accepts.

### `MoveChannel(int)` {#movechannel-int}

```csharp
public void MoveChannel(int delta);
```

Moves between the sliders, stopping at the first and the last.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `delta` | `int` | How far to move; negative goes up. |

### `MoveToMaximum()` {#movetomaximum}

```csharp
public void MoveToMaximum();
```

Jumps the active slider to its right end.

### `MoveToMinimum()` {#movetominimum}

```csharp
public void MoveToMinimum();
```

Jumps the active slider to its left end.

### `SetChannelFromFraction(ColorChannel, decimal)` {#setchannelfromfraction-colorchannel-decimal}

```csharp
public void SetChannelFromFraction(ColorChannel channel, decimal fraction);
```

Places a slider's handle at a position along its track.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `channel` | [`ColorChannel`](../arlecchino.modals.setting/ColorChannel.md) | The slider to move. |
| `fraction` | `decimal` | Position from `0` at the left end to `1` at the right; anything outside is pulled in. |

### `SetValue(Rgb)` {#setvalue-rgb}

```csharp
public void SetValue(Rgb color);
```

Loads a color into the three sliders, which is how an existing value is edited.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `color` | [`Rgb`](../arlecchino.rendering.colors/Rgb.md) | The color to start from. |

### `ValueOf(ColorChannel)` {#valueof-colorchannel}

```csharp
public int ValueOf(ColorChannel channel);
```

Value of a slider, for drawing all three at once.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `channel` | [`ColorChannel`](../arlecchino.modals.setting/ColorChannel.md) | The slider to read. |

**Returns** `int` — Its current value.

