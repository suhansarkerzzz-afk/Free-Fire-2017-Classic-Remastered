# Free Fire 2017 Classic Remastered

An original, fan-inspired classic battle-royale prototype with a 2017-era visual direction and lightweight procedural assets. This is not an official Garena product and does not use official Garena assets.

## Implemented in the current codebase

### Core gameplay
- Runtime-generated main menu and graphics settings.
- Character selection with four original colour presets.
- Third-person player, procedural character model and mobile/keyboard movement.
- Touch camera look and virtual joystick.
- Rifle and pistol starter loadout, plus unlockable SMG, shotgun and sniper pickup definitions.
- Shooting, reload, weapon switching, ammo HUD and health system.
- Mobile FIRE, RELOAD, SWAP, MEDKIT, DRIVE and EMOTE controls.
- Procedural island village with houses, trees, rocks, cover, road, river, bridge, street lamps and supply crates.
- Squad affiliation for bot groups; bots avoid targeting members of their own squad.

### Offline battle-royale prototype
- Match bootstrap configured for one human player and 49 AI bots.
- Bot patrol, pursuit and basic line-of-sight combat.
- Initial plane flyover and parachute descent sequence.
- Shrinking safe-zone boundary and outside-zone damage.
- Ammo, medkit and weapon pickups.
- Inventory readout and medkit use.
- Alive count, player-attributed kill count, win/lose state, and an interactive result panel with Play Again / Main Menu actions.
- Basic local rank points, kill progression and match-result rewards.
- Simple vehicle model with enter/exit and keyboard/mobile joystick driving.
- Cosmetic pet companion and procedural emotes.

### Local network prototype
- Room-code generation and validation.
- UDP same-Wi-Fi room discovery, host/join flow and basic player transform synchronisation.
- Android network permissions are included in the custom manifest.
- This is an early LAN prototype, not internet matchmaking. Player transforms are synchronised, but authoritative combat, reconnection, and robust lobby management still need development.
- Optional Android Bluetooth device discovery is included as a separate nearby-device scan, with Android 12+ nearby-device permission handling. It does not carry gameplay traffic; Wi-Fi/hotspot remains the gameplay transport.
- Voice chat is intentionally out of scope.

### Performance and build setup
- Persistent Low / Balanced / High runtime graphics profiles with a memory-aware 30 FPS target on devices reporting 3 GB RAM or less.
- Bot targeting uses a shared actor registry; safe-zone damage applies to all living actors; kill credit is attributed to the actual last damage source.
- Android build helper and GitHub Actions workflow target Unity 2022.3.62f2.
- Android target is ARM64, landscape, IL2CPP, minimum API 23.
- Windows build remains a later milestone.

## Important status

The code has been committed to GitHub, but **Unity compilation, gameplay testing, Android APK generation, real-device performance testing and LAN testing have intentionally not been run yet**. Some systems are prototype implementations and may need changes after the first compile/test pass. In particular, LAN networking, vehicle handling, parachute landing, AI balance and 2 GB memory use require verification.

## Open in Unity

1. Install Unity 2022.3.62f2 with Android Build Support, Android SDK & NDK Tools and OpenJDK.
2. Clone or download this repository.
3. Open the repository folder in Unity Hub.
4. Allow Unity to import the project.
5. The menu is generated when a scene loads. The Bluetooth Java source plug-in is configured for Android only.

## Android APK with GitHub Actions

1. Open repository **Settings → Secrets and variables → Actions**.
2. Add the secret named UNITY_LICENSE with the Unity license file contents for the account used to build.
3. Open **Actions → Android APK Build**.
4. Choose **Run workflow**.
5. When the run succeeds, download the FreeFire2017ClassicRemastered-Android-APK artifact.

The workflow has not yet been run or verified. A valid Unity license secret and a successful Unity build are required before an APK can be downloaded.

## Controls

- PC prototype: WASD / arrow keys to move; right-mouse drag to look; R reload; Q switch weapon; E enter/exit vehicle.
- Android prototype: left joystick to move/steer; drag the right side to look; tap the on-screen action buttons.
- LAN room panel: create a room and share its six-character code, or enter a code to search on the same Wi-Fi/hotspot.

## Planned after the first compile pass

- Fix all compiler errors and gameplay bugs in the first compile pass.
- Improve bot-vs-bot combat, navigation and cover usage.
- Complete LAN reliability, reconnection, team play and network authority.
- Bluetooth discovery is a nearby-device helper only; no Bluetooth gameplay transport is planned.
- Voice chat is not planned in this project phase.
- Profile and optimise on actual 2 GB Android phones.
- Complete APK release build, then prepare Windows PC build.
