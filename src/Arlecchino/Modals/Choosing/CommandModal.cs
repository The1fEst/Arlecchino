using System;
using System.Collections.Generic;
using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Modals.Choosing;

/// <summary>
/// The list of what can be pressed right now. It is a reminder rather than a menu, since the keys keep
/// working while it is open and whoever opened it says what a press does.
/// </summary>
public sealed class CommandModal : Modal
{
    /// <summary>The key and label of every command available in this context.</summary>
    public IReadOnlyList<(string Key, string Label)> Commands { get; init; } = [];

    /// <summary>Where the rows were drawn last frame, used to turn a click into a command.</summary>
    public SurfaceRegion Rows { get; set; }

    /// <summary>What a key press means. Set by whoever opened the palette.</summary>
    public Action<KeyPress>? OnKey { get; init; }

    /// <summary>What a click on a row means. Set by whoever opened the palette.</summary>
    public Action<int>? OnRow { get; init; }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame)
    {
        frame.Lists.Commands(this);
    }

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key) => OnKey?.Invoke(key);

    /// <inheritdoc/>
    public override void HandleMouse(ModalFrame frame, MouseEvent mouse)
    {
        if (mouse.Action != MouseAction.Pressed ||
            mouse.Button != MouseButton.Left ||
            !Rows.Contains(mouse.Row, mouse.Column))
        {
            return;
        }

        var (row, _) = Rows.ToLocal(mouse.Row, mouse.Column);

        OnRow?.Invoke(row);
    }
}
