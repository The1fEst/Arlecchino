using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Atoms;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// One node of a tree. Children are settable so a branch can be filled in when it is first opened
/// rather than up front.
/// </summary>
/// <typeparam name="T">What the node holds.</typeparam>
public sealed class TreeNode<T>
{
    /// <summary>What this node stands for.</summary>
    public required T Value { get; init; }

    /// <summary>What sits under it.</summary>
    public IReadOnlyList<TreeNode<T>> Children { get; set; } = [];

    /// <summary>Whether the children are showing.</summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Whether the node has children right now. A branch that has not been filled in yet reads as a
    /// leaf, so fill it in before it is first drawn.
    /// </summary>
    public bool HasChildren => Children.Count > 0;
}

/// <summary>
/// A hierarchy drawn as indented rows. Only the expanded parts are laid out, and that layout is
/// recomputed on demand rather than cached, so nodes may be added or expanded between frames.
/// </summary>
/// <typeparam name="T">What each node holds.</typeparam>
public sealed class Tree<T> : IArlecchinoInteractiveWidget
{
    private const string ExpandedMarker = "▾ ";
    private const string CollapsedMarker = "▸ ";
    private const string LeafMarker = "  ";
    private const int IndentWidth = 2;

    private readonly ArlecchinoKeymap _keymap;
    private readonly List<(TreeNode<T> Node, int Depth)> _visible = [];

    private SurfaceRegion _drawn;
    private ScrollWindow _window;

    /// <summary>Creates the tree.</summary>
    /// <param name="keymap">Keys to obey, so the tree follows the application's bindings.</param>
    public Tree(ArlecchinoKeymap keymap)
    {
        _keymap = keymap;
    }

    /// <summary>Turns a value into its label. The marker and the indent are added around it.</summary>
    public required Func<T, string> Render { get; init; }

    /// <summary>Colours a node. Ignored for the selected one.</summary>
    public Func<T, IArlecchinoColor>? ItemStyle { get; set; }

    /// <summary>
    /// What confirming a leaf does. Branches toggle instead, so this is never called for a node that
    /// has children.
    /// </summary>
    public Func<TreeNode<T>, ViewRoute>? OnActivate { get; init; }

    /// <summary>
    /// Called just before a branch opens, which is where its children can be filled in. It runs on the
    /// UI thread, so anything slow belongs in an <see cref="AsyncAtom{TLoaded}"/> instead.
    /// </summary>
    public Action<TreeNode<T>>? OnExpanding { get; init; }

    /// <summary>The top-level nodes.</summary>
    public IReadOnlyList<TreeNode<T>> Roots { get; set; } = [];

    /// <summary>Index of the selected row, counted over the rows that are showing rather than over all nodes.</summary>
    public int Selected { get; set; }

    /// <summary>Whether the tree has focus, which decides how strongly the selection is drawn.</summary>
    public bool IsFocused { get; set; }

    /// <summary>The selected node, or <c>null</c> when the tree is empty.</summary>
    public TreeNode<T>? SelectedNode
    {
        get
        {
            Flatten();
            return _visible.Count == 0 ? null : _visible[Math.Clamp(Selected, 0, _visible.Count - 1)].Node;
        }
    }

    /// <summary>
    /// Opens every branch. Branches are opened directly, so anything relying on the expand callback to
    /// fill in its children will still look empty.
    /// </summary>
    public void ExpandAll() => SetExpanded(Roots, true);

    /// <summary>Closes every branch, leaving only the roots showing.</summary>
    public void CollapseAll() => SetExpanded(Roots, false);

    /// <inheritdoc />
    [Obsolete(
        "Draw is replaced by Place, which draws the same thing and returns the region left over. " +
        "Draw is removed in 2.0, where Place takes its name.",
        DiagnosticId = ArlecchinoDiagnostics.ObsoleteDraw)]
    public void Draw(SurfaceRegion region) => Place(region);

    /// <summary>
    /// Draws the rows that are showing around the selection and remembers where they landed, which is
    /// what lets a click tell a marker from a label. The tree fills whatever it is given, so nothing is
    /// left underneath it.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region: the tree uses every row it is handed.</returns>
    public SurfaceRegion Place(SurfaceRegion region)
    {
        _drawn = region;

        if (region.IsEmpty)
        {
            return region;
        }

        Flatten();
        Selected = Math.Clamp(Selected, 0, Math.Max(0, _visible.Count - 1));
        _window = ScrollWindow.Around(Selected, _visible.Count, region.Height);

        var barred = ScrollBar.IsNeeded(_visible.Count, region.Height);
        var textWidth = barred ? Math.Max(0, region.Width - 1) : region.Width;

        for (var row = 0; row < _window.Count; row++)
        {
            var index = _window.First + row;
            var (node, depth) = _visible[index];
            var marker = node.HasChildren ? node.IsExpanded ? ExpandedMarker : CollapsedMarker : LeafMarker;
            var line = new string(' ', depth * IndentWidth) + marker + Render(node.Value);

            region.Write(row, 0, TextWidth.PadRight(TextWidth.Truncate(line, textWidth), textWidth),
                StyleOf(node, index));
        }

        if (barred)
        {
            ScrollBar.Draw(region, _window.First, _visible.Count);
        }

        return region.Rows(region.Height, 0);
    }

