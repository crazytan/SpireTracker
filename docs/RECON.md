# Decompilation Findings

Decompiled `sts2.dll` (v0.99.1, build 7ac1f450, 2026-03-13) using ilspycmd 9.1.0.

## Key Classes and APIs

### Progress Tracking

- **`MegaCrit.Sts2.Core.Saves.ProgressState`** — Holds all cross-run progress data
  - `IReadOnlySet<ModelId> DiscoveredRelics` — Set of relic IDs the player has ever picked up
  - `bool MarkRelicAsSeen(ModelId relicId)` — Adds a relic to the discovered set; returns true if newly added
  - Same pattern for cards (`DiscoveredCards`), potions (`DiscoveredPotions`), events, acts

- **`MegaCrit.Sts2.Core.Saves.SaveManager`** — Singleton (`SaveManager.Instance`)
  - `ProgressState Progress` — The current progress state
  - `void MarkRelicAsSeen(RelicModel relic)` — Marks relic as discovered
  - `bool IsRelicSeen(RelicModel relic)` — Checks `Progress.DiscoveredRelics.Contains(relic.Id)`

- **`MegaCrit.Sts2.Core.Saves.SerializableProgress`** — Serialization layer
  - `List<ModelId> DiscoveredRelics` — Persisted as `"discovered_relics"` in JSON

### Important: "Discovered" = Picked Up

- Despite the name, `DiscoveredRelics` tracks relics the player has **picked up**, not merely seen.
- This matches the in-game Compendium behavior: relics show as "Unknown" until picked up.
- `RelicReward.MarkContentAsSeen()` → `SaveManager.MarkRelicAsSeen()` is what adds relics.
- For SpireTracker: if a relic is NOT in `DiscoveredRelics`, badge as "NEW".

### NRelic — The Core Relic Display Node

- **`MegaCrit.Sts2.Core.Nodes.Relics.NRelic`** — Godot node for relic icons
  - Scene: `relics/relic`
  - `public RelicModel Model` — getter/setter; setter fires `ModelChanged` and calls `Reload()`
  - `TextureRect Icon` — node `%Icon`
  - `TextureRect Outline` — node `%Outline`
  - `IconSize _iconSize` — enum: `Small` (atlas packed), `Large` (full-res `BigIcon`)
  - `Reload()` — updates Icon/Outline textures based on Model and IconSize
  - **NRelic nodes are pooled** — `Reload()` fires each time a pooled node is repopulated
  - **BaseLib patches `NRelic.Reload()`** to add custom UI children (the canonical hook point)

### NRelicBasicHolder — Simple Relic Display

- **`MegaCrit.Sts2.Core.Nodes.Relics.NRelicBasicHolder`** — Extends NButton
  - Scene: `relics/relic_basic_holder`
  - `public NRelic Relic` — the NRelic child node at `%Relic`
  - `private RelicModel _model` — set in `Create()` factory before entering tree
  - `OnFocus()` — scales icon 1.25x, shows hover tips
  - Used by NChooseARelicSelection for boss relic picks

### Relic Reward UI

- **`MegaCrit.Sts2.Core.Nodes.Rewards.NRewardButton`** — Clickable reward button
  - Extends `NButton`
  - `Reward? Reward` — the reward data (could be `RelicReward`, `CardReward`, etc.)
  - `private Control _iconContainer` — node `%Icon`, holds the relic icon
  - `private MegaRichTextLabel _label` — node `%Label`
  - `private void Reload()` — creates icon from `Reward.CreateIcon()`, adds to `_iconContainer`

- **`MegaCrit.Sts2.Core.Rewards.RelicReward`** — Reward data for a relic
  - `private RelicModel? _relic` — the relic being offered
  - `bool IsPopulated` — whether the relic has been determined
  - `CreateIcon()` — creates TextureRect (NOT NRelic) with BigIcon + shader material
  - `MarkContentAsSeen()` → `SaveManager.Instance.MarkRelicAsSeen(_relic)`

### Boss Relic Selection — Overlay Screen

- **`MegaCrit.Sts2.Core.Nodes.Screens.NChooseARelicSelection`** — Boss/event relic pick
  - `private Control _relicRow` — contains relic choice holders
  - `private IReadOnlyList<RelicModel> _relics` — set before push
  - `_Ready()` creates `NRelicBasicHolder.Create(relic)` for each relic, spaced 200px apart
  - Children are `NRelicBasicHolder` (NOT NTreasureRoomRelicHolder)

### Treasure Room Relic Selection

- **`MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic.NTreasureRoomRelicCollection`**
  - Scene: `screens/shared_relic_picking_screen`
  - `NTreasureRoomRelicHolder SingleplayerRelicHolder` — node `%SingleplayerRelicHolder`
  - `List<NTreasureRoomRelicHolder> _multiplayerHolders` — for multi-relic chests
  - `List<NTreasureRoomRelicHolder> _holdersInUse` — whichever set is active
  - `InitializeRelics()` — populates from `TreasureRoomRelicSynchronizer.CurrentRelics`
  - Empty chest: creates MegaLabel with "TREASURE_EMPTY" text

