# Free Fire 2017 Classic Remastered — Development Task List

The active Android project is the Godot 4.4.1 port in `GodotPort/`. Keep the classic 2017 feel, Android first, Windows PC later, and design for 2 GB devices before raising visual settings for 4 GB / 6 GB devices.

## Source implementation status

- [x] Godot Compatibility-renderer project and landscape Android export preset.
- [x] Third-person player movement, camera, health, rifle ammo, reload and raycast shooting.
- [x] 49 offline bots plus one human player (50 participants total).
- [x] Bot spawn distribution, separation, melee attacks and ranged hitscan attacks.
- [x] Source-level optimization: stagger bot neighbour-separation scans to reduce repeated per-frame work. **Not yet validated in Godot or on a phone.**
- [x] Initial bot visual LOD: cull distant bot mesh parts beyond the configured visibility range while keeping AI and collision active. **Not yet validated or benchmarked.**
- [x] Static scenery visibility range for procedural houses, roofs and trees; building collision remains enabled. **Not yet validated or benchmarked.**
- [x] Share procedural `StandardMaterial3D` resources by color to avoid duplicate material allocations. **Not yet validated or memory-profiled.**
- [x] Low-poly humanoid player/bot visuals.
- [x] Mobile touch-drag movement/look, joystick knob and match-over/restart UI.
- [x] Ammo crates, medkit pickups, medkit counter and healing button.
- [x] Shrinking safe-zone source code connected to match startup (45-second wait, 180-second contraction, outside-zone damage). **Not yet validated.**
- [x] Safe-zone HUD status text for countdown, shrink percentage and final radius. **Not yet validated.**
- [x] Add source-level safe-area-aware HUD positioning for display cutouts and viewport resizing. **Not yet validated on a device.**

## Next coding priorities

- [ ] Improve safe-zone feedback and review edge cases after testing is authorized.
- [ ] Validate safe-area HUD positioning and tune scaling across phone aspect ratios, cutouts and landscape devices.
- [ ] Add selectable Low (2 GB), Medium (4 GB), High (6 GB) graphics profiles and define measurable per-profile rendering budgets.
- [ ] Improve island terrain, shoreline, water, elevation and recognizable classic-inspired POIs.
- [ ] Extend LOD/visibility to static island scenery and add Low (2 GB), Medium (4 GB), High (6 GB) graphics profiles.
- [ ] Add vehicles and vehicle interaction.
- [ ] Add plane/parachute deployment, pets, character skins and custom rooms.
- [ ] Add LAN/Wi-Fi/hotspot multiplayer after offline gameplay is stable.
- [ ] Optional Bluetooth device discovery only; it must not be used as gameplay transport.
- [ ] Prepare Windows PC build after Android gameplay is stable.
- [ ] Voice chat is out of scope and must not be added.

## Verification gate

- The last recorded successful CI build was Godot Android APK Build #18 on commit `85ba2032145bc8a03f46b051ff88bf96fcea8fb3`; it predates the safe-zone code.
- The Godot workflow is manual-only during the coding phase.
- Do **not** run Unity compilation, Godot validation, gameplay tests, APK builds or phone tests until the user says the coding phase is complete or explicitly authorizes testing.
- When authorized, run the Godot Android workflow manually, inspect all logs/warnings, then verify installation, controls, gameplay, FPS and RAM on physical devices. Never claim phone or 2 GB performance testing without actual device results.
