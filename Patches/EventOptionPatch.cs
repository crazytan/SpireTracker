using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Events;
using SpireTracker.UI;

namespace SpireTracker.Patches;

/// <summary>
/// Patches the event option button to show "NEW" badges on relics
/// offered by ancient events (Neow's blessings and similar).
///
/// Target: NEventOptionButton._Ready() — called when the option button
/// is initialized. For AncientEventModel options with a relic, the game
/// renders a RelicIcon TextureRect. We attach our badge to that icon.
/// </summary>
[HarmonyPatch(typeof(NEventOptionButton), "_Ready")]
public class EventOptionPatch
{
    static void Postfix(NEventOptionButton __instance)
    {
        try
        {
            var eventModel = __instance.Event;
            var option = __instance.Option;

            // Only ancient events (Neow, etc.) display relic icons
            if (eventModel is not AncientEventModel) return;
            if (option?.Relic == null) return;

            SpireTracker.Logger.Info($"EventOptionPatch: Found relic option {option.Relic.Id}");

            if (RelicTracker.IsNewRelic(option.Relic.Id))
            {
                SpireTracker.Logger.Info($"EventOptionPatch: Relic {option.Relic.Id} is NEW!");

                // The relic icon is at "%RelicIcon" — same node the game uses
                var relicIcon = __instance.GetNodeOrNull<Control>("%RelicIcon");
                if (relicIcon != null)
                {
                    NewBadge.AttachTo(relicIcon);
                    SpireTracker.Logger.Info("EventOptionPatch: Badge attached to RelicIcon");
                }
                else
                {
                    // Fallback: attach to the button itself
                    NewBadge.AttachTo(__instance);
                    SpireTracker.Logger.Info("EventOptionPatch: Badge attached to button (fallback)");
                }
            }
        }
        catch (Exception ex)
        {
            SpireTracker.Logger.Error($"EventOptionPatch.Postfix: {ex.Message}");
        }
    }
}
