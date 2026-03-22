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
/// Target: NChooseARelicSelection._Ready() — creates NRelicBasicHolder
/// children in _relicRow, each containing an NRelic with a RelicModel.
/// </summary>
[HarmonyPatch(typeof(NChooseARelicSelection), "_Ready")]
public class ChooseRelicPatch
{
    static void Postfix(NChooseARelicSelection __instance)
    {
        try
        {
            SpireTracker.Logger.Info("ChooseRelicPatch.Postfix fired");

            var relicRow = Traverse.Create(__instance).Field("_relicRow").GetValue<Control>();
            if (relicRow == null)
            {
                SpireTracker.Logger.Info("ChooseRelicPatch: _relicRow is null");
                return;
            }

            SpireTracker.Logger.Info(
                $"ChooseRelicPatch: _relicRow has {relicRow.GetChildCount()} children");

            foreach (var child in relicRow.GetChildren())
            {
                if (child is not NRelicBasicHolder holder) continue;

                var relicModel = holder.Relic?.Model;
                if (relicModel == null) continue;

                SpireTracker.Logger.Info(
                    $"ChooseRelicPatch: relic={relicModel.Id}, isNew={RelicTracker.IsNewRelic(relicModel)}");

                if (RelicTracker.IsNewRelic(relicModel))
                {
                    NewBadge.AttachTo(holder);
                }
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"ChooseRelicPatch.Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
