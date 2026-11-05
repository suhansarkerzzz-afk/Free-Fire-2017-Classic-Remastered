# Godot Port — Development Status

This is a new Godot 4.4.1 project being built alongside the legacy Unity prototype. The Unity files are intentionally retained and are not part of the Godot Android build.

## First playable foundation in this commit

- Godot 4.4.1 project with Compatibility renderer for broad Android device support.
- Procedural low-poly island ground, roads, simple houses, trees and named district labels.
- Offline Solo match entry menu.
- Third-person player controller with keyboard and touch-look / virtual movement input.
- 200 player HP, 30-round rifle magazine, reserve ammo and reload.
- 49 basic villain bots, each with 200 HP, chase movement and close-range damage.
- Basic raycast shooting and elimination count.
- Android debug APK export preset for ARMv7 and ARM64.
- GitHub Actions workflow to export a debug APK without a Unity license.

## Not yet ported / incomplete

- Full historical Bermuda map geometry and all 20 named POIs.
- Original weapon roster, pickup/drop inventory, medkits and loot.
- Parachute / plane, jeep, safe-zone ring and full battle HUD.
- Local Wi-Fi / hotspot room discovery and Bluetooth direct link.
- Authoritative multiplayer synchronization.
- Full mobile joystick and touch button UX polish, audio, animations and performance tuning.

## Build status

The project files and workflow have been committed, but no Godot import, script parse, runtime, Android export, APK install or real-device test has been performed yet. The first CI export must be run to reveal any Godot-version, export-template or Android SDK issues.

## Android CI export configuration update

- GitHub Actions now writes Godot 4 editor settings for the runner's Java 17 and Android SDK paths.
- CI generates a temporary Android debug keystore and supplies the documented Godot debug-signing environment variables.
- A headless runtime validation step now runs before APK export.
- APK export is required to produce a non-empty file before artifact upload.
- This configuration change is pending a fresh CI run; it is not yet a verified successful APK build.
