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
/// Target: NRewardButton.Reload() — called when a reward button is set up.
///
/// The reward screen uses TextureRect icons (via RelicReward.CreateIcon()),
/// NOT NRelic nodes, so this needs its own patch separate from NRelicPatch.
///
/// We use a Prefix to check DiscoveredRelics BEFORE the game might update it,
/// then a Postfix to attach the badge after the UI is built.
/// </summary>
[HarmonyPatch(typeof(NRewardButton), "Reload")]
public class RelicRewardPatch
{
    [ThreadStatic]
    private static bool _isNewRelic;

    [ThreadStatic]
    private static bool _isRelicReward;

    static void Prefix(NRewardButton __instance)
    {
        try
        {
            _isNewRelic = false;
            _isRelicReward = false;

            if (__instance.Reward is not RelicReward relicReward) return;
            if (!relicReward.IsPopulated) return;

            _isRelicReward = true;

            var relicModel = Traverse.Create(relicReward).Field("_relic").GetValue<RelicModel>();
            if (relicModel == null)
            {
                SpireTracker.Logger.Info("RelicRewardPatch: _relic field is null");
                return;
            }

            _isNewRelic = RelicTracker.IsNewRelic(relicModel);
            SpireTracker.Logger.Info(
                $"RelicRewardPatch.Prefix: relic={relicModel.Id}, isNew={_isNewRelic}");
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"RelicRewardPatch.Prefix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    static void Postfix(NRewardButton __instance)
    {
        try
        {
            if (!_isRelicReward)
            {
                return;
            }

            if (!_isNewRelic) return;

            // NRewardButton._iconContainer is the Control holding the relic icon
            var iconContainer = Traverse.Create(__instance)
                .Field("_iconContainer").GetValue<Control>();

            if (iconContainer == null)
            {
                SpireTracker.Logger.Info("RelicRewardPatch: _iconContainer is null, "
                    + "trying __instance as fallback");
                NewBadge.AttachTo(__instance);
                return;
            }

            NewBadge.AttachTo(iconContainer);
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"RelicRewardPatch.Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
