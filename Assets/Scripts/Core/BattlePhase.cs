namespace CapsuleWars.Core
{
    /// <summary>
    /// Where a battle is in its lifecycle.
    /// PreBattle: deployment, configuration, Start button. Units idle, no AI.
    /// Active: combat is running, AI ticks, damage flows.
    /// Resolved: a winner has been decided. End screen visible. Units frozen.
    /// </summary>
    public enum BattlePhase
    {
        PreBattle = 0,
        Active = 1,
        Resolved = 2
    }
}
