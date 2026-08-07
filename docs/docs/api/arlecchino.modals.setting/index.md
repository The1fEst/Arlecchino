---
title: Arlecchino.Modals.Setting
sidebar_label: Arlecchino.Modals.Setting
sidebar_position: 0
---

# Arlecchino.Modals.Setting

## Classes

| Type | Summary |
|---|---|
| [`ColorModal`](ColorModal.md) | A color picked on three sliders. Hue, saturation and lightness are edited rather than the raw channels because they are what people reach for; the result is converted to [`Rgb`](../arlecchino.rendering.colors/Rgb.md) on the way out. Both directions round to whole units, so feeding a color back in can shift it by one. |
| [`DateModal`](DateModal.md) | A calendar date, edited as year, month and day. The value is kept inside the bounds and inside the calendar at every step, so a day that the new month does not have is pulled back to its last day rather than rejected. |
| [`SegmentedModal`](SegmentedModal.md) | A value edited as a row of fixed-width number segments, the way dates and times are. Digits are collected per segment and only applied once the segment fills up, so a half-typed segment never produces a nonsensical intermediate value. |
| [`SliderModal`](SliderModal.md) | A value inside a range, adjusted by arrows or by dragging the track. There is nothing to type, so the value is always valid and the dialog never reports an error. |
| [`TimeModal`](TimeModal.md) | A time of day, edited as hours and minutes on a 24-hour clock. Everything wraps: stepping past midnight comes back around instead of stopping. |
| [`ToggleModal`](ToggleModal.md) | A yes-or-no answer, flipped with the arrows or picked by clicking one of the two chips. |

## Interfaces

| Type | Summary |
|---|---|
| [`IBoundedModal`](IBoundedModal.md) | A value that moves in steps between two ends. Shared by the number field and the slider, so the stepping keys work the same in both. |

## Enums

| Type | Summary |
|---|---|
| [`ColorChannel`](ColorChannel.md) | One of the three sliders in the color dialog. |

