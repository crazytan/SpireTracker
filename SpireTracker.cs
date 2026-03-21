using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;

namespace SpireTracker;

/// <summary>
/// Mod entry point. Uses the game's [ModInitializer] attribute to register
/// an initialization method that Harmony-patches relic display code.
/// </summary>
[ModInitializer(nameof(Initialize))]
public class SpireTracker
{
    public const string ModId = "SpireTracker";
    public const string HarmonyId = "com.spiretracker";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        Logger.Info("SpireTracker initializing...");

        var harmony = new Harmony(HarmonyId);
        int succeeded = 0;
        int failed = 0;

        Type[] patchClasses =
        {
            typeof(Patches.RelicRewardPatch),
            typeof(Patches.ChooseRelicPatch),
            typeof(Patches.MerchantRelicPatch),
        };

        foreach (var patchClass in patchClasses)
        {
            try
            {
                harmony.CreateClassProcessor(patchClass).Patch();
                Logger.Info($"  Patched: {patchClass.Name}");
                succeeded++;
            }
            catch (Exception ex)
            {
                Logger.Error($"  Failed to patch {patchClass.Name}: {ex.Message}");
                failed++;
            }
        }

        Logger.Info($"SpireTracker loaded! Patches: {succeeded} succeeded, {failed} failed");
    }
}
