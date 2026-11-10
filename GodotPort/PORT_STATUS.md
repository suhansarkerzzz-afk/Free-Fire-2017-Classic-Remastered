# Godot Port — Development Status

This is the active Android project, built with Godot 4.4.1 and the Compatibility renderer. The previous Unity prototype remains in the repository as reference code and is not included in the Godot Android APK.

## Implemented in source

- Procedural low-poly island ground, roads, houses, trees and 12 district labels.
- Third-person player with desktop keyboard input and Android touch-drag movement/look.
- Player health (200 HP), 30-round rifle magazine, reserve ammo, reload and raycast shooting.
- 49 offline bots (50 participants including the player), each with 200 HP.
- Bot chase movement, separation, ranged hitscan rifle attacks and close-range attacks.
- **New, not yet validated:** neighbour-separation scans are staggered and refreshed about 5 times per second per bot instead of running on every physics frame, reducing repeated 49-bot proximity work.
- **New, not yet validated:** bot render meshes use a 44 m visibility range with a 4 m margin. This culls distant mesh parts only; bot AI, CharacterBody3D movement and collision remain active. The actual draw-call/FPS benefit has not been measured.
- HUD for health, ammo, enemy count and match status.
- **New, not yet validated:** match HUD positions are adjusted using Android/display safe-area insets. Top labels, centered status text, touch buttons and joystick are moved away from cutouts, and layout recalculates when the viewport size changes.
- Mobile joystick knob and FIRE / RELOAD / JUMP / SPRINT / MEDKIT buttons.
- Ammo and medkit pickups, medkit counter and healing.
- Player-death Game Over panel and Play Again scene restart.
- **New, not yet validated:** shrinking safe-zone controller. It waits 45 seconds, contracts from 52 m to 8 m over 180 seconds, draws a boundary ring and deals 8 damage every 2 seconds to the player/bots outside it.
- **New, not yet validated:** match HUD reports the safe-zone countdown, shrink percentage and final radius. The HUD polls the controller four times per second rather than every rendered frame.

## Last recorded CI result

- [Godot Android APK Build #18](https://github.com/suhansarkerzzz-afk/Free-Fire-2017-Classic-Remastered/actions/runs/42053639599) succeeded on commit `85ba2032145bc8a03f46b051ff88bf96fcea8fb3`.
- That run predates the safe-zone addition and must not be treated as validation of the current source.
- No CI build, Godot runtime test, APK export or phone test has been started for the safe-zone or bot-scan optimization changes.

## Remaining work

- Review the safe-zone implementation after the coding phase and validate its visual boundary, timing and damage behaviour.
- Validate the new safe-area HUD positioning and tune scaling across phone aspect ratios and cutout layouts.
- Improve character outfits, weapon holding pose, hit/death animations, weapon models, effects and audio.
- Add more detailed terrain, shoreline, water, height variation and recognizable classic-inspired points of interest.
- Extend visibility/LOD to static island scenery and add selectable Low (2 GB), Medium (4 GB) and High (6 GB) profiles; measure actual FPS and RAM use on physical devices.
- Add vehicles, parachute, pets, skins and custom rooms.
- Add LAN/Wi-Fi/hotspot multiplayer only after offline gameplay is stable. Bluetooth may be used for optional device discovery only, not as gameplay transport.
- Voice chat is explicitly out of scope.

## Controls

- Desktop: WASD move, Shift sprint, Space jump, F fire, R reload, right-mouse drag to look.
- Android: drag on the left side to move, drag on the right side to look, and use FIRE / RELOAD / JUMP / SPRINT / MEDKIT buttons.

## Build policy during coding

The GitHub Actions workflow remains available through manual `workflow_dispatch` only. Do not start validation, APK export, gameplay tests or phone tests until the user confirms that coding is complete or explicitly asks to test.
