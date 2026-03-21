# SpireTracker — Decompilation Findings

Decompiled `sts2.dll` (v0.99.1, build 7ac1f450, 2026-03-13) using ilspycmd 9.1.0.

## Key Classes and APIs

### Progress Tracking

- **`MegaCrit.Sts2.Core.Saves.ProgressState`** — Holds all cross-run progress data
  - `IReadOnlySet<ModelId> DiscoveredRelics` — Set of relic IDs the player has ever seen
  - `bool MarkRelicAsSeen(ModelId relicId)` — Adds a relic to the discovered set; returns true if newly added
  - Same pattern for cards (`DiscoveredCards`), potions (`DiscoveredPotions`), events, acts

- **`MegaCrit.Sts2.Core.Saves.SaveManager`** — Singleton (`SaveManager.Instance`)
  - `ProgressState Progress` — The current progress state (delegates to `ProgressSaveManager`)
  - `void MarkRelicAsSeen(RelicModel relic)` — Marks relic as discovered AND adds to player's run-local DiscoveredRelics list

- **`MegaCrit.Sts2.Core.Saves.Managers.ProgressSaveManager`** — Manages progress save file
  - `ProgressState Progress` — Actual progress data
  - `void MarkRelicAsSeen(RelicModel relic)` — Called by SaveManager, delegates to ProgressState

### Important: "Discovered" vs "Picked"

- **Discovered** = player has seen the relic (appeared on screen). Stored in `ProgressState.DiscoveredRelics`.
- **Picked/Obtained** = player actually selected the relic. Tracked per-run via `ModelChoiceHistoryEntry.wasPicked` in run history files, NOT aggregated in progress state.
- For SpireTracker v0.1, we use `DiscoveredRelics` — if a relic is NOT in this set, the player has literally never seen it before. This is the most useful signal for "NEW".

### Relic Reward UI

- **`MegaCrit.Sts2.Core.Nodes.Rewards.NRewardButton`** — The clickable reward button
  - Extends `NButton` (Godot Control)
  - `Reward? Reward` property — the reward data (could be `RelicReward`, `CardReward`, etc.)
  - `private Control _iconContainer` — holds the relic icon, good target for badge attachment
  - `private void Reload()` — called when reward is assigned, builds the visual
  - `MarkContentAsSeen()` is called later via `RelicReward.MarkContentAsSeen()` → `SaveManager.Instance.MarkRelicAsSeen()`

- **`MegaCrit.Sts2.Core.Rewards.RelicReward`** — Reward data for a relic
  - `private RelicModel? _relic` — the relic being offered
  - `bool IsPopulated` — whether the relic has been determined
  - `void MarkContentAsSeen()` → calls `SaveManager.Instance.MarkRelicAsSeen(_relic)`

### Boss Relic Selection ("Choose a Relic")

- **`MegaCrit.Sts2.Core.Nodes.Screens.NChooseARelicSelection`** — Boss/treasure relic pick screen
  - `private Control _relicRow` — contains relic choice holders
  - Children are likely `NTreasureRoomRelicHolder` instances
  - `_Ready()` initializes the screen

### Shop Relics

- **`MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantRelic`** — Shop relic slot
  - Extends `NMerchantSlot`
  - `private Control _relicHolder` — visual container for the relic
  - `void UpdateVisual()` — called to update the display
  - Entry is `MerchantRelicEntry` with `.Model` property (RelicModel)

### Relic Model IDs

- Type: `MegaCrit.Sts2.Core.Models.ModelId` (not raw strings at runtime)
- Serialized as strings like `"RELIC.BURNING_BLOOD"` in JSON save files
- Compared using ModelId equality operators

### Mod Entry Point Pattern

- Attribute: `[ModInitializer("MethodName")]` on a class
- The method must be `public static void`
- Uses `MegaCrit.Sts2.Core.Modding` namespace
- Harmony is available as `0Harmony.dll` in the game data directory
- BetterSpire2 reference mod confirms this pattern works in production
