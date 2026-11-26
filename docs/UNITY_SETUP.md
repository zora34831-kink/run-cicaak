# RUN CICAAK — Unity setup and build

The repository now contains a Unity project scaffold, runtime prototype blockout, gameplay scripts, and an Editor build script. It is still a prototype, not a finished release APK.

## Requirements

- Unity 2022.3 LTS
- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

## Open and generate scenes

1. Clone this repository.
2. Open the repository root in Unity Hub.
3. Wait for the editor to import the UGUI package and compile scripts.
4. In the Unity menu choose RUN CICAAK → Generate Scenes.
5. Open Assets/Scenes/MainMenu.unity or press Play to test.

The scene generator creates:
- MainMenu.unity with Play, Character, Shop, High Score, and Settings pages.
- Game.unity with the runtime-generated four-room house blockout.

## Test controls

The generated game scene creates a touch joystick and SPRINT, JUMP, HIDE, and PAUSE buttons. In the Unity Editor, keyboard controls are also available:
- WASD / arrow keys: move
- Left Shift: sprint
- Space: jump
- E: hide when near a hiding spot

## Build APK locally

Choose RUN CICAAK → Build Android APK. The output path is Build/RUN-CICAAK.apk. The build script targets Android ARM64, landscape orientation, and package ID com.zora.runcicaak.

## GitHub Actions

The workflow .github/workflows/android-apk.yml builds the APK on manual dispatch. Add these Unity license secrets under repository Settings → Secrets and variables → Actions before running:
- UNITY_LICENSE
- UNITY_EMAIL
- UNITY_PASSWORD

The APK is published as the RUN-CICAAK-Android-APK workflow artifact after a successful build. No APK has been verified until a successful workflow run produces that artifact.

## Firebase

Follow docs/FIREBASE_LEADERBOARD.md to configure Firebase Authentication, App Check, Firestore, and Cloud Functions. Import Firebase Auth and Functions Unity SDK packages and define FIREBASE_UNITY_SDK for the Android target to enable the client adapter.

## Prototype limitations

- Gecko and human are primitive placeholders, not final 3D models.
- House rooms are generated from primitive cubes; final art, props, lighting, animation and sound are not yet included.
- Human obstacle avoidance is a basic steering prototype, not full pathfinding.
- Firebase project setup and Android config file are not included.
- The server performs plausibility checks but does not provide complete anti-cheat guarantees.
- Device testing and release signing remain necessary.
