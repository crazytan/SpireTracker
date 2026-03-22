using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using SpireTracker.UI;

namespace SpireTracker.Patches;

/// <summary>
/// Patches the merchant/shop relic display to show "NEW" badges
/// on relics that haven't been picked up yet.
///
/// Target: NMerchantRelic.UpdateVisual() — called when the shop relic
/// slot updates its visual representation.
///
/// NMerchantRelic has a private _relic field (RelicModel) set in FillSlot()
/// before UpdateVisual() is called. We read it directly via Traverse.
/// </summary>
[HarmonyPatch(typeof(NMerchantRelic), "UpdateVisual")]
public class MerchantRelicPatch
{
    static void Postfix(NMerchantRelic __instance)
    {
        try
        {
            var relicModel = Traverse.Create(__instance).Field("_relic").GetValue<RelicModel>();
            if (relicModel == null) return;

            if (RelicTracker.IsNewRelic(relicModel))
            {
                var relicHolder = Traverse.Create(__instance).Field("_relicHolder").GetValue<Control>();
                NewBadge.AttachTo(relicHolder ?? (Control)__instance);
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"MerchantRelicPatch.Postfix: {ex.Message}");
        }
    }
}
