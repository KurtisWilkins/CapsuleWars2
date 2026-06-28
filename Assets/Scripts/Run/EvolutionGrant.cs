using CapsuleWars.Data.Units;

namespace CapsuleWars.Run
{
    /// <summary>
    /// Grants post-battle XP to the run's party (BTS-H). Adds <c>config.XpPerBattleWin</c> to each party unit's
    /// <c>UnitDTO.Xp</c>; the reward hook (BattleNodeReturn on a win) calls this then <c>RunSession.Save()</c>. The
    /// accumulated XP drives each unit's evolution tier + base-stat growth (UnitEvolution) the next time it spawns.
    /// Returns the number of units that gained XP.
    /// </summary>
    public static class EvolutionGrant
    {
        public static int GrantXp(RunState state, EvolutionConfig_SO config)
        {
            if (state == null || config == null || config.XpPerBattleWin <= 0) return 0;
            int granted = 0;
            var party = state.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] == null) continue;
                party[i].Xp += config.XpPerBattleWin;
                granted++;
            }
            return granted;
        }
    }
}
