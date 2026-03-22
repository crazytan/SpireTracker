using Godot;

namespace SpireTracker.UI;

/// <summary>
/// Creates and manages "NEW" badge labels that are attached as children
/// of relic UI nodes to indicate relics the player hasn't picked up yet.
/// Uses a Panel with ColorRect background + Label for maximum visibility.
/// </summary>
public static class NewBadge
{
    private const string BadgeNodeName = "SpireTracker_NewBadge";

    /// <summary>
    /// Attaches a "NEW" badge to the given control node.
    /// Uses a PanelContainer with a colored background and label for robustness.
    /// </summary>
    public static void AttachTo(Control parent)
    {
        if (parent == null) return;

        // Prevent duplicate badges
        if (parent.HasNode(BadgeNodeName)) return;

        // Background panel
        var panel = new PanelContainer();
        panel.Name = BadgeNodeName;

        // Create a dark background using StyleBoxFlat
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        styleBox.CornerRadiusBottomLeft = 3;
        styleBox.CornerRadiusBottomRight = 3;
        styleBox.CornerRadiusTopLeft = 3;
        styleBox.CornerRadiusTopRight = 3;
        styleBox.ContentMarginLeft = 4;
        styleBox.ContentMarginRight = 4;
        styleBox.ContentMarginTop = 1;
        styleBox.ContentMarginBottom = 1;
        panel.AddThemeStyleboxOverride("panel", styleBox);

        // Label inside the panel
        var label = new Label();
        label.Text = "NEW";
        label.AddThemeColorOverride("font_color", new Color(1.0f, 0.84f, 0.0f)); // Gold
        label.AddThemeColorOverride("font_outline_color", new Color(0.0f, 0.0f, 0.0f));
        label.AddThemeFontSizeOverride("font_size", 12);
        label.AddThemeConstantOverride("outline_size", 2);
        label.HorizontalAlignment = HorizontalAlignment.Center;

        panel.AddChild(label);

        // Position at top-right of parent
        panel.Position = new Vector2(-8, -8);
        panel.ZIndex = 10;

        parent.AddChild(panel);

        SpireTracker.Logger.Info($"NewBadge attached to {parent.Name}");
    }

    /// <summary>
    /// Removes the "NEW" badge from a control node, if present.
    /// </summary>
    public static void RemoveFrom(Control parent)
    {
        if (parent == null) return;

        var existing = parent.GetNodeOrNull(BadgeNodeName);
        if (existing != null)
        {
            parent.RemoveChild(existing);
            existing.QueueFree();
        }
    }
}
