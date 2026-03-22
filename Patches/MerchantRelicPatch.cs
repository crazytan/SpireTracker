using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
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
/// NMerchantRelic fields (from decompilation):
///   - _relicEntry (MerchantRelicEntry) — has .Model (RelicModel)
///   - _relicNode (NRelic?) — the NRelic created in UpdateVisual()
///   - _relicHolder (Control) — container node for the relic visual
/// </summary>
[HarmonyPatch(typeof(NMerchantRelic), "UpdateVisual")]
public class MerchantRelicPatch
{
    static void Postfix(NMerchantRelic __instance)
    {
        try
        {
            SpireTracker.Logger.Info("MerchantRelicPatch.Postfix fired");

            RelicModel? relicModel = null;

            // Strategy 1: Get model from _relicNode (NRelic created in UpdateVisual)
            try
            {
                var relicNode = Traverse.Create(__instance)
                    .Field("_relicNode").GetValue<NRelic>();
                relicModel = relicNode?.Model;
                if (relicModel != null)
                    SpireTracker.Logger.Info(
                        $"MerchantRelicPatch: got model from _relicNode: {relicModel.Id}");
            }
            catch (Exception ex)
            {
                SpireTracker.Logger.Info($"MerchantRelicPatch: _relicNode access failed: {ex.Message}");
            }

            // Strategy 2: Get model from _relicEntry.Model
            if (relicModel == null)
            {
                try
                {
                    var entry = Traverse.Create(__instance).Field("_relicEntry").GetValue();
                    if (entry != null)
                    {
                        relicModel = Traverse.Create(entry)
                            .Property("Model").GetValue<RelicModel>();
                        if (relicModel != null)
                            SpireTracker.Logger.Info(
                                $"MerchantRelicPatch: got model from _relicEntry.Model: {relicModel.Id}");
                    }
                }
                catch (Exception ex)
                {
                    SpireTracker.Logger.Info(
                        $"MerchantRelicPatch: _relicEntry access failed: {ex.Message}");
                }
            }

            // Strategy 3: Legacy field name _relic (in case RECON.md was correct)
            if (relicModel == null)
            {
                try
                {
                    relicModel = Traverse.Create(__instance)
                        .Field("_relic").GetValue<RelicModel>();
                    if (relicModel != null)
                        SpireTracker.Logger.Info(
                            $"MerchantRelicPatch: got model from _relic: {relicModel.Id}");
                }
                catch (Exception ex)
                {
                    SpireTracker.Logger.Info(
                        $"MerchantRelicPatch: _relic access failed: {ex.Message}");
                }
            }

            if (relicModel == null)
            {
                SpireTracker.Logger.Info("MerchantRelicPatch: could not find relic model on "
                    + $"{__instance.GetType().Name}");
                // Log all fields for diagnostic purposes
                LogFields(__instance);
                return;
            }

            if (!RelicTracker.IsNewRelic(relicModel))
            {
                SpireTracker.Logger.Info(
                    $"MerchantRelicPatch: {relicModel.Id} is NOT new, skipping");
                return;
            }

            // Attach badge to _relicHolder, or fall back to the instance itself
            var relicHolder = Traverse.Create(__instance)
                .Field("_relicHolder").GetValue<Control>();
            NewBadge.AttachTo(relicHolder ?? (Control)__instance);
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error(
                $"MerchantRelicPatch.Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Logs all declared fields on the instance for diagnostic purposes.
    /// Only called when we can't find the relic model.
    /// </summary>
    private static void LogFields(object instance)
    {
        try
        {
            var fields = AccessTools.GetDeclaredFields(instance.GetType());
            foreach (var field in fields)
            {
                var val = field.GetValue(instance);
                SpireTracker.Logger.Info(
                    $"  Field: {field.FieldType.Name} {field.Name} = {val}");
            }
        }
        catch { }
    }
}
