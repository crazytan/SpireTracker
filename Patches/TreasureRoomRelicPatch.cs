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
/// In singleplayer, SingleplayerRelicHolder is used (1 relic).
/// In multiplayer, _multiplayerHolders are used (2-4 relics).
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
            var holdersInUse = Traverse.Create(__instance)
                .Field("_holdersInUse")
                .GetValue<System.Collections.Generic.List<NTreasureRoomRelicHolder>>();
            if (holdersInUse == null) return;

            foreach (var holder in holdersInUse)
            {
                if (holder?.Relic?.Model == null) continue;

                if (RelicTracker.IsNewRelic(holder.Relic.Model))
                {
                    NewBadge.AttachTo(holder);
                }
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"TreasureRoomRelicPatch.Postfix: {ex.Message}");
        }
    }
}
