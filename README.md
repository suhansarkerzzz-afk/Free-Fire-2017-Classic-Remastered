# Free Fire 2017 Classic Remastered

An original, fan-inspired battle-royale prototype with a 2017-era visual direction and lightweight procedural assets. This is not an official Garena product and does not use official Garena assets.

## Current Android direction: Godot Port

The Android project is now being rebuilt in **Godot 4.4.1** so the APK workflow does not depend on a Unity license. The new project lives in `GodotPort/`. The previous Unity prototype is intentionally retained in the repository as a reference; it is not used by the Godot Android workflow.

### Godot prototype foundation

- Offline Solo match entry menu.
- Procedural low-poly island ground, roads, houses, trees and named district labels.
- Third-person player controller with keyboard input and touch movement/look handling.
- Player health 200, rifle magazine 30, reserve ammo and reload.
- 49 basic villain bots with 200 HP, chase movement and close-range attacks.
- Raycast shooting and elimination counter.
- Android debug APK export preset for ARMv7 and ARM64.
- Godot Compatibility renderer selected for broad Android device support.

See [GodotPort/PORT_STATUS.md](GodotPort/PORT_STATUS.md) for what is implemented and what remains to be ported.

### Build Android APK with GitHub Actions

1. Open repository **Actions**.
2. Select **Godot Android APK Build**.
3. Choose **Run workflow**.
4. When the run succeeds, download the artifact named `FreeFire2017ClassicRemastered-Godot-Android-APK`.

No Unity license or `UNITY_LICENSE` secret is required for this Godot workflow. The workflow is manual and has not yet been run. A successful export and phone installation are not claimed.

## Planned Godot port milestones

1. Complete the player controller, mobile controls and stable 49-bot Solo match.
2. Port weapon roster, pickups, inventory, medkits, loot and damage balancing.
3. Port the Bermuda-inspired island with all 20 named districts, improved terrain and minimap.
4. Port parachute / plane, safe zone and vehicle gameplay.
5. Rebuild Local Wi-Fi / hotspot room discovery and Bluetooth direct-link support in Godot.
6. Improve 2 GB RAM performance, then test on real Android phones.
7. Windows PC build after Android stabilizes.

## Legacy Unity prototype (reference only)

The repository still contains the original Unity implementation under `Assets/`, `Packages/` and `ProjectSettings/`. It includes:

- Offline Solo design for one player against 49 AI bots.
- Local Wi-Fi / mobile Hotspot BETA with room-code discovery and basic player transform replication.
- Optional Bluetooth Classic RFCOMM direct 1-to-1 link prototype.
- Procedural Bermuda-inspired districts, player movement, weapons, health, loot, safe zone, vehicle and parachute prototypes.

The legacy local multiplayer is incomplete: authoritative health/damage, shooting, loot, bot state, match phase, multi-client Bluetooth rooms, reconnect and host migration are not synchronized. It is not a complete synchronized battle-royale match.

The old Unity workflow `.github/workflows/android-apk.yml` remains for reference only and still requires a valid Unity license. Use `.github/workflows/godot-android-apk.yml` for the new Android direction.

## Important testing status

No Godot import, GDScript parse, runtime gameplay test, Android APK export, APK installation, real-device performance test or network test has been performed yet. The Godot source and workflow have been committed to GitHub, but successful build or gameplay behaviour is not yet verified.
