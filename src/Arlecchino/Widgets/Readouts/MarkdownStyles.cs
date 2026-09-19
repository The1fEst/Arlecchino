using Arlecchino.Rendering.Colors;

namespace Arlecchino.Widgets.Readouts;

/// <summary>
/// What each part of a document is written in. Every one of them is <c>null</c> to begin with, which
/// leaves it to <see cref="Theme"/>; what is worth setting is what a palette has no role for.
/// </summary>
public sealed class MarkdownStyles
{
    /// <summary>The first two levels of heading, <see cref="Theme.Header"/> when left alone.</summary>
    public IArlecchinoColor? Heading { get; init; }

    /// <summary>The four below them, <see cref="Theme.Accent"/> when left alone.</summary>
    public IArlecchinoColor? Subheading { get; init; }

    /// <summary>What the document put between two stars, the ordinary text in bold when left alone.</summary>
    public IArlecchinoColor? Strong { get; init; }

    /// <summary>What it put between one, the ordinary text in italic when left alone.</summary>
    public IArlecchinoColor? Emphasis { get; init; }

    /// <summary>
    /// Code, both a span of it and a block, <see cref="Theme.Info"/> when left alone. A background of its
    /// own is worth giving this one: a block is painted across the pane with it.
    /// </summary>
    public IArlecchinoColor? Code { get; init; }

    /// <summary>
    /// Bullets, the numbers of a list and the bar down the side of a quote,
    /// <see cref="Theme.Active"/> when left alone.
    /// </summary>
    public IArlecchinoColor? Marker { get; init; }

    /// <summary>What a quote says, <see cref="Theme.Secondary"/> when left alone.</summary>
    public IArlecchinoColor? Quote { get; init; }

    /// <summary>What a link is written as, the ordinary text underlined when left alone.</summary>
    public IArlecchinoColor? Link { get; init; }

    /// <summary>Where it goes, <see cref="Theme.Secondary"/> when left alone.</summary>
    public IArlecchinoColor? Url { get; init; }

    /// <summary>Rules and the lines between the columns of a table, <see cref="Theme.Info"/> when left alone.</summary>
    public IArlecchinoColor? Rule { get; init; }

    /// <summary>The first row of a table, <see cref="Theme.TableHeader"/> when left alone.</summary>
    public IArlecchinoColor? TableHeader { get; init; }
}
