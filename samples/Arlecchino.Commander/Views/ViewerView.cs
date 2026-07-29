using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Arlecchino.Commander.Files;
using Arlecchino.Commander.Model;
using Arlecchino.Commander.Stores;
using Arlecchino.Commands;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Widgets;
using Microsoft.Extensions.Hosting;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Commander.Views;

public sealed class ViewerView : IArlecchinoView
{
    private const int ReadLimit = 512 * 1024;
    private const int ChunkBytes = 8192;
    private const int BytesPerRow = 16;
    private const int TextProbe = 8192;

    private readonly Surface _surface;
    private readonly PaneTree _layout;
    private readonly FocusRing _focus;
    private readonly IHostApplicationLifetime _lifetime;

    public ViewerView(Surface surface, Panels panels, ArlecchinoOptions options, IHostApplicationLifetime lifetime)
    {
        _surface = surface;
        _lifetime = lifetime;

        var path = panels.Viewing.Value;
        var text = new TextView(options.Keymap);
        var (body, kind, size) = Load(panels.ViewingSource, path, panels.ViewingSize);

        text.Text = body;

        var status = new StatusBar
        {
            Left = [() => $"{Sizes.Grouped(size)} bytes · {kind}"],
            Right = [static () => "Esc back", static () => "↑↓ PgUp PgDn scroll"],
        };

        _layout = Branch(
            Rows,
            PaneSize.CellsFromEnd(1),
            Leaf(text, () => Path.GetFileName(path)),
            Leaf(status));

        _focus = _layout.AsFocusRing(options.Keymap);
    }

    public void Draw() => _layout.Draw(_surface.Content);

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Escape, static () => "back", static () => ViewKind.Commander),
        ViewCommand.Navigating(ConsoleKey.F3, static () => "back", static () => ViewKind.Commander),
        ViewCommand.For(ConsoleKey.F10, static () => "quit", _lifetime.StopApplication),
    ];

    private static (string Text, string Kind, long Size) Load(IFileSource source, string path, long size)
    {
        try
        {
            var bytes = Head(source, path);

            return IsBinary(bytes)
                ? (Dump(bytes), Truncated("hex", bytes.Length, size), size)
                : (Encoding.UTF8.GetString(bytes).Replace("\t", "    ", StringComparison.Ordinal),
                    Truncated("text", bytes.Length, size), size);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return (error.Message, "unreadable", 0);
        }
    }

    private static string Truncated(string kind, int read, long size) =>
        read < size ? $"{kind}, first {Sizes.Short(read)}" : kind;

    private static byte[] Head(IFileSource source, string path)
    {
        using var stream = source.OpenRead(path);
        var held = new MemoryStream();
        var chunk = new byte[ChunkBytes];

        while (held.Length < ReadLimit)
        {
            var read = stream.Read(chunk, 0, (int)Math.Min(chunk.Length, ReadLimit - held.Length));

            if (read <= 0)
            {
                break;
            }

            held.Write(chunk, 0, read);
        }

        return held.ToArray();
    }

    private static bool IsBinary(byte[] bytes)
    {
        var probed = Math.Min(TextProbe, bytes.Length);

        for (var index = 0; index < probed; index++)
        {
            if (bytes[index] == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string Dump(byte[] bytes)
    {
        var text = new StringBuilder();

        for (var offset = 0; offset < bytes.Length; offset += BytesPerRow)
        {
            var row = bytes.AsSpan(offset, Math.Min(BytesPerRow, bytes.Length - offset));

            text.Append(offset.ToString("x8", CultureInfo.InvariantCulture)).Append("  ");

            for (var index = 0; index < BytesPerRow; index++)
            {
                text.Append(index < row.Length
                        ? row[index].ToString("x2", CultureInfo.InvariantCulture)
                        : "  ")
                    .Append(' ');
            }

            text.Append(' ');

            foreach (var value in row)
            {
                text.Append(value is >= 32 and < 127 ? (char)value : '.');
            }

            text.AppendLine();
        }

        return text.ToString();
    }
}
