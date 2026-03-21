using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using SpireTracker.UI;

namespace SpireTracker.Patches;

/// <summary>
/// Patches the merchant/shop relic display to show "NEW" badges
/// on relics that haven't been discovered yet.
///
/// Target: NMerchantRelic.UpdateVisual() — called when the shop relic
/// slot updates its visual representation.
///
/// NMerchantRelic extends NMerchantSlot, which has an Entry property
/// of type MerchantEntry. For relics, this is MerchantRelicEntry,
/// which has a .Model property of type RelicModel.
/// </summary>
[HarmonyPatch(typeof(NMerchantRelic), "UpdateVisual")]
public class MerchantRelicPatch
{
    static void Postfix(NMerchantRelic __instance)
    {
        try
        {
            // NMerchantSlot has an Entry property; for relics it's MerchantRelicEntry
            var entry = Traverse.Create(__instance).Property("Entry").GetValue<MerchantEntry>();
            if (entry is not MerchantRelicEntry relicEntry) return;

            var relicModel = relicEntry.Model;
            if (relicModel == null) return;

            if (RelicTracker.IsNewRelic(relicModel.Id))
            {
                // Attach badge to the relic holder visual
                var relicHolder = Traverse.Create(__instance).Field("_relicHolder").GetValue<Control>();
                if (relicHolder != null)
                {
                    NewBadge.AttachTo(relicHolder);
                }
                else
                {
                    // Fallback: attach to the instance itself
                    NewBadge.AttachTo(__instance);
                }
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"MerchantRelicPatch.Postfix: {ex.Message}");
        }
    }
}
