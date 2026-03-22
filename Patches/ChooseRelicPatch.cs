using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens;
using SpireTracker.UI;

namespace SpireTracker.Patches;

/// <summary>
/// Patches the "Choose a Relic" overlay screen (boss relic picks, events)
/// to show "NEW" badges on relics that haven't been picked up yet.
///
/// Target: NChooseARelicSelection._Ready() — the _relicRow container holds
/// NRelicBasicHolder children, each with a Relic (NRelic) whose .Model is
/// the RelicModel.
/// </summary>
[HarmonyPatch(typeof(NChooseARelicSelection), "_Ready")]
public class ChooseRelicPatch
{
    static void Postfix(NChooseARelicSelection __instance)
    {
        try
        {
            var relicRow = Traverse.Create(__instance).Field("_relicRow").GetValue<Control>();
            if (relicRow == null) return;

            foreach (var child in relicRow.GetChildren())
            {
                if (child is not NRelicBasicHolder holder) continue;

                // NRelicBasicHolder.Relic is NRelic; .Model is the RelicModel
                var relicModel = holder.Relic?.Model;
                if (relicModel == null) continue;

                if (RelicTracker.IsNewRelic(relicModel))
                {
                    NewBadge.AttachTo(holder);
                }
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"ChooseRelicPatch.Postfix: {ex.Message}");
        }
    }
}
