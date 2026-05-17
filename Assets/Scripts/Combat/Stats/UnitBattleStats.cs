namespace CapsuleWars.Combat.Stats
{
    /// <summary>
    /// Per-unit counters accumulated during one battle. Reset every battle.
    /// In M8 a per-run aggregator merges these into <c>LegacyUnitProfile</c>
    /// lifetime totals for legacy units.
    /// </summary>
    public class UnitBattleStats
    {
        public string UnitId;
        public string DisplayName;

        public int DamageDealt;
        public int AttackCountDealt;
        public int DamageTaken;
        public int AttackCountTaken;
        public int Kills;
        public int Faints;

        public void RecordDamageDealt(int amount)
        {
            DamageDealt += amount;
            AttackCountDealt++;
        }

        public void RecordDamageTaken(int amount)
        {
            DamageTaken += amount;
            AttackCountTaken++;
        }

        public void RecordKill()
        {
            Kills++;
        }

        public void RecordFaint()
        {
            Faints++;
        }
    }
}
