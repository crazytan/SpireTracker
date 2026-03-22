using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Rewards;
using SpireTracker.UI;

namespace SpireTracker.Patches;

/// <summary>
/// Patches the relic reward button to show a "NEW" badge when the relic
/// has never been picked up before.
///
/// Target: NRewardButton.Reload() — called when a reward button is set up
/// with its reward data and rendered on screen.
///
/// We use a Prefix to check DiscoveredRelics BEFORE the game updates the set,
/// then a Postfix to attach the badge after the UI is built.
/// </summary>
[HarmonyPatch(typeof(NRewardButton), "Reload")]
public class RelicRewardPatch
{
    // Thread-local storage to pass "is new" state from Prefix to Postfix
    [ThreadStatic]
    private static bool _isNewRelic;

    [ThreadStatic]
    private static bool _isRelicReward;

    /// <summary>
    /// Before Reload: check if this is a relic reward and if the relic is new.
    /// Must happen before the game updates DiscoveredRelics.
    /// </summary>
    static void Prefix(NRewardButton __instance)
    {
        try
        {
            _isNewRelic = false;
            _isRelicReward = false;

            if (__instance.Reward is not RelicReward relicReward) return;
            if (!relicReward.IsPopulated) return;

            _isRelicReward = true;

            // Access the private _relic field via Harmony's Traverse
            var relicModel = Traverse.Create(relicReward).Field("_relic").GetValue<RelicModel>();
            if (relicModel == null) return;

            _isNewRelic = RelicTracker.IsNewRelic(relicModel);
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"RelicRewardPatch.Prefix: {ex.Message}");
        }
    }

    /// <summary>
    /// After Reload: if we determined the relic is new, attach a badge.
    /// The icon container is the "%Icon" child of NRewardButton.
    /// </summary>
    static void Postfix(NRewardButton __instance)
    {
        try
        {
            if (!_isRelicReward || !_isNewRelic) return;

            // NRewardButton has a private _iconContainer field (Control)
            var iconContainer = Traverse.Create(__instance).Field("_iconContainer").GetValue<Control>();
            if (iconContainer == null) return;

            NewBadge.AttachTo(iconContainer);
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"RelicRewardPatch.Postfix: {ex.Message}");
        }
    }
}
