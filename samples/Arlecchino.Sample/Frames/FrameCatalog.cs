using System;
using System.Collections.Generic;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Setting;
using Arlecchino.Navigation;
using Arlecchino.Sample.Views;

namespace Arlecchino.Sample.Frames;

internal static class FrameCatalog
{
    private static readonly FrameShot Fallback = new RouteShot(ViewKind.Default);

    private static readonly Dictionary<string, FrameShot> Shots = new(StringComparer.OrdinalIgnoreCase)
    {
        ["number"] = new ModalShot(ViewKind.Default,
            static state => state.Modal = new NumberModal
            {
                Title = "Price",
                Text = "12.50",
                Minimum = 0,
                Maximum = 500,
                Step = 5,
                Decimals = 2,
                Prefix = "$ ",
                OnSubmit = static _ => { },
            }),
        ["slider"] = new ModalShot(ViewKind.Default,
            static state => state.Modal = new SliderModal
            {
                Title = "Volume",
                Value = 60,
                Suffix = " %",
                OnSubmit = static _ => { },
            }),
        ["toggle"] = new ModalShot(ViewKind.Default,
            static state => state.RequestToggle("Fullscreen", true, static _ => { })),
        ["multi"] = new ModalShot(ViewKind.Default,
            static state => state.RequestMultiChoice(
                "Columns",
                ["Name", "Date Modified", "Size", "Kind"],
                ["Name", "Size"],
                static _ => { })),
        ["password"] = new ModalShot(ViewKind.Default,
            static state =>
            {
                state.RequestPassword("Passphrase", static _ => { });
                ((TextModal)state.Modal!).Text = "hunter2";
            }),
        ["date"] = new ModalShot(ViewKind.Default,
            static state => state.RequestDate("Release date", new(2026, 7, 25), static _ => { })),
        ["time"] = new ModalShot(ViewKind.Default,
            static state => state.RequestTime("Start at", new(9, 41), static _ => { })),
        ["color"] = new ModalShot(ViewKind.Default,
            static state => state.RequestColor("Accent color", new(63, 169, 245), static _ => { })),
        ["picker"] = new ModalShot(Routes.FilePicker,
            static state => state.FilePicker = new(
                "Pick a folder",
                PickFolder: true,
                Environment.CurrentDirectory,
                ViewKind.Default,
                static _ => { })),
        ["widgets"] = new RouteShot(ViewKind.Widgets),
        ["settings"] = new RouteShot(ViewKind.Settings),
        ["panes"] = new RouteShot(ViewKind.Panes),
        ["charts"] = new RouteShot(ViewKind.Charts),
        ["pictures"] = new RouteShot(ViewKind.Pictures),
        ["about"] = new RouteShot(ViewKind.About),
        ["help"] = new RouteShot(ViewKind.Default, Routes.Help),
    };

    public static FrameShot For(string view) => Shots.GetValueOrDefault(view, Fallback);
}
