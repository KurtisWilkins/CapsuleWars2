namespace CapsuleWars.Core
{
    /// <summary>
    /// Why a battle ended and which side won.
    /// </summary>
    public enum BattleEndReason
    {
        KnockOut = 0,
        SuddenDeath = 1,
        Draw = 2
    }

    /// <summary>
    /// Outcome of a completed battle. <see cref="WinningTeam"/> is null on a draw.
    /// </summary>
    public readonly struct BattleResult
    {
        public readonly Team? WinningTeam;
        public readonly BattleEndReason Reason;
        public readonly float Duration;

        public BattleResult(Team? winningTeam, BattleEndReason reason, float duration)
        {
            WinningTeam = winningTeam;
            Reason = reason;
            Duration = duration;
        }

        public bool IsDraw => !WinningTeam.HasValue;
        public bool PlayerWon => WinningTeam == Team.Player;
    }
}
