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
- [x] Add selectable Low (2 GB), Medium (4 GB), High (6 GB) graphics profiles; Low is the default and the selected profile is saved to `user://settings.cfg`. RAM labels are manual guidance, not automatic device detection. **Not yet validated on a device.**
- [x] Add four original palette-based player outfits with lobby selection, torso/arm/leg colors, a simple helmet/chest accent and saved selection in `user://settings.cfg`. **Source only; not yet validated and not a full model-based skin inventory.**
- [x] Add a lightweight procedural Jeep prototype with keyboard/touch-joystick driving, nearby enter/exit interaction and a mobile DRIVE/EXIT button. **Source only; not yet validated, and it is not a complete vehicle system.**
- [x] Add source-level Jeep health and collision damage: 500 HP, horizontal impact threshold 5 m/s, up to 60 damage per hit with a 0.85-second cooldown, destroyed-state lockout/driver exit and an in-vehicle health HUD. **Not yet validated or balanced.**
- [x] Add source-level transport-plane route, player jump, parachute deployment/steering and landing transition. **Not yet validated.**
- [x] Add speed-based damage for hard unassisted freefall landings above 9.5 m/s (maximum 140 HP); parachute landings bypass this damage path. **Source only; not yet validated or balanced.**
- [x] Add four original palette-based player outfit choices and persist the selected outfit. **Not yet validated; not full model-based skins.**
- [x] Add three lightweight procedural pet companion choices with saved selection and a visual follow/hide prototype. **Source only; no navigation, collision, combat or multiplayer sync; not yet validated.**
- [x] Add a lightweight procedural rifle visual with camera-pivot attachment, player holding pose, recoil, muzzle flash and magazine reload movement. **Source only; not a full rigged animation system and not yet validated.**

## Next coding priorities

- [ ] Review and validate safe-zone timing, boundary visuals and damage after testing is authorized.
- [ ] Validate safe-area HUD positioning and tune scaling across phone aspect ratios, cutouts and landscape devices.
- [ ] Improve island terrain, shoreline, water, elevation and recognizable classic-inspired POIs.
- [ ] Tune Low / Medium / High rendering budgets using measured FPS and memory results after the coding phase is complete and testing is authorized.
- [ ] Review and tune Jeep handling, collision damage, camera/seat position and mobile interaction after testing is authorized; then add additional vehicle types.
- [ ] Validate the transport plane, parachute controls, landing collision and fall-damage balance after testing is authorized.
- [ ] Add full 3D character models, model-based skin inventory, weapon holding/run/fire/reload animations, hit reactions and death animations.
- [ ] Improve pet obstacle navigation and follower spacing after the basic prototype is validated.
- [ ] Design and implement custom-room lobby and room-code flow after offline match state is stable.
- [ ] Add LAN/Wi-Fi/hotspot multiplayer after offline gameplay is stable; keep gameplay transport on Wi-Fi/hotspot.
- [ ] Optional Bluetooth device discovery only; it must not be used as gameplay transport.
- [ ] Prepare Windows PC build after Android gameplay is stable.
- [ ] Voice chat is out of scope and must not be added.

## Verification gate

- The last recorded successful CI build was Godot Android APK Build #18 on commit `85ba2032145bc8a03f46b051ff88bf96fcea8fb3`; it predates the newer safe-zone, airdrop and cosmetic/weapon source.
- The Godot workflow is manual-only during the coding phase.
- Do **not** run Unity compilation, Godot validation, gameplay tests, APK builds or phone tests until the user says the coding phase is complete or explicitly authorizes testing.
- When authorized, run the Godot Android workflow manually, inspect all logs/warnings, then verify installation, controls, gameplay, FPS and RAM on physical devices. Never claim phone or 2 GB performance testing without actual device results.
