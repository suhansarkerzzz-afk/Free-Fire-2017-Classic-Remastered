# Godot Port — Development Status

This is the active Android project, built with Godot 4.4.1 alongside the retained legacy Unity prototype. The Unity files are reference code and are not included in the Godot Android APK.

## Implemented prototype foundation

- Godot 4.4.1 project using the Compatibility renderer.
- Procedural low-poly island ground, two crossing roads, simple houses, trees and 12 district labels.
- Third-person player with keyboard movement for desktop testing and touch-drag movement/look on Android.
- Player health (200 HP), 30-round rifle magazine, reserve ammo, reload and raycast shooting.
- 49 basic villain bots, each with 200 HP, chase movement and close-range attacks.
- HUD for health, ammo, enemy count and match status.
- Landscape Android export preset for ARMv7 and ARM64.
- GitHub Actions workflow for a debug APK; no Unity license secret is needed.

## Verified Android CI build

- Workflow run: [#10 — Godot Android APK Build](https://github.com/suhansarkerzzz-afk/Free-Fire-2017-Classic-Remastered/actions/runs/42046524963)
- Commit: `6955558aa166beea4c0880c808eda7bda306a75e`
- Result: success — Godot import, headless runtime validation, APK export, signing and artifact upload all completed.
- Artifact: `FreeFire2017ClassicRemastered-Godot-Android-APK` (about 53.8 MB).
- [Download the APK artifact](https://github.com/suhansarkerzzz-afk/Free-Fire-2017-Classic-Remastered/actions/runs/42046524963/artifacts/13424856526)

CI success confirms project import, script validation, export and packaging. It does not confirm installation, touch behaviour, performance or gameplay on a physical phone. Those still require device testing.

## Known limitations and next work

- The current MOVE circle is a visual guide, not a movable analog joystick; movement uses left-side touch dragging.
- Bots do not use guns yet; they chase and attack at close range.
- No visible weapon model, weapon pickup/drop system, medkits or loot in the Godot port.
- The island is a flat procedural prototype, not the full Bermuda map or its faithful terrain/POIs.
- No plane/parachute, jeep, shrinking safe zone, minimap, animation or finished audio.
- No Wi-Fi/hotspot room discovery, Bluetooth transport or multiplayer synchronization in the Godot port.
- No measured 2 GB / 4 GB / 6 GB device performance results yet.
- The export log reports that no project icon is configured; this is non-fatal, and the APK still exported successfully.

## Controls

- Desktop: WASD move, Shift sprint, Space jump, F fire, R reload, right-mouse drag to look.
- Android: drag on the left side to move, drag on the right side to look, and use FIRE / RELOAD / JUMP / SPRINT buttons.

## Build

The workflow runs on pushes affecting `GodotPort/**` or its workflow file, and can also be started manually from GitHub Actions. The uploaded debug artifact is retained for 14 days.