- **`MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic.NTreasureRoomRelicHolder`**
  - `NRelic Relic` — node `%Relic`
  - `Initialize(relic)` — sets `Relic.Model = relic`, shows glow particles for uncommon/rare

### Shop Relics

- **`MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantRelic`** — Extends NMerchantSlot
  - `NRelic.IconSize _iconSize` — [Export]
  - `Control _relicHolder` — node `%RelicHolder`
  - `NRelic? _relicNode` — the NRelic created in UpdateVisual()
  - `MerchantRelicEntry _relicEntry` — has `.Model` (RelicModel) and `.Cost`
  - `UpdateVisual()` — creates `NRelic.Create(_relicEntry.Model, _iconSize)`, adds to `_relicHolder`

### Neow Event (Relic Blessings)

- **`MegaCrit.Sts2.Core.Models.Events.Neow`** — Extends AncientEventModel
  - Offers relic choices via `EventOption` with `RelicOption<T>()` pattern
  - PositiveOptions: ArcaneScroll, BoomingConch, Pomander, GoldenPearl, LeadPaperweight,
    NewLeaf, NeowsTorment, PreciseScissors, LostCoffer
  - CurseOptions: CursedPearl, LargeCapsule, LeafyPoultice, PrecariousShears
  - Extra options: NutritiousOyster, StoneHumidifier, MassiveScroll, LavaRock,
    SmallCapsule, SilverCrucible, ScrollBoxes
  - `GenerateInitialOptions()` picks 1 curse + 2 positive (shuffled, with anti-duplicates)
  - Relic display uses the event system's option UI (not NRelic directly)

### Relic Inventory (Player's Equipped Relics)

- **`MegaCrit.Sts2.Core.Nodes.Relics.NRelicInventory`** — FlowContainer
  - Listens to `_player.RelicObtained` / `_player.RelicRemoved`
  - `Add()` creates `NRelicInventoryHolder.Create(relic)`
  - `AnimateRelic()` — plays newly-acquired animation from start position
  - Click opens inspect screen: `NGame.Instance.GetInspectRelicScreen().Open(list, model)`

### Compendium (Relic Collection)

- **`NRelicCollectionEntry`** — Scene for each relic in compendium
  - `ModelVisibility` enum: `None`, `Visible`, `NotSeen`, `Locked`
  - `NotSeen`: creates NRelic but sets `Icon.SelfModulate = StsColors.ninetyPercentBlack`
  - `Visible`: normal display with character pool outline color

- **`NRelicCollection`** — Main compendium screen
  - Loads `SaveManager.Instance.Progress.DiscoveredRelics` as `HashSet<RelicModel>`
  - Categories: Starter, Common, Uncommon, Rare, Shop, Ancient, Event

### MegaLabel

- **`MegaCrit.Sts2.addons.mega_text.MegaLabel`** — Extends Label
  - `_Ready()` calls `MegaLabelHelper.AssertThemeFontOverride()` and `RefreshFont()`
  - Auto-sizes text to fit container bounds (binary search on font size)
  - Has `AutoSizeEnabled`, `MinFontSize`, `MaxFontSize` properties
  - Requires theme font override — will log errors without one

### Relic Model

- **`MegaCrit.Sts2.Core.Models.RelicModel`** — Abstract, extends AbstractModel
  - `ModelId Id` — e.g., `"RELIC.BURNING_BLOOD"`
  - `Icon` — atlas texture from `atlases/relic_atlas.sprites/`
  - `BigIcon` — full-res from `relics/<name>.png`
  - `Rarity` — Common, Uncommon, Rare, Shop, Ancient, Starter, Event, None
  - `MerchantCost` — Common=200, Uncommon=250, Rare=300, Shop=225
  - `HoverTips` — list of HoverTip for tooltip display

### Mod Entry Point Pattern

- Attribute: `[ModInitializer("MethodName")]` on a class
- Uses `MegaCrit.Sts2.Core.Modding` namespace
- Harmony available as `0Harmony.dll` in game data directory
- BetterSpire2 confirms: plain Godot Label + theme overrides works for mod UI
- BetterSpire2 uses `FindGameFont()` to scavenge a font from game scene tree nodes

## UI Patterns from Other Mods

### BetterSpire2 (reference mod)
- Uses plain Godot `Label` with `AddThemeColorOverride`/`AddThemeFontSizeOverride` — works
- Explicitly finds and caches a game font via scene tree walking
- Uses `ZIndex = 100` for overlays, `CanvasLayer` (Layer=10) for floating panels
- Validates nodes with `GodotObject.IsInstanceValid()` before every use
- Uses `QueueFree()` for cleanup, `CallDeferred()` for initial positioning

### BaseLib (community framework)
- Patches `NRelic.Reload()` to add custom UI children — the canonical hook point
- Uses `AccessTools.Field(typeof(NRelic), "_model")` for private field access
- Creates `NTemporaryUi : Control` marker class to identify/cleanup mod-added nodes
- Handles node pooling: cleans up existing `NTemporaryUi` children before adding new ones
- Uses `SpireField<T>` (ConditionalWeakTable wrapper) to attach extra data to game objects
