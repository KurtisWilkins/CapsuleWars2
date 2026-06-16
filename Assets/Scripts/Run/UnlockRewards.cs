using UnityEngine;

namespace CapsuleWars.Run
{
    /// <summary>
    /// Computes the meta-progression unlock points a finished run awards
    /// (Docs/12_RoguelikeRun.md §82: earned on run completion — win, or a loss
    /// after making progress). Pure + deterministic so it's test-friendly; the
    /// run-end flow applies the result to the PlayerProfile once.
    /// </summary>
    public static class UnlockRewards
    {
        /// <summary>Bonus points for completing (winning) a run, on top of the per-floor points.</summary>
        public const int CompletionBonus = 5;

        /// <summary>
        /// One point per floor reached, plus a completion bonus on a win.
        /// </summary>
        public static int PointsForRun(int floorsReached, bool won)
        {
            int pts = Mathf.Max(0, floorsReached) + (won ? CompletionBonus : 0);
            return pts;
        }
    }
}
