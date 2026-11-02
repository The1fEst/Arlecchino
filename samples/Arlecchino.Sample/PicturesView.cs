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

    private static readonly LocString[] Notes =
    [
        LocString.BlocksNote,
        LocString.SixelNote,
        LocString.KittyNote,
        LocString.UnsupportedNote,
    ];

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

        _picture.Draw(left.Border(Theme.Info, Loc(LocString.PictureSize, _picture.PixelWidth, _picture.PixelHeight)));

        var notes = right.Border(Theme.Info, Loc(LocString.HowTitle)).Flow();
        var size = Protocol();
        var image = TerminalCapabilities.Resolve(size);

        notes.AppendLine(size == image ? Loc(LocString.DrawnAs, image) : $"{size} → {image}", Theme.Accent);
        notes.SkipLine();
        notes.AppendLine(Loc(LocString.TerminalSays), Theme.Secondary);
        notes.AppendLine(Loc(LocString.SixelSupport, Said(TerminalCapabilities.Sixel)), Theme.Secondary);
        notes.AppendLine(Loc(LocString.KittySupport, Said(TerminalCapabilities.Kitty)), Theme.Secondary);
        notes.AppendLine(
            Loc(LocString.CellSize,
                Glyphs.CellWidth,
                Glyphs.CellHeight,
                Loc(TerminalCapabilities.CellSizeKnown ? LocString.CellSizeReported : LocString.CellSizeGuessed)),
            Theme.Secondary);

        notes.AppendLine(
            Loc(LocString.BehindColor, TerminalCapabilities.Background?.Hex ?? Loc(LocString.UnknownColor)),
            Theme.Secondary);
        notes.SkipLine();
        notes.AppendLine(Loc(LocString.ProtocolNote), Theme.Secondary);
        notes.SkipLine();

        foreach (var note in Notes)
        {
            foreach (var line in TextWidth.Wrap(Loc(note), notes.Region.Width))
            {
                notes.AppendLine(line, Theme.Secondary);
            }

            notes.SkipLine();
        }
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

    public (string Key, string Description)[] Hints() => [("p", Loc(LocString.HintProtocol)), ("Esc", Loc(LocString.Back))];

    private static string Said(bool yes) => Loc(yes ? LocString.AnswerYes : LocString.AnswerNo);

    private ImageProtocol Protocol() => _picture.Protocol ?? Glyphs.Picture;

    private static (RGB[] Pixels, int Width, int Height) Card()
    {
        using var stream = typeof(PicturesView).Assembly.GetManifestResourceStream(Resource) ?? throw new InvalidOperationException($"{Resource} was not built into the sample");

        return Png(stream);
    }

    private static (RGB[] Pixels, int Width, int Height) Png(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        using var pixelData = new MemoryStream();

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
                pixelData.Write(chunk);
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

        pixelData.Position = 0;

        return Unfilter(pixelData, width, height, channels);
    }

    private static (RGB[] Pixels, int Width, int Height) Unfilter(
        Stream stream,
        int width,
        int height,
        int channels)
    {
        using var inflate = new ZLibStream(stream, CompressionMode.Decompress);

        var stride = width * channels;
        var raw = new byte[(stride + 1) * height];
        var at = 0;

        while (at < raw.Length)
        {
            var count = inflate.Read(raw, at, raw.Length - at);

            if (count == 0)
            {
                throw new InvalidOperationException("the PNG ended before its pixels did");
            }

            at += count;
        }

        var pixels = new RGB[width * height];
        var line = new byte[stride];
        var previousRow = new byte[stride];

        for (var row = 0; row < height; row++)
        {
            var filter = raw[row * (stride + 1)];

            Array.Copy(raw, (row * (stride + 1)) + 1, line, 0, stride);

            for (var index = 0; index < stride; index++)
            {
                var left = index >= channels ? line[index - channels] : 0;
                var corner = index >= channels ? previousRow[index - channels] : 0;

                line[index] += filter switch
                {
                    1 => (byte)left,
                    2 => previousRow[index],
                    3 => (byte)((left + previousRow[index]) / 2),
                    4 => Paeth(left, previousRow[index], corner),
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

            Array.Copy(line, previousRow, stride);
        }

        return (pixels, width, height);
    }

    private static byte Paeth(int left, int previousRow, int corner)
    {
        var guess = left + previousRow - corner;
        var toLeft = Math.Abs(guess - left);
        var toPrevious = Math.Abs(guess - previousRow);
        var toCorner = Math.Abs(guess - corner);

        return (byte)(toLeft <= toPrevious && toLeft <= toCorner ? left : toPrevious <= toCorner ? previousRow : corner);
    }

    private static int BigEndian(ReadOnlySpan<byte> bytes) =>
        (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
}
