# Free Fire 2017 Classic Remastered

An original, fan-inspired battle-royale prototype with a classic 2017-era feel and lightweight procedural visuals. This is not an official Garena product and does not use official Garena assets.

## Active Android project

The active Android project is **Godot 4.4.1** in `GodotPort/`. The earlier Unity prototype is retained as reference code and is not part of the current Godot APK workflow. Android is the first target; Windows PC support comes later.

## Offline prototype currently written

- One human player plus **49 offline bots** (50 participants total).
- Procedural low-poly island with roads, houses, trees and 12 district labels.
- Third-person player, 200 HP, rifle magazine/reserve ammo, reload and raycast shooting.
- Bots with 200 HP, chase/separation movement, ranged hitscan attacks and close-range attacks.
- **New, not yet validated:** distant bot render meshes are culled beyond a 44 m visibility range (4 m margin); AI and collision remain active. Actual performance gains have not been measured.
- **New, not yet validated:** procedural houses, roofs and trees use a 76 m visual range while building collision remains; repeated props share materials by color to reduce duplicate resource allocations. Actual memory/FPS gains have not been measured.
- Landscape mobile controls: left-side drag movement with a visual joystick knob, right-side drag camera look, and FIRE / RELOAD / JUMP / SPRINT / MEDKIT buttons.
- **New, not yet validated:** HUD controls and labels are positioned using display safe-area insets and recalculate after viewport resizing to reduce overlap with phone cutouts.
- Ammo and medkit pickups, medkit inventory counter and healing.
- Player-death Game Over panel and Play Again scene restart.
- A shrinking safe-zone controller has now been added to the Godot match startup. It begins with a 45-second grace period, contracts over 180 seconds and damages the player/bots outside its boundary. This latest safe-zone code has **not** been run or validated yet.
- **New, not yet validated:** the main menu is now the first screen; a match starts only when the player taps Offline Solo. The menu panel scales to fit smaller landscape viewports.
- **New, not yet validated:** selectable Low (2 GB), Medium (4 GB) and High (6 GB) graphics profiles. Low is the default; profiles adjust bot/scenery visual distances and 3D MSAA. RAM labels are guidance only, not automatic detection, and performance has not been measured.

This is still an early offline prototype, not a finished battle royale.

## Not implemented in the active Godot port

- Vehicles, parachute deployment, pets, selectable character skins and custom rooms.
- LAN/Wi-Fi/hotspot gameplay networking and synchronized multiplayer.
- Optional Bluetooth device discovery (discovery only; Bluetooth is not the gameplay transport).
- Measured performance or compatibility on physical 2 GB / 4 GB / 6 GB phones.

**Voice chat is out of scope and must not be added.** Local multiplayer will be considered only after the offline game is stable. The initial target remains one human plus bots.

## Build and verification gate

The most recent recorded successful Android CI run before the safe-zone change was [Godot Android APK Build #18](https://github.com/suhansarkerzzz-afk/Free-Fire-2017-Classic-Remastered/actions/runs/42053639599), on commit `85ba2032145bc8a03f46b051ff88bf96fcea8fb3`. It does **not** validate the newer safe-zone code.

To respect the current coding-first phase, the Godot workflow is now **manual-only**. No build, gameplay test or phone test should be started until the user says coding is complete or explicitly authorizes testing. When authorized, open repository Actions, select **Godot Android APK Build**, and choose **Run workflow**.

Godot Android export does not require a Unity license. The legacy Unity workflow is separate and is not used for the active Android project.

## Verification limits

Still unverified on a physical phone: installation, touch-control usability, landscape safe-area layout, gameplay balance, frame rate, memory use and battery impact. The 2 GB device target must be measured rather than assumed.
