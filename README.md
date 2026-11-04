# Free Fire 2017 Classic Remastered

An original, fan-inspired low-poly battle-royale prototype. This is not an official Garena product and does not use official Garena assets.

## Current prototype
- Runtime-generated main menu and graphics quality selector.
- Start button opens a generated training arena.
- Third-person follow camera.
- CharacterController movement (WASD / arrow keys).
- On-screen touch movement buttons for Android testing.
- Five simple training bots that follow and attack the player.\n- Basic raycast shooting with a mobile FIRE button.\n- Health and elimination HUD prototype.
- Unity UI package manifest and editor version metadata.

## Open in Unity
1. Install **Unity 2022.3 LTS** with the Android Build Support module (SDK, NDK and OpenJDK).
2. Clone or download this repository.
3. Open the repository folder in Unity Hub.
4. Let Unity import the project, then open any scene (the menu is created automatically after a scene loads).
5. Press Play to test the menu and training arena.

## Android APK
In Unity, select **File > Build Settings > Android > Switch Platform**. Set a package identifier under Player Settings, choose ARM64, then use **Build** to create an APK. The project has not yet been built or device-tested; configure Android SDK/NDK through Unity Hub first.

## Controls
- PC: WASD or arrow keys.
- Android: touch the LEFT / RIGHT / FORWARD / BACK buttons.
- The current arena and bots are prototype placeholders, not a finished multiplayer game.

## Planned next milestones
1. Proper joystick and camera-look touch controls.
2. Health, weapon, shooting and bot combat systems.
3. Lobby and local Wi-Fi multiplayer.
4. Performance profiling on 2 GB RAM Android devices.
5. Windows build after the Android prototype is stable.
