using System;

[Serializable]
public class IndonesiaLeaderboardEntry
{
    public int rank;
    public string displayName;
    public string avatarId;
    public int score;
    public float survivalSeconds;
}

[Serializable]
public class IndonesiaLeaderboardSnapshot
{
    public string country = "ID";
    public int? myRank;
    public IndonesiaLeaderboardEntry[] entries = new IndonesiaLeaderboardEntry[0];
}