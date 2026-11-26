# RUN CICAAK — Unity setup

This repository contains the first gameplay foundation. It is source code, not a finished APK.

## Recommended project
- Unity 2022.3 LTS or newer LTS
- 3D Core (URP optional)
- Android Build Support, Android SDK & NDK Tools, and OpenJDK

## Add the scripts
Copy the C# files from `Assets/Scripts` into a Unity project's matching folder.

## First playable scene
1. Create a floor and several walls using 3D cubes.
2. Create a small gecko placeholder (capsule is fine), add a CharacterController and PlayerController.
3. Add a camera and assign its Transform to PlayerController.cameraTransform.
4. Create a human placeholder, add HumanChaserAI, then assign the gecko Transform as target.
5. Add an empty GameObject named GameManager and attach GameManager.cs.
6. Create hiding props. Add HidingSpot.cs and optionally assign a child Transform as hidePosition.
7. Add UI buttons and connect their OnClick/Pointer events to SetMoveInput, SetSprint, RequestJump, and ToggleHide. A joystick UI should call SetMoveInput continuously and reset it to Vector2.zero when released.

## Current limitations
- Placeholder gameplay foundation only; art, animation, audio, room layouts, menus, shop, achievements UI, Firebase sign-in, and Indonesia leaderboard still need implementation.
- Score persistence is local only. Never trust client-submitted leaderboard scores; validate submissions on a trusted backend.
- The human AI is a basic prototype and does not yet navigate around furniture.
