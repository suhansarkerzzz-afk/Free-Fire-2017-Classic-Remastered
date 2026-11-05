# Free Fire 2017 Classic Remastered

An original, fan-inspired battle-royale prototype with a 2017-era visual direction and lightweight procedural assets. This is not an official Garena product and does not use official Garena assets.

## Active Android implementation

The active Android build is the Godot 4.4.1 project in `GodotPort/`. The legacy Unity prototype remains in `Assets/` as reference code and is not part of the Godot APK.

The current Godot prototype includes a procedural low-poly island, roads, houses, trees, 12 district labels, third-person player movement, touch-drag movement/look, 200 HP, rifle ammo/reload, basic raycast shooting, HUD and 49 basic chase/melee bots.

The current Godot APK was successfully exported and uploaded by GitHub Actions run [#10](https://github.com/suhansarkerzzz-afk/Free-Fire-2017-Classic-Remastered/actions/runs/42046524963). The CI workflow imported the project, ran headless runtime validation, exported and signed the Android debug APK, and uploaded the artifact. Physical-device installation and gameplay have not yet been verified.

## Two offline / local-play design targets

### Offline Solo

- Target: one local human player against 49 local AI bots.
- Current Godot version: basic chase/melee bots; no bot gun combat yet.
- The island and POIs are procedural approximations, not a faithful recreation of the original map.
- Godot does not yet include the full plane/parachute, loot, medkits, weapon pickup/drop, safe-zone or vehicle gameplay.

### Local Wi-Fi / mobile Hotspot (legacy Unity prototype — BETA)

- Separate opt-in mode; Solo Offline does not open LAN sockets.
- One player creates a room and shares its six-character room code; friends join on the same Wi-Fi or phone hotspot.
- Uses local UDP discovery and basic player transform replication; no internet matchmaking or dedicated server.
- This remains an early networking prototype: authoritative health, damage, shooting, loot, bot state, match phase, reconnect and host migration are not synchronized.
- Bluetooth Classic RFCOMM is a separate optional 1-to-1 transport in the legacy Unity prototype. It exchanges player position, rotation and team state only; it is not complete Bluetooth gameplay and requires further discoverability and device testing.
- These local networking features have not been ported to Godot.

## Performance and build setup

- The Godot project uses lightweight procedural primitives and the Compatibility renderer.
- Android export supports ARMv7 and ARM64 and is configured for landscape.
- No measured 2 GB / 4 GB / 6 GB device performance results are available yet.
- The memory-aware graphics profile and Unity Android build helper belong to the legacy Unity prototype, not the active Godot APK.

## Current limitations

This is a prototype, not a finished commercial game. The Godot island is a flat procedural approximation, not the original Bermuda geometry. The Godot port still needs a real analog joystick, bot gun combat, weapon models, loot/inventory, medkits, parachute/plane, vehicle gameplay, safe zone, minimap, richer audio/animation and real-device performance tuning.

The legacy Unity prototype contains additional feature experiments, but they are not automatically present in the Godot APK. Its Wi-Fi and Bluetooth prototypes are not fully synchronized multiplayer.

## Open in Godot

1. Download Godot 4.4.1.
2. Clone or download this repository.
3. Import the `GodotPort/` folder in Godot.
4. Run the project or use the GitHub Actions Android workflow.

## Open the legacy prototype in Unity

1. Install Unity 2022.3.62f2 with Android Build Support, Android SDK & NDK Tools and OpenJDK.
2. Clone or download this repository.
3. Open the repository folder in Unity Hub.
4. Allow Unity to import the project.

The legacy Unity Android workflow still requires a valid `UNITY_LICENSE` secret. The active Godot Android workflow does not.

## Controls

- Godot desktop prototype: WASD move; Shift sprint; Space jump; F fire; R reload; right-mouse drag to look.
- Godot Android prototype: drag the left side to move; drag the right side to look; tap/hold the on-screen action buttons.
- Legacy Unity prototype: left joystick to move/steer; SPRINT and JUMP buttons; drag the right side to look; FIRE, RELOAD, SWAP, DROP, MEDKIT and DRIVE buttons.

## Planned improvements

- True mobile analog joystick and polished touch layout.
- Bot ranged combat, cover use and improved navigation.
- Better island terrain, coastline, roads and detailed POIs.
- Spawn island, airplane route and controllable parachute.
- Crouch/prone, scopes, recoil, weapon balancing and equipment.
- Full minimap and battle HUD.
- Loot, medkits, inventory and vehicle gameplay in Godot.
- Port and complete authoritative LAN combat/health/loot/match synchronization, reconnect and room lifecycle.
- Port Bluetooth RFCOMM transport to Godot only after the LAN match protocol is stable.
- Sound, animation and low-memory profiling on physical Android devices.
- Windows PC build after Android stabilizes.
