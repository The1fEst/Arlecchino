using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Rendering.Terminals;
using Arlecchino.Sample.Views;
using Arlecchino.Widgets.Pictures;

namespace Arlecchino.Sample;

public sealed class PicturesView : IArlecchinoView
{
    private const string Resource = "Arlecchino.Sample.card.png";

    private readonly Surface _surface;
    private readonly Picture _picture = new() { Background = Theme.Default };

    public PicturesView(Surface surface)
    {
        _surface = surface;

        var (pixels, width, height) = Card();

        _picture.Show(pixels, width, height);
    }

    public void Draw()
    {
        var (left, right) = _surface.Content.SplitLeft(_surface.Content.Width - 26);

        _picture.Draw(left.Border(Theme.Info, $"{_picture.PixelWidth}×{_picture.PixelHeight} pixels"));

        var notes = right.Border(Theme.Info, "how").Flow();
        var asked = Protocol();
        var drawn = TerminalCapabilities.Resolve(asked);

        notes.AppendLine(asked == drawn ? $"drawn as {drawn}" : $"{asked} → {drawn}", Theme.Accent);
        notes.SkipLine();
        notes.AppendLine("The terminal says:", Theme.Muted);
        notes.AppendLine($"  sixel {Said(TerminalCapabilities.Sixel)}", Theme.Muted);
        notes.AppendLine($"  kitty {Said(TerminalCapabilities.Kitty)}", Theme.Muted);
        notes.AppendLine(
            $"  cell {Glyphs.CellWidth}x{Glyphs.CellHeight} " +
            (TerminalCapabilities.CellSizeKnown ? "reported" : "guessed"),
            Theme.Muted);

        notes.AppendLine(
            $"  behind {TerminalCapabilities.Background?.Hex ?? "unknown"}",
            Theme.Muted);
        notes.SkipLine();
        notes.AppendLine("p walks the four.", Theme.Muted);
        notes.SkipLine();
        notes.AppendLine("Blocks work in every", Theme.Muted);
        notes.AppendLine("terminal: two pixels", Theme.Muted);
        notes.AppendLine("to a cell, upper half", Theme.Muted);
        notes.AppendLine("block over background.", Theme.Muted);
        notes.SkipLine();
        notes.AppendLine("Sixel: Windows Terminal,", Theme.Muted);
        notes.AppendLine("xterm, foot, WezTerm.", Theme.Muted);
        notes.AppendLine("256 colors taken from", Theme.Muted);
        notes.AppendLine("the picture. Measured", Theme.Muted);
        notes.AppendLine("in pixels, not cells.", Theme.Muted);
        notes.SkipLine();
        notes.AppendLine("Kitty: kitty, WezTerm,", Theme.Muted);
        notes.AppendLine("Ghostty. Full color.", Theme.Muted);
        notes.SkipLine();
        notes.AppendLine("A terminal that cannot", Theme.Muted);
        notes.AppendLine("speak one shows the", Theme.Muted);
        notes.AppendLine("escape as text.", Theme.Muted);
    }

    public ViewRoute Handle(KeyPress key)
    {
        if (key.Key == ConsoleKey.Escape)
        {
            return ViewKind.Default;
        }

        if (char.ToLowerInvariant(key.Character) != 'p')
        {
            return ViewRoute.None;
        }

        _picture.Protocol = Protocol() switch
        {
            ImageProtocol.Auto => ImageProtocol.Blocks,
            ImageProtocol.Blocks => ImageProtocol.Sixel,
            ImageProtocol.Sixel => ImageProtocol.Kitty,
            _ => ImageProtocol.Auto,
        };

        _surface.ForgetPreviousFrame();

        return ViewRoute.None;
    }

    public ViewRoute HandleMouse(MouseEvent mouse) => ViewRoute.None;

    public (string Key, string Description)[] Hints() => [("p", "protocol"), ("Esc", "back")];

    private static string Said(bool yes) => yes ? "yes" : "no";

    private ImageProtocol Protocol() => _picture.Protocol ?? Glyphs.Picture;

    private static (Rgb[] Pixels, int Width, int Height) Card()
    {
        using var stream = typeof(PicturesView).Assembly.GetManifestResourceStream(Resource) ?? throw new InvalidOperationException($"{Resource} was not built into the sample");

        return Png(stream);
    }

    private static (Rgb[] Pixels, int Width, int Height) Png(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        using var deflated = new MemoryStream();

        reader.ReadBytes(8);

        var width = 0;
        var height = 0;
        var channels = 0;

        while (true)
        {
            var length = BigEndian(reader.ReadBytes(4));
            var kind = Encoding.ASCII.GetString(reader.ReadBytes(4));
            var chunk = reader.ReadBytes(length);

            reader.ReadBytes(4);

            if (kind == "IEND")
            {
                break;
            }

            if (kind == "IDAT")
            {
                deflated.Write(chunk);
                continue;
            }

            if (kind != "IHDR")
            {
                continue;
            }

            width = BigEndian(chunk.AsSpan(0, 4));
            height = BigEndian(chunk.AsSpan(4, 4));
            channels = chunk[9] == 6 ? 4 : 3;

            if (chunk[8] != 8 || (chunk[9] != 2 && chunk[9] != 6) || chunk[12] != 0)
            {
                throw new InvalidOperationException(
                    "the sample reads eight-bit RGB or RGBA PNGs that are not interlaced");
            }
        }

        deflated.Position = 0;

        return Unfilter(deflated, width, height, channels);
    }

    private static (Rgb[] Pixels, int Width, int Height) Unfilter(
        Stream deflated,
        int width,
        int height,
        int channels)
    {
        using var inflate = new ZLibStream(deflated, CompressionMode.Decompress);

        var stride = width * channels;
        var raw = new byte[(stride + 1) * height];
        var read = 0;

        while (read < raw.Length)
        {
            var got = inflate.Read(raw, read, raw.Length - read);

            if (got == 0)
            {
                throw new InvalidOperationException("the PNG ended before its pixels did");
            }

            read += got;
        }

        var pixels = new Rgb[width * height];
        var line = new byte[stride];
        var above = new byte[stride];

        for (var row = 0; row < height; row++)
        {
            var filter = raw[row * (stride + 1)];

            Array.Copy(raw, (row * (stride + 1)) + 1, line, 0, stride);

            for (var at = 0; at < stride; at++)
            {
                var left = at >= channels ? line[at - channels] : 0;
                var corner = at >= channels ? above[at - channels] : 0;

                line[at] += filter switch
                {
                    1 => (byte)left,
                    2 => above[at],
                    3 => (byte)((left + above[at]) / 2),
                    4 => Paeth(left, above[at], corner),
                    _ => 0,
                };
            }

            for (var column = 0; column < width; column++)
            {
                pixels[(row * width) + column] = new(
                    line[column * channels],
                    line[(column * channels) + 1],
                    line[(column * channels) + 2]);
            }

            Array.Copy(line, above, stride);
        }

        return (pixels, width, height);
    }

    private static byte Paeth(int left, int above, int corner)
    {
        var guess = left + above - corner;
        var toLeft = Math.Abs(guess - left);
        var toAbove = Math.Abs(guess - above);
        var toCorner = Math.Abs(guess - corner);

        return (byte)(toLeft <= toAbove && toLeft <= toCorner ? left : toAbove <= toCorner ? above : corner);
    }

    private static int BigEndian(ReadOnlySpan<byte> bytes) =>
        (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
}
