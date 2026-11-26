const { onCall, HttpsError } = require("firebase-functions/v2/https");
const { initializeApp } = require("firebase-admin/app");
const { getFirestore, FieldValue } = require("firebase-admin/firestore");

initializeApp();
const db = getFirestore();
const COLLECTION = "leaderboardIndonesia";
const WEEKLY_COLLECTION = "leaderboardIndonesiaWeekly";
const MONTHLY_COLLECTION = "leaderboardIndonesiaMonthly";
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

function getIndonesiaPeriodKeys() {
  const parts = new Intl.DateTimeFormat("en-CA", {
    timeZone: "Asia/Jakarta",
    year: "numeric",
    month: "2-digit",
    day: "2-digit"
  }).formatToParts(new Date());

  const values = Object.fromEntries(parts.map((part) => [part.type, part.value]));
  const year = Number(values.year);
  const month = Number(values.month);
  const day = Number(values.day);

  const localDate = new Date(Date.UTC(year, month - 1, day));
  const weekday = localDate.getUTCDay() || 7;
  const monday = new Date(localDate);
  monday.setUTCDate(localDate.getUTCDate() - weekday + 1);

  const thursday = new Date(monday);
  thursday.setUTCDate(monday.getUTCDate() + 3);
  const weekYear = thursday.getUTCFullYear();

  const jan4 = new Date(Date.UTC(weekYear, 0, 4));
  const jan4Weekday = jan4.getUTCDay() || 7;
  const firstMonday = new Date(jan4);
  firstMonday.setUTCDate(jan4.getUTCDate() - jan4Weekday + 1);
  const weekNumber = 1 + Math.round((monday - firstMonday) / 604800000);

  return {
    week: weekYear + "-W" + String(weekNumber).padStart(2, "0"),
    month: year + "-" + String(month).padStart(2, "0")
  };
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

  const periods = getIndonesiaPeriodKeys();
  const allTimeRef = db.collection(COLLECTION).doc(uid);
  const weeklyRef = db.collection(WEEKLY_COLLECTION).doc(periods.week + "_" + uid);
  const monthlyRef = db.collection(MONTHLY_COLLECTION).doc(periods.month + "_" + uid);

  const result = await db.runTransaction(async (tx) => {
    const snapshots = await tx.getAll(allTimeRef, weeklyRef, monthlyRef);
    const previousAllTime = snapshots[0].exists ? snapshots[0].data() : null;
    const previousWeekly = snapshots[1].exists ? snapshots[1].data() : null;
    const previousMonthly = snapshots[2].exists ? snapshots[2].data() : null;
    const now = Date.now();

    if (previousAllTime?.lastSubmitMs && now - previousAllTime.lastSubmitMs < 10000)
      throw new HttpsError("resource-exhausted", "Tunggu sebelum mengirim skor lagi.");

    const base = {
      uid,
      country: "ID",
      displayName,
      avatarId,
      survivalSeconds,
      updatedAt: FieldValue.serverTimestamp()
    };

    const allTimeScore = Math.max(score, previousAllTime?.score || 0);
    const weeklyScore = Math.max(score, previousWeekly?.score || 0);
    const monthlyScore = Math.max(score, previousMonthly?.score || 0);

    tx.set(allTimeRef, {
      ...base,
      score: allTimeScore,
      survivalSeconds: score >= (previousAllTime?.score || 0) ? survivalSeconds : (previousAllTime?.survivalSeconds || 0),
      lastSubmitMs: now
    }, { merge: true });

    tx.set(weeklyRef, {
      ...base,
      bucket: periods.week,
      score: weeklyScore,
      survivalSeconds: score >= (previousWeekly?.score || 0) ? survivalSeconds : (previousWeekly?.survivalSeconds || 0)
    }, { merge: true });

    tx.set(monthlyRef, {
      ...base,
      bucket: periods.month,
      score: monthlyScore,
      survivalSeconds: score >= (previousMonthly?.score || 0) ? survivalSeconds : (previousMonthly?.survivalSeconds || 0)
    }, { merge: true });

    return {
      score: allTimeScore,
      weeklyScore,
      monthlyScore,
      isNewBest: !previousAllTime || score > (previousAllTime.score || 0)
    };
  });

  return result;
});

exports.getIndonesiaLeaderboard = onCall({ region: "asia-southeast2" }, async (request) => {
  const limit = Math.min(Math.max(Number(request.data?.limit) || 100, 1), 100);
  const period = ["all", "weekly", "monthly"].includes(request.data?.period) ? request.data.period : "all";
  const periods = getIndonesiaPeriodKeys();

  let collectionName = COLLECTION;
  let bucket = null;
  let myDocumentId = request.auth?.uid || null;

  if (period === "weekly") {
    collectionName = WEEKLY_COLLECTION;
    bucket = periods.week;
    myDocumentId = request.auth ? bucket + "_" + request.auth.uid : null;
  } else if (period === "monthly") {
    collectionName = MONTHLY_COLLECTION;
    bucket = periods.month;
    myDocumentId = request.auth ? bucket + "_" + request.auth.uid : null;
  }

  let query = db.collection(collectionName);
  if (bucket) query = query.where("bucket", "==", bucket);

  const top = await query
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
  if (request.auth && myDocumentId) {
    const mine = await db.collection(collectionName).doc(myDocumentId).get();
    if (mine.exists) {
      let aheadQuery = db.collection(collectionName);
      if (bucket) aheadQuery = aheadQuery.where("bucket", "==", bucket);
      const ahead = await aheadQuery.where("score", ">", mine.data().score || 0).count().get();
      myRank = ahead.data().count + 1;
    }
  }

  return { country: "ID", period, entries, myRank };
});