# Free Fire 2017 Classic Remastered — Development Task List

This is the active work plan for the Godot 4.4.1 Android prototype. Work from the top down. Keep Android first, preserve the classic 2017 feel, and optimize for 2 GB devices before enabling higher visual settings on 4 GB and 6 GB devices.

## Current status
- [x] Godot Android debug APK workflow completes successfully (Build #10 and Build #13).
- [x] Landscape orientation configured.
- [x] Player movement, camera, health, rifle ammo, reload and raycast shooting foundation.
- [x] 49 offline bots spawn.
- [x] Bot spawn points spread across the island.
- [x] Bot separation and melee attack-pressure limit added.
- [x] Capsule-only player/bot visuals replaced with simple low-poly humanoid parts (Build #14 succeeded).
- [ ] Confirm Build #15 (joystick + match-over UI) and inspect all Godot warnings/errors.
- [ ] Fix phone viewport/safe-area layout and HUD scaling; test on a real landscape phone.
- [x] Add a visual mobile analog joystick knob that follows left-side touch drag (Build #15 pending); improve anchoring/sensitivity after phone testing.
- [x] Add player-death Game Over panel and Play Again scene restart (Build #15 pending); add pause/settings menu later.
- [ ] Improve character visuals: modular outfits, weapon holding pose, hit/death animations.
- [ ] Add optimized weapon models, muzzle flash, impact effects and sound.
- [ ] Add loot pickups, ammo, medkits and inventory.
- [ ] Build a more detailed classic-inspired island with terrain height, shoreline, water, roads and recognizable POIs.
- [ ] Add LOD, visibility ranges, texture compression/atlases and baked lighting.
- [ ] Add graphics presets: Low (2 GB), Medium (4 GB), High (6 GB); profile actual RAM/FPS on devices.
- [ ] Add vehicles and safe-zone/match phase.
- [x] Add basic ranged bot rifle hitscan (this commit); [ ] add cover seeking, patrol and difficulty tuning.
- [ ] Add LAN/hotspot multiplayer only after stable offline gameplay; Bluetooth is a later optional transport.
- [ ] Produce signed release APK and document install/test results.

## Rules for every change
1. Inspect the existing implementation before editing.
2. Make small, isolated changes; do not overwrite unrelated working features.
3. Run the GitHub Actions Android build after code changes.
4. If a build fails, inspect the failed job logs, fix the root cause, and rebuild.
5. Never mark phone installation, touch controls, FPS, or RAM performance as tested until verified on a physical device.
