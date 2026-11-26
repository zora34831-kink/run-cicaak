# RUN CICAAK — Firebase Indonesia Leaderboard

The repository includes a Firebase Functions v2 backend and an optional Unity client adapter.

## Firebase project setup

1. Create a Firebase project and register the Android package ID used by the Unity project.
2. Enable **Authentication → Sign-in method → Anonymous**.
3. Create the Cloud Firestore database.
4. Install Firebase CLI and run `firebase login`.
5. From the repository root run `npm --prefix functions install`.
6. Deploy backend and rules with `firebase deploy --only functions,firestore:rules,firestore:indexes`.

The callable functions use region `asia-southeast2`:
- `submitRun`: accepts authenticated run submissions, forces country code `ID`, checks score against elapsed time, and keeps each player's best score.
- `getIndonesiaLeaderboard`: returns up to 100 ranked entries and the signed-in player's rank.

## Unity client

1. Import the Firebase Unity SDK packages **Firebase Auth** and **Firebase Functions** into the Unity project.
2. Add the Android Firebase config file `google-services.json` under `Assets`.
3. Add scripting define symbol `FIREBASE_UNITY_SDK` for the Android build target.
4. Enable Anonymous Authentication in Firebase.
5. Configure Firebase App Check for Android using Play Integrity. During editor development, configure the Firebase App Check debug provider and register its debug token in the Firebase console.
6. Add `FirebaseLeaderboardClient` to a persistent scene object.
7. Call `SubmitRunAsync` after a completed run and `GetTopAsync(100)` to load the national ranking.

## Important security notes

- Never put Firebase Admin service-account keys in the Unity client or repository.
- Firestore client writes to `leaderboardIndonesia` are denied; only trusted Cloud Functions write leaderboard entries.
- App Check is enforced on score submissions.
- The score check is a basic plausibility filter, not proof that a run is cheat-free. A production release should add stronger server-side telemetry and abuse monitoring.
- The leaderboard only stores a game nickname and selected in-game avatar ID; do not use real names or personal contact details.