using Godot;

namespace SpireTracker.UI;

/// <summary>
/// Creates and manages "NEW" badge labels attached to relic UI nodes.
/// Indicates relics the player hasn't picked up yet.
///
/// Key design decisions:
/// - Uses plain Godot Label (BetterSpire2 confirms this works in STS2)
/// - Finds and caches a game font to ensure text renders (STS2 may not provide
///   a default theme font for dynamically created Labels)
/// - High ZIndex (100) to render above relic icons and outlines
/// - MouseFilter.Ignore so badges don't intercept relic clicks/hovers
/// - Cleans up existing badges before adding new ones (handles node pooling)
/// </summary>
public static class NewBadge
{
    private const string BadgeNodeName = "SpireTracker_NewBadge";
    private static Font? _cachedFont;

    /// <summary>
    /// Attaches a "NEW" badge to the given control node.
    /// Safe to call multiple times — cleans up existing badge first.
    /// </summary>
    public static void AttachTo(Control parent)
    {
        if (parent == null || !GodotObject.IsInstanceValid(parent)) return;

        // Clean up existing badge (important for pooled/reused nodes)
        RemoveFrom(parent);

        var label = new Label
        {
            Name = BadgeNodeName,
            Text = "NEW",
            ZIndex = 100,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        // Style: gold text with black outline
        label.AddThemeColorOverride("font_color", new Color(1.0f, 0.84f, 0.0f));
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);

        // Find and apply a game font if available
        var font = FindGameFont(parent);
        if (font != null)
        {
            label.AddThemeFontOverride("font", font);
        }

        parent.AddChild(label);

        // Position at top-left inside parent bounds (guaranteed visible).
        // Use positive coords to avoid clipping issues.
        label.Position = new Vector2(0, 0);

        SpireTracker.Logger.Info(
            $"NewBadge attached to {parent.GetType().Name}:{parent.Name} "
            + $"(size={parent.Size}, visible={parent.Visible})");
    }

    /// <summary>
    /// Removes the "NEW" badge from a control node, if present.
    /// </summary>
    public static void RemoveFrom(Control parent)
    {
        if (parent == null || !GodotObject.IsInstanceValid(parent)) return;

        var existing = parent.GetNodeOrNull(BadgeNodeName);
        if (existing != null && GodotObject.IsInstanceValid(existing))
        {
            parent.RemoveChild(existing);
            existing.QueueFree();
        }
    }

    /// <summary>
    /// Finds a font from the game's UI by walking up the parent chain
    /// and checking theme fonts. Caches the result for performance.
    /// </summary>
    private static Font? FindGameFont(Control startNode)
    {
        if (_cachedFont != null) return _cachedFont;

        try
        {
            // Walk up the tree to find a node with a theme font
            Node? current = startNode;
            while (current != null)
            {
                if (current is Control control)
                {
                    var font = control.GetThemeFont("font", "Label");
                    if (font != null)
                    {
                        _cachedFont = font;
                        return font;
                    }
                }
                current = current.GetParent();
            }

            // Fallback: try ThemeDB
            var fallback = ThemeDB.Singleton?.FallbackFont;
            if (fallback != null)
            {
                _cachedFont = fallback;
                return fallback;
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"FindGameFont error: {ex.Message}");
        }

        return null;
    }
}
