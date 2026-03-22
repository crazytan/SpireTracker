using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;

namespace SpireTracker;

/// <summary>
/// Queries the game's progress state to determine whether a relic
/// has been picked up before in any prior run.
///
/// The game tracks "discovered" relics in ProgressState.DiscoveredRelics.
/// Despite the name, this set tracks relics the player has actually picked up,
/// matching the in-game Compendium behavior (Unknown = never picked).
///
/// Our logic: if a relic is NOT in DiscoveredRelics, it's new — the player
/// has never picked it up. We check this BEFORE the game updates the set,
/// using a Harmony Prefix.
/// </summary>
public static class RelicTracker
{
    /// <summary>
    /// Returns true if this relic has NOT been picked up before in any prior run.
    /// Must be called before the game updates DiscoveredRelics to get accurate results.
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
    /// Returns true if this relic has NOT been picked up before.
    /// Overload accepting RelicModel directly.
    /// </summary>
    public static bool IsNewRelic(RelicModel relic)
    {
        return IsNewRelic(relic.Id);
    }
}