    /// <summary>
    /// Moves through the rows and opens or closes branches. The horizontal arrows behave the way they
    /// do in a file manager: right opens a closed branch or steps into it, left closes an open one or
    /// jumps to the parent.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>What became of the key, including a route when a leaf was activated.</returns>
    public FocusResult Handle(ConsoleKeyInfo key)
    {
        Flatten();

        if (_keymap.MoveUp.Matches(key))
        {
            Move(-1);
        }
        else if (_keymap.MoveDown.Matches(key))
        {
            Move(1);
        }
        else if (_keymap.First.Matches(key))
        {
            Selected = 0;
        }
        else if (_keymap.Last.Matches(key))
        {
            Selected = Math.Max(0, _visible.Count - 1);
        }
        else if (_keymap.MoveRight.Matches(key))
        {
            Expand();
        }
        else if (_keymap.MoveLeft.Matches(key))
        {
            Collapse();
        }
        else if (_keymap.Confirm.Matches(key))
        {
            return Activate();
        }
        else
        {
            return FocusResult.Ignored;
        }

        return FocusResult.Handled;
    }

    /// <summary>
    /// Scrolls with the wheel and selects with a click. Clicking the marker toggles the branch, while
    /// clicking the label of the already selected row activates it.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>What became of the event, including a route when a leaf was activated.</returns>
    public FocusResult HandleMouse(MouseEvent mouse)
    {
        if (_drawn.IsEmpty || !_drawn.Contains(mouse.Row, mouse.Column))
        {
            return FocusResult.Ignored;
        }

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                Move(-1);
                return FocusResult.Handled;
            case MouseAction.ScrolledDown:
                Move(1);
                return FocusResult.Handled;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left:
                return Click(mouse);
            default:
                return FocusResult.Ignored;
        }
    }

    private FocusResult Click(MouseEvent mouse)
    {
        Flatten();

        var (row, column) = _drawn.ToLocal(mouse.Row, mouse.Column);
        var index = _window.First + row;

        if (index < 0 || index >= _visible.Count)
        {
            return FocusResult.Handled;
        }

        var wasSelected = index == Selected;
        Selected = index;

        var (node, depth) = _visible[index];
        var onMarker = column >= depth * IndentWidth && column < depth * IndentWidth + IndentWidth;

        if (!node.HasChildren || !onMarker)
        {
            return wasSelected ? Activate() : FocusResult.Handled;
        }

        Toggle(node);
        return FocusResult.Handled;
    }

    private FocusResult Activate()
    {
        if (SelectedNode is not { } node)
        {
            return FocusResult.Handled;
        }

        if (!node.HasChildren)
        {
            return OnActivate is null ? FocusResult.Handled : FocusResult.Navigate(OnActivate(node));
        }

        Toggle(node);
        return FocusResult.Handled;
    }

    private void Expand()
    {
        if (SelectedNode is not { HasChildren: true, IsExpanded: false } node)
        {
            Move(1);
            return;
        }

        Toggle(node);
    }

    private void Collapse()
    {
        if (SelectedNode is { HasChildren: true, IsExpanded: true } node)
        {
            node.IsExpanded = false;
            return;
        }

        MoveToParent();
    }

    private void MoveToParent()
    {
        Flatten();

        if (_visible.Count == 0)
        {
            return;
        }

        var depth = _visible[Math.Clamp(Selected, 0, _visible.Count - 1)].Depth;

        for (var index = Selected - 1; index >= 0; index--)
        {
            if (_visible[index].Depth >= depth)
            {
                continue;
            }

            Selected = index;
            return;
        }
    }

    private void Toggle(TreeNode<T> node)
    {
        if (!node.IsExpanded)
        {
            OnExpanding?.Invoke(node);
        }

        node.IsExpanded = !node.IsExpanded;
    }

    private void Move(int delta)
    {
        Flatten();
        Selected = Math.Clamp(Selected + delta, 0, Math.Max(0, _visible.Count - 1));
    }

    private void Flatten()
    {
        _visible.Clear();
        Collect(Roots, 0);
    }

    private void Collect(IReadOnlyList<TreeNode<T>> nodes, int depth)
    {
        foreach (var node in nodes)
        {
            _visible.Add((node, depth));

            if (node.IsExpanded)
            {
                Collect(node.Children, depth + 1);
            }
        }
    }

    private static void SetExpanded(IReadOnlyList<TreeNode<T>> nodes, bool expanded)
    {
        foreach (var node in nodes)
        {
            node.IsExpanded = expanded;
            SetExpanded(node.Children, expanded);
        }
    }

    private IArlecchinoColor StyleOf(TreeNode<T> node, int index)
    {
        if (index != Selected)
        {
            return ItemStyle?.Invoke(node.Value) ?? Theme.Default;
        }

        return IsFocused ? Theme.ActiveSelected : Theme.Selected;
    }
}
