# Free Fire 2017 Classic Remastered

An original, fan-inspired battle-royale prototype with a 2017-era visual direction and lightweight procedural assets. This is not an official Garena product and does not use official Garena assets.

## Current Android direction: Godot Port

The active Android project is **Godot 4.4.1** in `GodotPort/`. The previous Unity prototype is retained in the repository as reference code and is not used by the Godot Android workflow.

### Godot prototype foundation

- Procedural low-poly island, roads, houses, trees and 12 district labels.
- Third-person player with keyboard controls and Android touch-drag movement/look.
- Player health 200, rifle magazine 30, reserve ammo and reload.
- 49 basic villain bots with 200 HP, chase movement and close-range attacks.
- Basic raycast shooting and HUD.
- Android landscape export for ARMv7 and ARM64.
- Godot Compatibility renderer for broad Android device support.

This is an early playable foundation, not a finished battle-royale game. The MOVE circle is currently a visual guide rather than a true analog joystick. Bots do not shoot yet, and the Godot port does not yet include loot, weapons pickup/drop, vehicles, parachute, safe zone or multiplayer.

See [GodotPort/PORT_STATUS.md](GodotPort/PORT_STATUS.md) for the detailed implementation and test status.

## Latest Android APK

The latest GitHub Actions build succeeded:

- Workflow run #10: [Godot Android APK Build](https://github.com/suhansarkerzzz-afk/Free-Fire-2017-Classic-Remastered/actions/runs/42046524963)
- Commit: `6955558aa166beea4c0880c808eda7bda306a75e`
- Godot import, headless runtime validation, Android export, debug signing and artifact upload passed.
- Artifact name: `FreeFire2017ClassicRemastered-Godot-Android-APK`
- [Download the APK artifact](https://github.com/suhansarkerzzz-afk/Free-Fire-2017-Classic-Remastered/actions/runs/42046524963/artifacts/13424856526)

The CI result does not replace testing on a physical Android phone. Landscape orientation, touch controls, gameplay and performance on 2 GB / 4 GB / 6 GB devices still need real-device verification.

## Build Android APK with GitHub Actions

1. Open repository **Actions**.
2. Select **Godot Android APK Build**.
3. Choose **Run workflow**, or push a change under `GodotPort/`.
4. When the run succeeds, download the artifact named `FreeFire2017ClassicRemastered-Godot-Android-APK`.

No Unity license or `UNITY_LICENSE` secret is required for the Godot workflow.

## Legacy Unity prototype (reference only)

The repository still contains the previous Unity implementation under `Assets/`, `Packages/` and `ProjectSettings/`. It includes prototypes for offline bots, weapons, loot, safe zone, vehicle, parachute, local Wi-Fi/hotspot and Bluetooth direct-link transport.

The Unity multiplayer code is incomplete: authoritative health/damage, shooting, loot, bot state, match phase, reconnect and host migration are not synchronized. Bluetooth currently exchanges basic player state only and still needs discoverability / device testing. It is not a complete synchronized battle-royale match.

The old `.github/workflows/android-apk.yml` workflow is for the legacy Unity project and requires a valid Unity license secret. Use `.github/workflows/godot-android-apk.yml` for the active Android APK.

## Testing status

Verified in GitHub Actions: Godot 4.4.1 project import, headless runtime validation, Android debug APK export, APK signing and artifact upload.

Not yet verified: APK installation and play on a physical phone, touch-control usability, real-device performance, full gameplay balance, Unity compilation, Wi-Fi multiplayer or Bluetooth multiplayer.
