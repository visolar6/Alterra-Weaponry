# Alterra Weaponry - Changelog

> **Important Note**: This is a community port of the original Subnautica: Below Zero mod created by [VELD](https://github.com/VELD-Dev/). The vast majority of this mod's functionality and design was created by VELD. This changelog documents the changes made for the Subnautica port.

## [1.0.5] - Subnautica Port - 2026-01-22

### Added
- Sound effect for explosive torpedo detonations using crashfish explosion sound
- Banner images for PDA entries (Coal, BlackPowder, ExplosiveTorpedo)
- Separate asset bundle for banner images to avoid PathID conflicts

### Changed
- Reduced particle effect scaling for torpedo explosions (divisor increased to 15 for more subtle visuals)
- Improved explosion particle effect customization with proper scaling modes

### Fixed
- Fixed typo in mod.json (`Dependecies` → `Dependencies`)
- Asset bundle loading now uses separate bundles for core assets and banners

## [1.0.3] - Previous Release
- Initial release features

## Installation for Vortex
1. Download the mod archive
2. Extract to `\BepInEx\plugins\AlterraWeaponry\`
3. Launch the game through Vortex

## Requirements
- BepInEx 5.x
- SMLHelper v2.14.1 or higher
- Subnautica (v46.3+)

## Compatibility
- ✅ Nitrox Compatible
- ✅ Vortex Deployable
