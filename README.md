# Free Fire 2017 Classic Remastered

An original, fan-inspired offline battle-royale prototype with a 2017-era visual direction and lightweight procedural assets. This is not an official Garena product and does not use official Garena assets.

## Offline-first game scope

The primary game mode is fully offline and does not require an account, internet connection, online matchmaking, or a dedicated server.

- One local human player against 49 local AI bots.
- Procedural Bermuda-inspired island and named POI districts.
- Third-person movement, mobile joystick and touch camera; keyboard controls for development.
- Initial plane flyover and parachute descent sequence.
- Bot patrol, pursuit and basic line-of-sight combat.
- Rifle/pistol starter loadout, weapon pickups, shooting, reload, ammo and weapon switching.
- Health, medkits, inventory capacity, ammo and weapon loot.
- Shrinking safe zone and outside-zone damage.
- Simple vehicle prototype with enter/exit and driving.
- Alive count, player kill count, match win/lose and result panel.
- Local graphics settings: Low, Balanced and High.
- Local progression and match rewards.

## Offline-only menu and startup

The main menu exposes only offline play, graphics settings and character selection. The offline match startup does not launch LAN room discovery, Bluetooth discovery, online matchmaking, or companion/pet systems. Network-related prototype scripts remain in the repository for possible separate future work, but they are not started by the offline game.

## Performance and build setup

- Lightweight procedural primitives; no remote asset downloads are required at runtime.
- Memory-aware graphics profile with a 30 FPS target on devices reporting 3 GB RAM or less.
- Android build helper and GitHub Actions workflow target Unity 2022.3.62f2.
- Android target is ARM64, landscape, minimum API 23.
- Android APK first; Windows PC build is a later milestone.

## Current limitations

This is still a prototype, not a finished commercial game. The Bermuda layout and POIs are procedural approximations, not a faithful recreation of the original map. Plane/parachute, vehicle handling, bot navigation and combat balance need further development. Armor/helmet equipment, prone/crouch, scopes, full minimap and richer sound/animation are not yet complete.

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

## Planned offline improvements

- Complete classic-style map terrain, coastline, roads and detailed POIs.
- Spawn island, full airplane route and controllable parachute.
- Crouch/prone, scopes, recoil and weapon balancing.
- Helmet/vest equipment and improved loot.
- Full minimap and battle HUD.
- Better bot navigation, cover use and low-memory optimisation for 2 GB phones.
- Complete sound, animation and match flow.
