# RUN CICAAK

RUN CICAAK is a third-person Android gecko escape game prototype built with Unity.

## Current repository contents

- Unity 2022.3 LTS project scaffold.
- Runtime-generated four-room house blockout (living room, kitchen, bedroom, storage).
- Gecko movement, jump, sprint, hiding and mobile touch controls.
- Human chase prototype with basic obstacle steering and procedural character motion.
- Collectible coins, persistent wallet, score, survival timer, XP and levels.
- Gecko skin selection and shop logic.
- Escape/caught result screen and local achievements.
- Firebase Functions v2 backend contract for an Indonesia-only top-100 leaderboard.
- Unity Editor scene generator and Android APK build method.
- GitHub Actions workflow for manual Android APK builds.

## Open in Unity

1. Install Unity 2022.3 LTS with Android Build Support, SDK & NDK Tools, and OpenJDK.
2. Clone this repository and open the project folder in Unity Hub.
3. Wait for package import and script compilation.
4. Use RUN CICAAK → Generate Scenes from the Unity menu.
5. Press Play to test the generated blockout in the Editor.

## Build Android APK locally

Use RUN CICAAK → Build Android APK from the Unity menu. The APK output is Build/RUN-CICAAK.apk.

## GitHub Actions build

The workflow is .github/workflows/android-apk.yml. Before running it, add Unity license secrets to the repository:
- UNITY_LICENSE
- UNITY_EMAIL
- UNITY_PASSWORD

Then open Actions → RUN CICAAK Android APK → Run workflow. The APK is uploaded as a workflow artifact if the build succeeds.

## Firebase leaderboard

See docs/FIREBASE_LEADERBOARD.md. The Unity Firebase SDK packages and Firebase Android configuration are external setup requirements; do not commit service-account keys.

## Important status

The generated house and characters are primitive blockout objects. This repository is not yet a finished commercial-quality game: final 3D models, animation, sound, visual polish, device testing, Firebase project configuration, and a verified APK build remain outstanding.
