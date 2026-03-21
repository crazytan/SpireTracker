# SpireTracker — Implementation Status

## What's Done

### Project Scaffold
- .NET 9.0 C# library project (`SpireTracker.csproj`)
- References game DLLs (`sts2.dll`, `0Harmony.dll`) via platform-aware paths (macOS/Windows/Linux)
- `GodotSharp` NuGet package for Godot API access
- `mod_manifest.json` with id "SpireTracker"
- `dotnet build` succeeds, produces `SpireTracker.dll` (11KB)

### Game API Decompilation (RECON.md)
- Decompiled `sts2.dll` and documented all relevant classes
- Found `SaveManager.Instance.Progress.DiscoveredRelics` — the set of relics the player has seen
- Found `NRewardButton.Reload()` — the method that builds reward button visuals
- Found `NChooseARelicSelection._Ready()` — boss/treasure relic pick screen init
- Found `NMerchantRelic.UpdateVisual()` — shop relic display update
- Documented the "discovered" vs "picked" distinction

### Mod Entry Point (`SpireTracker.cs`)
- Uses `[ModInitializer]` attribute (same pattern as BetterSpire2)
- Creates Harmony instance and patches all three target classes
- Logs success/failure for each patch

### RelicTracker Helper (`RelicTracker.cs`)
- `IsNewRelic(ModelId)` — checks if relic is NOT in `DiscoveredRelics`
- Called before the game marks the relic as seen (via Harmony Prefix timing)

### Harmony Patches
- **`RelicRewardPatch`** — Prefix/Postfix on `NRewardButton.Reload()`
  - Prefix: checks if relic is new before `MarkContentAsSeen()` runs
  - Postfix: attaches "NEW" badge to icon container
- **`ChooseRelicPatch`** — Postfix on `NChooseARelicSelection._Ready()`
  - Iterates relic row children and badges new relics
- **`MerchantRelicPatch`** — Postfix on `NMerchantRelic.UpdateVisual()`
  - Checks shop relic entry and badges new relics

### UI Badge (`UI/NewBadge.cs`)
- Creates gold "NEW" `Label` with black outline
- Positioned at top-right of relic icon
- Prevents duplicate badges via node name check
- `AttachTo()` / `RemoveFrom()` API

## What's Left (Needs In-Game Testing)

### Must Verify
- [ ] Mod loads in-game (drop DLL + manifest into `mods/SpireTracker/` folder)
- [ ] "SpireTracker loaded!" appears in game logs
- [ ] Harmony patches apply without errors
- [ ] "NEW" badges appear on reward screen relics
- [ ] "NEW" badges appear on boss/treasure relic selection
- [ ] "NEW" badges appear on shop relics
- [ ] Badges don't duplicate on screen refresh
- [ ] Badges don't interfere with hover tooltips or click targets

### Likely Adjustments Needed
- `ChooseRelicPatch`: The `_relicRow` children may not have a `_relic` field — need to verify the exact child type and field name in-game. May need to use a different hook point.
- `MerchantRelicPatch`: The `Entry` property accessor might need adjustment based on the actual `NMerchantSlot` API.
- Badge positioning: The `Vector2(-8, -4)` offset may need tuning based on actual relic icon sizes in-game.
- Badge font size: 14pt may be too large or small depending on UI scale.

### Phase 2 Ideas
- Track "picked" vs just "discovered" by scanning run history files
- Add card tracking (same pattern, different reward type)
- Add potion tracking
- Add mod settings toggle (enable/disable badges)
- Test in multiplayer/co-op
