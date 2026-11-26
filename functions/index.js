const { onCall, HttpsError } = require("firebase-functions/v2/https");
const { initializeApp } = require("firebase-admin/app");
const { getFirestore, FieldValue } = require("firebase-admin/firestore");

initializeApp();
const db = getFirestore();
const COLLECTION = "leaderboardIndonesia";
const MAX_SURVIVAL_SECONDS = 7200;
const MAX_SCORE_BONUS = 5000;

function cleanName(value) {
  const name = String(value || "Gecko").replace(/[^a-zA-Z0-9 _-]/g, "").trim();
  return name.slice(0, 16) || "Gecko";
}

function cleanAvatar(value) {
  const avatar = String(value || "classic");
  return /^[a-z0-9_-]{1,24}$/i.test(avatar) ? avatar : "classic";
}

exports.submitRun = onCall({ enforceAppCheck: true, region: "asia-southeast2" }, async (request) => {
  if (!request.auth) throw new HttpsError("unauthenticated", "Login diperlukan.");

  const uid = request.auth.uid;
  const score = Number(request.data?.score);
  const survivalSeconds = Number(request.data?.survivalSeconds);
  const displayName = cleanName(request.data?.displayName);
  const avatarId = cleanAvatar(request.data?.avatarId);

  if (!Number.isSafeInteger(score) || score < 0)
    throw new HttpsError("invalid-argument", "Score tidak valid.");
  if (!Number.isFinite(survivalSeconds) || survivalSeconds < 0 || survivalSeconds > MAX_SURVIVAL_SECONDS)
    throw new HttpsError("invalid-argument", "Durasi permainan tidak valid.");

  const maximumPlausibleScore = Math.floor(survivalSeconds * 10 + MAX_SCORE_BONUS);
  if (score > maximumPlausibleScore)
    throw new HttpsError("failed-precondition", "Score tidak lolos pemeriksaan.");

  const ref = db.collection(COLLECTION).doc(uid);
  const result = await db.runTransaction(async (tx) => {
    const snap = await tx.get(ref);
    const previous = snap.exists ? snap.data() : null;
    const now = Date.now();

    if (previous?.lastSubmitMs && now - previous.lastSubmitMs < 10000)
      throw new HttpsError("resource-exhausted", "Tunggu sebelum mengirim skor lagi.");

    const bestScore = Math.max(score, previous?.score || 0);
    const bestTime = score >= (previous?.score || 0) ? survivalSeconds : (previous?.survivalSeconds || 0);

    tx.set(ref, {
      uid,
      country: "ID",
      displayName,
      avatarId,
      score: bestScore,
      survivalSeconds: bestTime,
      updatedAt: FieldValue.serverTimestamp(),
      lastSubmitMs: now
    }, { merge: true });

    return { score: bestScore, isNewBest: !previous || score > (previous.score || 0) };
  });

  return result;
});

exports.getIndonesiaLeaderboard = onCall({ region: "asia-southeast2" }, async (request) => {
  const limit = Math.min(Math.max(Number(request.data?.limit) || 100, 1), 100);
  const top = await db.collection(COLLECTION)
    .orderBy("score", "desc")
    .orderBy("updatedAt", "asc")
    .limit(limit)
    .get();

  const entries = top.docs.map((doc, index) => {
    const data = doc.data();
    return {
      rank: index + 1,
      displayName: data.displayName || "Gecko",
      avatarId: data.avatarId || "classic",
      score: data.score || 0,
      survivalSeconds: data.survivalSeconds || 0
    };
  });

  let myRank = null;
  if (request.auth) {
    const mine = await db.collection(COLLECTION).doc(request.auth.uid).get();
    if (mine.exists) {
      const myScore = mine.data().score || 0;
      const ahead = await db.collection(COLLECTION).where("score", ">", myScore).count().get();
      myRank = ahead.data().count + 1;
    }
  }

  return { country: "ID", entries, myRank };
});