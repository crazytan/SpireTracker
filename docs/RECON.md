# Decompilation Findings

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

### Important: "Discovered" = Picked Up

- Despite the name, `DiscoveredRelics` tracks relics the player has **picked up**, not merely seen.
- This matches the in-game Compendium behavior: relics show as "Unknown" until picked up.
- For SpireTracker, if a relic is NOT in `DiscoveredRelics`, the player has never picked it up → badge as "NEW".

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

### Boss Relic Selection ("Choose a Relic") — Overlay Screen

- **`MegaCrit.Sts2.Core.Nodes.Screens.NChooseARelicSelection`** — Boss/event relic pick overlay
  - `private Control _relicRow` — contains relic choice holders
  - Children are **`NRelicBasicHolder`** (NOT NTreasureRoomRelicHolder)
  - `NRelicBasicHolder.Relic` (NRelic) → `.Model` (RelicModel)
  - `_Ready()` creates holders dynamically from `_relics` list and adds them to `_relicRow`

### Treasure Room Relic Selection

- **`MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic.NTreasureRoomRelicCollection`** — Treasure chest relics
  - `void InitializeRelics()` — populates holders from `TreasureRoomRelicSynchronizer.CurrentRelics`
  - `SingleplayerRelicHolder` (NTreasureRoomRelicHolder) — used for 1-relic chests
  - `_multiplayerHolders` (List) — used for multi-relic chests
  - `_holdersInUse` (List) — whichever set is active
  - **`NTreasureRoomRelicHolder`** extends NButton
    - `Relic` property (NRelic) → `.Model` (RelicModel)
    - In namespace `MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic`

### Shop Relics

- **`MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantRelic`** — Shop relic slot
  - Extends `NMerchantSlot`
  - `private Control _relicHolder` — visual container for the relic
  - `private RelicModel? _relic` — set in `FillSlot()`, the relic model
  - `void UpdateVisual()` — called to update the display (protected override)

### NRelicBasicHolder

- **`MegaCrit.Sts2.Core.Nodes.Relics.NRelicBasicHolder`** — Simple relic display holder
  - `NRelic Relic` — public property, the relic node
  - `private NRelic _relic` — set in `_Ready()` from `GetNode<NRelic>("%Relic")`
  - `private RelicModel _model` — set in `Create()` before entering tree

### MegaLabel

- **`MegaCrit.Sts2.addons.mega_text.MegaLabel`** extends `Label`
  - `_Ready()` calls `MegaLabelHelper.AssertThemeFontOverride()` and `RefreshFont()`
  - Auto-sizes text to fit container bounds
  - Requires theme font override — plain Labels may render without it using fallback fonts

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
