using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;

namespace SpireTracker;

/// <summary>
/// Queries the game's progress state to determine whether a relic
/// has been seen/discovered before in any prior run.
///
/// The game tracks "discovered" relics in ProgressState.DiscoveredRelics.
/// A relic is added to this set when it appears on screen (reward, shop, etc.)
/// via SaveManager.Instance.MarkRelicAsSeen().
///
/// Our logic: if a relic is NOT yet in DiscoveredRelics at the time it's
/// being displayed, it's truly new — the player has never encountered it.
/// We check this BEFORE the game marks it as seen, using a Harmony Prefix.
///
/// Note: "discovered" != "picked/obtained". A relic can be discovered (seen)
/// but never actually picked. Phase 2 could scan run history for wasPicked
/// to distinguish "seen but never taken" vs "never seen at all".
/// </summary>
public static class RelicTracker
{
    /// <summary>
    /// Returns true if this relic has NOT been seen before in any prior run.
    /// Must be called before the game's MarkRelicAsSeen() to get accurate results.
    /// </summary>
    public static bool IsNewRelic(ModelId relicId)
    {
        try
        {
            var progress = SaveManager.Instance?.Progress;
            if (progress == null) return false;

            return !progress.DiscoveredRelics.Contains(relicId);
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"Error checking relic {relicId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Returns true if this relic has NOT been seen before.
    /// Overload accepting RelicModel directly.
    /// </summary>
    public static bool IsNewRelic(RelicModel relic)
    {
        return IsNewRelic(relic.Id);
    }
}
