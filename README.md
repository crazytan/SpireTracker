# SpireTracker

A lightweight QoL mod for **Slay the Spire 2** that marks relics you've never picked up with a **"NEW"** badge. Helps completionists track what's still unexplored.

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple)
![Godot 4.5](https://img.shields.io/badge/Godot-4.5-blue)
![License: MIT](https://img.shields.io/badge/License-MIT-green)

## Features

- 🏷️ **"NEW" badges** on relics you haven't picked up before
- 🎁 **Reward screen** — relic rewards after combat
- 👑 **Boss/treasure relics** — boss relic selection and treasure rooms
- 🛒 **Shop relics** — merchant relic slots
- 🔮 **Neow's blessings** — ancient event relic options
- ⚡ **Zero persistence** — syncs with the game's own Compendium tracking
- 🔒 **No gameplay impact** — purely visual, won't affect saves or achievements

## Installation

1. Download the latest release from [GitHub Releases](https://github.com/crazytan/SpireTracker/releases) or [Nexus Mods](https://www.nexusmods.com/slaythespire2/mods/)
2. Extract the zip into your game's `mods/` folder:
   - **Windows:** `<Steam>\steamapps\common\Slay the Spire 2\mods\SpireTracker\`
   - **macOS:** `~/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/SpireTracker/`
   - **Linux:** `~/.local/share/Steam/steamapps/common/Slay the Spire 2/mods/SpireTracker/`
3. The folder should contain `SpireTracker.dll` and `mod_manifest.json`
4. Launch the game — mods are enabled automatically

> **⚠️ Note:** Enabling mods switches to a separate save system. Copy your `profile1/` folder to `modded/` to preserve your Compendium progress.

## How It Works

The game's Compendium tracks which relics you've picked up via `ProgressState.DiscoveredRelics`. SpireTracker uses [Harmony](https://github.com/pardeike/Harmony) to patch the UI methods that display relics, checking each relic against the Compendium's discovered set. If a relic hasn't been picked up yet, we attach a gold "NEW" label to its icon — matching the Compendium's "Unknown" state.

### Patched Screens

| Class | Method | Screen |
|---|---|---|
| `NRewardButton` | `Reload()` | Relic rewards (combat, events) |
| `NChooseARelicSelection` | `_Ready()` | Boss relic picks |
| `NTreasureRoomRelicCollection` | `InitializeRelics()` | Treasure room relics |
| `NMerchantRelic` | `UpdateVisual()` | Shop relic slots |
| `NEventOptionButton` | `_Ready()` | Neow's blessings & ancient events |

## Building from Source

Most users should just download the pre-built release. Only follow these steps if you want to modify the mod.

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Slay the Spire 2 installed via Steam (the build references game DLLs)

### Build

```bash
dotnet build -c Release
```

If Steam is installed in a non-default location, pass your game directory:

```bash
dotnet build -c Release -p:Sts2Dir="D:\SteamLibrary\steamapps\common\Slay the Spire 2"
```

Output: `bin/Release/net9.0/SpireTracker.dll` — copy it along with `mod_manifest.json` to your `mods/SpireTracker/` folder.

## Project Structure

```
SpireTracker/
├── SpireTracker.cs              # Mod entry point ([ModInitializer])
├── RelicTracker.cs              # Compendium progress query helper
├── Patches/
│   ├── RelicRewardPatch.cs      # Relic reward screen
│   ├── ChooseRelicPatch.cs      # Boss relic selection
│   ├── TreasureRoomRelicPatch.cs# Treasure room relics
│   ├── MerchantRelicPatch.cs    # Shop relics
│   └── EventOptionPatch.cs      # Neow & ancient events
├── UI/
│   └── NewBadge.cs              # Gold "NEW" badge label
├── SpireTracker.csproj          # .NET 9.0 project
└── mod_manifest.json            # STS2 mod manifest
```

## Status

**v0.1.0** — Working in-game!

- [x] Project scaffold & build
- [x] Game API decompilation & recon
- [x] Harmony patches for all 5 relic screens
- [x] "NEW" badge UI with proper depth layering
- [x] In-game testing (Neow, rewards, shop, treasure room)

## Roadmap

- **Phase 2:** Card tracking (same pattern, different reward type)
- **Phase 3:** Potion tracking
- **Future:** Mod settings toggle, badge style customization

## Credits

- [MegaCrit](https://www.megacrit.com/) for Slay the Spire 2
- [Harmony](https://github.com/pardeike/Harmony) for runtime patching
- [BetterSpire2](https://github.com/jdr1813/BetterSpire2) for mod pattern reference

## License

MIT
