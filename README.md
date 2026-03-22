# SpireTracker

A lightweight QoL mod for **Slay the Spire 2** that marks relics you've never picked up with a **"NEW"** badge. Helps completionists track what's still unexplored.

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple)
![Godot 4.5](https://img.shields.io/badge/Godot-4.5-blue)
![License: MIT](https://img.shields.io/badge/License-MIT-green)

## Features

- 🏷️ **"NEW" badges** on relics you haven't picked up before
- 🎁 **Reward screen** — relic rewards after combat
- 👑 **Boss/treasure relics** — "choose a relic" selection screen
- 🛒 **Shop relics** — merchant relic slots
- ⚡ **Zero persistence** — syncs with the game's own Compendium tracking
- 🔒 **No gameplay impact** — purely visual, won't affect saves or achievements

## How It Works

The game's Compendium tracks which relics you've picked up via `ProgressState.DiscoveredRelics`. SpireTracker uses [Harmony](https://github.com/pardeike/Harmony) to patch the UI methods that display relics, checking each relic against the Compendium's discovered set. If a relic hasn't been picked up yet, we attach a gold "NEW" label to its icon — matching the Compendium's "Unknown" state.

### Patched Methods

| Class | Method | Screen |
|---|---|---|
| `NRewardButton` | `Reload()` | Relic rewards (combat, events) |
| `NChooseARelicSelection` | `_Ready()` | Boss/treasure relic picks |
| `NMerchantRelic` | `UpdateVisual()` | Shop relic slots |

## Installation

1. Navigate to your Slay the Spire 2 install directory
2. Create a `mods/SpireTracker/` folder if it doesn't exist
3. Copy `SpireTracker.dll` and `mod_manifest.json` into the folder
4. Launch the game and enable mods

> **⚠️ Note:** Enabling mods switches to a separate save system. Copy your `profile1/` folder to `modded/` to preserve progress.

### Game Data Locations

| OS | Path |
|---|---|
| **Windows** | `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\` |
| **macOS** | `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/` |
| **Linux** | `~/.local/share/Steam/steamapps/common/Slay the Spire 2/` |

## Building from Source

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Slay the Spire 2 installed via Steam (the build references game DLLs)

### Build

```bash
dotnet build -c Release
```

The output DLL will be in `bin/Release/net9.0/`. Copy `SpireTracker.dll` and `mod_manifest.json` to your mods folder.

## Project Structure

```
SpireTracker/
├── SpireTracker.cs          # Mod entry point ([ModInitializer])
├── RelicTracker.cs          # Progress data query helper
├── Patches/
│   ├── RelicRewardPatch.cs  # Relic reward screen patch
│   ├── ChooseRelicPatch.cs  # Boss/treasure relic pick patch
│   └── MerchantRelicPatch.cs# Shop relic patch
├── UI/
│   └── NewBadge.cs          # Gold "NEW" label creation
├── SpireTracker.csproj      # .NET 9.0 project
└── mod_manifest.json        # STS2 mod manifest
```

## Status

**v0.1.0** — Code complete, awaiting in-game testing.

- [x] Project scaffold & build
- [x] Game API decompilation & recon
- [x] Harmony patches for all 3 relic screens
- [x] "NEW" badge UI
- [ ] In-game verification
- [ ] Badge positioning/sizing tuning

## Roadmap

- **Phase 2:** Card tracking (same pattern, different reward type)
- **Phase 3:** Potion tracking
- **Future:** Mod settings toggle, "seen but never picked" distinction

## Credits

- [MegaCrit](https://www.megacrit.com/) for Slay the Spire 2
- [Harmony](https://github.com/pardeike/Harmony) for runtime patching
- [BetterSpire2](https://github.com/jdr1813/BetterSpire2) for mod pattern reference

## License

MIT
