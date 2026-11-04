# Free Fire 2017 Classic Remastered

An original, fan-inspired low-poly battle-royale prototype. This is not an official Garena product and does not use official Garena assets.

## Current prototype
- Runtime-generated main menu and graphics quality selector.
- Start button opens a generated training arena.
- Third-person orbit camera with touch-drag look control.
- Mobile virtual joystick for movement.
- PC keyboard movement (WASD / arrow keys) and mouse drag camera look in the Editor/Standalone.
- Five simple training bots that follow and attack the player.
- Two selectable weapons: RIFLE and PISTOL, each with separate damage, range, fire rate and ammo.
- Separate magazine/reserve ammo and reload timing for each weapon, with ammo/weapon HUD and mobile RELOAD/SWAP buttons.
- Procedural low-poly rifle and pistol view models, firing recoil, brief muzzle flash and generated prototype shot sounds.
- Health and elimination HUD prototype.
- Unity UI package manifest and editor version metadata.

## Open in Unity
1. Install **Unity 2022.3 LTS** with the Android Build Support module (SDK, NDK and OpenJDK).
2. Clone or download this repository.
3. Open the repository folder in Unity Hub.
4. Let Unity import the project, then open any scene (the menu is created automatically after a scene loads).
5. Press Play to test the menu and training arena.

## Android APK
After running **Tools > Classic Remastered > Prepare Android Build**, install Android Build Support (SDK, NDK and OpenJDK) through Unity Hub. Select **File > Build Settings > Android** and click **Build** to create the APK. The repository includes an Editor setup helper, but no APK has been compiled or device-tested yet. Unity Editor and Android Build Support must be installed on a build machine.

## Build APK with GitHub Actions
1. In GitHub, open **Settings > Secrets and variables > Actions > New repository secret**.
2. Add a secret named `UNITY_LICENSE` containing the Unity license file contents for the Unity account used to build.
3. Open the repository's **Actions** tab and select **Android APK Build**.
4. Click **Run workflow** and wait for the job to finish.
5. Open the completed run and download the artifact named `FreeFire2017ClassicRemastered-Android-APK`.

The workflow runs Unity 2022.3.62f1 in Android mode and uploads the APK as a build artifact. A valid Unity license secret is required. If the workflow fails, open its build log and share the error so it can be fixed. This workflow has been added but has not yet been run or verified.

## Controls
- PC: WASD / arrow keys to move; right-mouse drag to look; press R to reload and Q to switch weapons.
- Android: use the left-side virtual joystick to move, drag on the right side to rotate the camera, tap FIRE to shoot, RELOAD to reload and SWAP to change weapon.
- UI buttons are excluded from camera-drag input.
- The current arena and bots are prototype placeholders, not a finished multiplayer game.

## Planned next milestones
1. Better arena, character visuals and bot behavior.
2. Lobby and local Wi-Fi multiplayer.
3. Performance profiling on 2 GB RAM Android devices.
4. Windows build after the Android prototype is stable.
