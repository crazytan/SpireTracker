using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Saves;
using SpireTracker.UI;

namespace SpireTracker.Patches;

/// <summary>
/// Patches the "Choose a Relic" selection screen (boss/treasure relic picks)
/// to show "NEW" badges on relics that haven't been discovered yet.
///
/// Target: NChooseARelicSelection._Ready() — called when the selection
/// screen is initialized and relic holders are populated.
///
/// The screen has a _relicRow container with NTreasureRoomRelicHolder children,
/// each displaying a RelicModel. We iterate them in Postfix and badge any
/// whose relic hasn't been discovered.
/// </summary>
[HarmonyPatch(typeof(NChooseARelicSelection), "_Ready")]
public class ChooseRelicPatch
{
    static void Postfix(NChooseARelicSelection __instance)
    {
        try
        {
            // The _relicRow contains the relic choice holders
            var relicRow = Traverse.Create(__instance).Field("_relicRow").GetValue<Control>();
            if (relicRow == null) return;

            foreach (var child in relicRow.GetChildren())
            {
                if (child is not Control relicHolder) continue;

                // Try to find a RelicModel on the holder via reflection
                var relicField = Traverse.Create(relicHolder).Field("_relic");
                if (!relicField.FieldExists()) continue;

                var relicModel = relicField.GetValue<RelicModel>();
                if (relicModel == null) continue;

                if (RelicTracker.IsNewRelic(relicModel))
                {
                    NewBadge.AttachTo(relicHolder);
                }
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"ChooseRelicPatch.Postfix: {ex.Message}");
        }
    }
}
