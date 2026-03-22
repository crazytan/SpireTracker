using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using SpireTracker.UI;

namespace SpireTracker.Patches;

/// <summary>
/// Patches the treasure room relic selection screen to show "NEW" badges.
///
/// Target: NTreasureRoomRelicCollection.InitializeRelics() — called when
/// the treasure chest is opened and relic holders are populated.
///
/// Each NTreasureRoomRelicHolder has a Relic (NRelic) with .Model (RelicModel).
/// We iterate _holdersInUse which contains whichever set is active.
/// </summary>
[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "InitializeRelics")]
public class TreasureRoomRelicPatch
{
    static void Postfix(NTreasureRoomRelicCollection __instance)
    {
        try
        {
            SpireTracker.Logger.Info("TreasureRoomRelicPatch.Postfix fired");

            var holdersInUse = Traverse.Create(__instance)
                .Field("_holdersInUse")
                .GetValue<System.Collections.Generic.List<NTreasureRoomRelicHolder>>();

            if (holdersInUse == null)
            {
                SpireTracker.Logger.Info("TreasureRoomRelicPatch: _holdersInUse is null");
                return;
            }

            SpireTracker.Logger.Info(
                $"TreasureRoomRelicPatch: {holdersInUse.Count} holders in use");

            foreach (var holder in holdersInUse)
            {
                if (holder?.Relic?.Model == null) continue;

                var model = holder.Relic.Model;
                SpireTracker.Logger.Info(
                    $"TreasureRoomRelicPatch: relic={model.Id}, isNew={RelicTracker.IsNewRelic(model)}");

                if (RelicTracker.IsNewRelic(model))
                {
                    NewBadge.AttachTo(holder);
                }
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error(
                $"TreasureRoomRelicPatch.Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
