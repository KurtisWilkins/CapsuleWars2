namespace CapsuleWars.Run
{
    /// <summary>
    /// Static holder for the in-progress run's state. Survives scene
    /// transitions (Map ↔ Battle) within a single editor/play session.
    /// Reset when starting a new run.
    /// M8 will replace this with persistent JSON loaded by a RunLoader.
    /// </summary>
    public static class RunSession
    {
        public static RunState Current;

        public static bool IsActive => Current != null;

        public static void StartNew(RunState state)
        {
            Current = state;
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}
