using System;

namespace CapsuleWars.Persistence.Dto
{
    /// <summary>
    /// Cumulative stats for one legacy unit across all runs.
    /// Merged into at the end of each battle (or each run, in M9+).
    /// </summary>
    [Serializable]
    public class LifetimeStatsDTO
    {
        public int Kills;
        public int DamageDealt;
        public int DamageTaken;
        public int Faints;
        public int BattlesParticipated;
        public int RunsParticipated;

        /// <summary>Adds one battle's results into this lifetime total.</summary>
        public void MergeBattle(int damageDealt, int damageTaken, int kills, bool fainted)
        {
            DamageDealt += damageDealt;
            DamageTaken += damageTaken;
            Kills += kills;
            if (fainted) Faints++;
            BattlesParticipated++;
        }
    }
}
