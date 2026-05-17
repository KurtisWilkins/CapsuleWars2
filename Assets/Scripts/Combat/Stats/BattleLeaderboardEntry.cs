namespace CapsuleWars.Combat.Stats
{
    /// <summary>
    /// One row of the post-battle leaderboard. Built by
    /// <see cref="BattleStatsAggregator.BuildLeaderboard"/>.
    /// </summary>
    public readonly struct BattleLeaderboardEntry
    {
        public readonly string UnitId;
        public readonly string DisplayName;
        public readonly int DamageDealt;
        public readonly int DamageTaken;
        public readonly int Kills;

        public BattleLeaderboardEntry(string unitId, string displayName, int damageDealt, int damageTaken, int kills)
        {
            UnitId = unitId;
            DisplayName = displayName;
            DamageDealt = damageDealt;
            DamageTaken = damageTaken;
            Kills = kills;
        }
    }
}
