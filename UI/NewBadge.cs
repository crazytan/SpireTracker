using Godot;

namespace SpireTracker.UI;

/// <summary>
/// Creates and manages "NEW" badge labels that are attached as children
/// of relic UI nodes to indicate relics the player hasn't picked up yet.
/// </summary>
public static class NewBadge
{
    private const string BadgeNodeName = "SpireTracker_NewBadge";

    /// <summary>
    /// Attaches a "NEW" label to the given control node (typically a relic icon container).
    /// If a badge already exists on this node, it does nothing (prevents duplicates).
    /// </summary>
    public static void AttachTo(Control parent)
    {
        if (parent == null) return;

        // Prevent duplicate badges
        if (parent.HasNode(BadgeNodeName)) return;

        var label = new Label();
        label.Name = BadgeNodeName;
        label.Text = "NEW";

        // Style the label
        label.AddThemeColorOverride("font_color", new Color(1.0f, 0.84f, 0.0f)); // Gold
        label.AddThemeColorOverride("font_outline_color", new Color(0.0f, 0.0f, 0.0f)); // Black outline
        label.AddThemeFontSizeOverride("font_size", 14);
        label.AddThemeConstantOverride("outline_size", 3);

        // Position at top-right of parent
        label.SetAnchorsPreset(Control.LayoutPreset.TopRight);
        label.GrowHorizontal = Control.GrowDirection.Begin;
        label.Position = new Vector2(-8, -4);

        // Ensure it renders on top
        label.ZIndex = 10;

        parent.AddChild(label);
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
