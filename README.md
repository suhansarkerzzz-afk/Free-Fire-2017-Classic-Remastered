# Free Fire 2017 Classic Remastered

An original, fan-inspired battle-royale prototype with a 2017-era visual direction and lightweight procedural assets. This is not an official Garena product and does not use official Garena assets.

## Two offline play options

### Offline Solo
- Works without internet and does not open LAN sockets.
- One local human player against 49 local AI bots.
- Procedural Bermuda-inspired island and named POI districts.
- Third-person movement, mobile joystick and touch camera; keyboard controls for development.
- Plane flyover/parachute prototype, weapons, loot, health, medkits, safe zone and simple vehicle.
- Local graphics settings and progression.

### Local Wi-Fi / mobile Hotspot (BETA)
- A separate opt-in menu mode; Solo Offline remains completely independent.
- One player creates a room and shares its six-character room code; friends join on the same Wi-Fi or phone hotspot.
- Uses local UDP discovery and player transform replication; no internet matchmaking or dedicated server.
- Current implementation is an early networking prototype: remote player movement/appearance is replicated, but authoritative health, damage, shooting, loot, bot state, match phase, reconnect and host migration are not synchronized. Do not describe this as finished multiplayer or a complete Mini Militia-style match yet.
- Bluetooth is not used for gameplay transport.

## Performance and build setup

- Lightweight procedural primitives; no remote asset downloads are required at runtime.
- Memory-aware graphics profile with a 30 FPS target on devices reporting 3 GB RAM or less.
- Android build helper and GitHub Actions workflow target Unity 2022.3.62f2.
- Android target is ARM64, landscape, minimum API 23.
- Android APK first; Windows PC build is a later milestone.

## Current limitations

This is a prototype, not a finished commercial game. The Bermuda layout and POIs are procedural approximations, not a faithful recreation of the original map. Plane/parachute, vehicle handling, bot navigation and combat balance need further development. Armor/helmet equipment, prone/crouch, scopes, full minimap and richer sound/animation are not yet complete. Local Wi-Fi multiplayer needs authoritative gameplay synchronization and device testing.

No Unity compilation, Play Mode, gameplay testing, Android APK generation, real-device performance testing or network testing has been run. Code has only been committed/read back from GitHub; successful build and runtime behaviour are not claimed.

## Open in Unity

1. Install Unity 2022.3.62f2 with Android Build Support, Android SDK & NDK Tools and OpenJDK.
2. Clone or download this repository.
3. Open the repository folder in Unity Hub.
4. Allow Unity to import the project.

## Android APK with GitHub Actions

1. Open repository Settings → Secrets and variables → Actions.
2. Add the secret named UNITY_LICENSE with the Unity license file contents for the account used to build.
3. Open Actions → Android APK Build.
4. Choose Run workflow.
5. When the run succeeds, download the Android APK artifact.

The workflow has not yet been run or verified. A valid Unity license secret and a successful Unity build are required before an APK can be downloaded.

## Controls

- PC prototype: WASD / arrow keys to move; Shift sprint; Space jump; right-mouse drag to look; R reload; Q switch weapon; G drop weapon; E enter/exit vehicle.
- Android prototype: left joystick to move/steer; SPRINT toggle and JUMP buttons; drag the right side to look; tap FIRE, RELOAD, SWAP, DROP, MEDKIT and DRIVE.

## Planned improvements

- Complete classic-style map terrain, coastline, roads and detailed POIs.
- Spawn island, full airplane route and controllable parachute.
- Crouch/prone, scopes, recoil and weapon balancing.
- Helmet/vest equipment and improved loot.
- Full minimap and battle HUD.
- Better bot navigation, cover use and low-memory optimisation for 2 GB phones.
- Finish authoritative LAN combat/health/loot/match synchronization, reconnect and room lifecycle.
- Complete sound, animation and match flow.
