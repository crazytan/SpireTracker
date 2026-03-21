# SpireTracker — Relic Tracker Mod for Slay the Spire 2

## Overview

A lightweight QoL mod that marks relics you **haven't picked yet** with a visual "NEW" indicator during gameplay. Helps completionists track what they've tried and what's still unexplored.

**Phase 1: Relics only.** Cards and potions may follow in future phases.

## Tech Stack

- **Engine:** Godot 4.5 (game is native Godot, but supports C# mods)
- **Mod framework:** C# / .NET 9.0 with Harmony patching (officially supported by STS2)
- **Mod loader:** Built-in — game loads `.dll` + `.pck` from `mods/` folder
- **Base dependency:** [BaseLib](https://github.com/Alchyr/BaseLib-StS2) (community mod framework)
- **Template:** [ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2)

## Game Architecture (Recon Results)

- **Game binary:** Native Godot (Mach-O universal, arm64 + x86_64)
- **Game resources:** `Slay the Spire 2.pck` (1.7GB Godot resource pack)
- **Version:** v0.99.1 (build 7ac1f450, 2026-03-13)
- **Bundle ID:** `com.megacrit.SlayTheSpire2`
- **Mods folder:** `SlayTheSpire2.app/../mods/` (needs to be created)
- **⚠️ Modded mode:** Enabling mods switches to a separate save system. Copy `profile1/` → `modded/` to preserve progress.

### Save File Locations

- **Steam cloud:** `~/Library/Application Support/Steam/userdata/207085905/2868840/remote/`
- **Local data:** `~/Library/Application Support/SlayTheSpire2/steam/`
- **Profile:** `profile1/saves/progress.save` (JSON)
- **Run history:** `profile1/saves/history/*.run` (JSON)

### Progress Data Structure

`progress.save` contains:
- `discovered_relics`: list of 114 discovered relic IDs (e.g., `"RELIC.BURNING_BLOOD"`)
- `discovered_cards`: 406 cards
- `discovered_potions`: 47 potions
- `character_stats`: per-character win/loss/playtime/ascension
- `ancient_stats`: per-ancient win/loss by character

Run history files contain per-floor data including `relic_choices` with `was_picked` flags.

### Player Stats (Jia's profile)

- **Total playtime:** 10.3 hours, 49 runs
- **Characters:** Ironclad (0-3), Silent (1-1), Regent (1-1), Necrobinder (0-2), Defect (1-0)
- **Steam ID:** 207085905

## Architecture

### Core Principle

**No custom persistence.** The game's `progress.save` already tracks which relics have been obtained vs merely discovered. We read the game's own progress data at runtime — zero external JSON files.

### What We Hook

1. **Relic reward screen** — When relics are offered (boss relic, treasure room, shop, events)
   - Harmony Postfix on the UI method that renders relic choices
   - For each relic: query the game's progress/compendium data to check if it's been picked before
   - If never picked → overlay a "NEW" indicator on the relic

2. **Shop relics** — Same logic, different UI surface
   - Hook the shop relic display

### What We DON'T Hook

- Card rewards (Phase 2)
- Potion rewards (Phase 3)
- Combat or game logic — this mod is purely visual/informational

### Visual Design

- Small **"NEW"** text label (or ✦ star icon) positioned at the top-right corner of the relic image
- Color: bright gold or green to stand out without being obnoxious
- Rendered as a Godot `Label` or `TextureRect` node, added as a child of the relic's UI node
- Should not interfere with hover tooltips or click targets

## Implementation Plan

### Step 0: Recon ✅ (partially complete)
- [x] Confirm game architecture (native Godot with C# mod support + Harmony)
- [x] Find save file locations and progress data structure
- [x] Identify relic ID format (`RELIC.BURNING_BLOOD` style strings)
- [ ] Set up BaseLib + ModTemplate and verify a hello-world mod loads
- [ ] Use in-game console (`` ` `` key) to explore relic/progress APIs
- [ ] Decompile game or BaseLib DLL to find:
  - Progress tracking class (relic obtained/discovered sets)
  - Relic reward UI class and render method
  - Shop UI class (relic section)

### Step 1: Scaffold
- [ ] Create project from ModTemplate-StS2
- [ ] Set up `mod_manifest.json`
- [ ] Create entry point
- [ ] Verify mod loads in-game (just a log message)

### Step 2: Progress Data Access
- [ ] Access the game's progress/compendium singleton at runtime
- [ ] Build a helper: `bool HasPickedRelic(string relicId)`
- [ ] Test by logging picked vs unpicked relics on mod load

### Step 3: Relic Reward UI Patch
- [ ] Harmony Postfix on relic reward screen render method
- [ ] For each displayed relic, check `HasPickedRelic()`
- [ ] If unpicked → create and attach a "NEW" Label node to the relic's UI element
- [ ] Handle cleanup when the reward screen closes

### Step 4: Shop Patch
- [ ] Same logic applied to shop relic display

### Step 5: Polish
- [ ] Ensure labels don't duplicate on screen refresh
- [ ] Test across all relic sources (boss, treasure, shop, events)
- [ ] Add mod settings toggle (if feasible) to enable/disable the overlay
- [ ] Test in multiplayer (co-op) — labels should reflect the local player's progress

## File Structure

```
SpireTracker/
├── SPEC.md                  # This file
├── SpireTracker.csproj      # .NET 9.0 project
├── SpireTracker.cs          # Entry point
├── RelicTracker.cs          # Progress data access helper
├── Patches/
│   ├── RelicRewardPatch.cs  # Harmony patch for relic reward screen
│   └── ShopRelicPatch.cs    # Harmony patch for shop relics
├── UI/
│   └── NewBadge.cs          # "NEW" label creation and positioning
├── mod_manifest.json
└── mod_image.png            # Mod icon for in-game mod list
```

## Open Questions (resolve during remaining Step 0)

1. What's the exact class/method for checking relic pick history at runtime?
2. What's the relic reward screen class and its render method?
3. Does the shop use a separate UI class or share with reward screen?
4. What Godot node type are relic UI elements? (needed for attaching child labels)
5. How does the modded save system handle progress — does it copy from vanilla or start fresh?

## Links

- [BaseLib (mod framework)](https://github.com/Alchyr/BaseLib-StS2)
- [BaseLib Wiki](https://alchyr.github.io/BaseLib-Wiki/)
- [ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2)
- [STS2 Modding Guide (Chinese, comprehensive)](https://github.com/freude916/sts2-quickRestart)
- [BetterSpire2 (reference mod)](https://github.com/jdr1813/BetterSpire2)
- [STS2 Nexus Mods](https://www.nexusmods.com/slaythespire2/mods)
- [Viewed Cards Statistics (similar concept)](https://www.nexusmods.com/slaythespire2/mods/170)
