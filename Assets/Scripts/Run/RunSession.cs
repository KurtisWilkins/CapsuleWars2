using CapsuleWars.Persistence;

namespace CapsuleWars.Run
{
    /// <summary>
    /// Static holder for the in-progress run's state. Survives scene
    /// transitions (Map ↔ Battle) within a single play session, and is
    /// persisted to disk (<see cref="RunStore"/>, <c>run.json</c>) so a run
    /// resumes across app restarts. <see cref="Save"/> is called at run
    /// milestones (start, node completion, between-rounds customization).
    /// </summary>
    public static class RunSession
    {
        public static RunState Current;

        public static bool IsActive => Current != null;

        public static void StartNew(RunState state)
        {
            Current = state;
            Save();
        }

        /// <summary>Persist the current run, or delete the save if there is none.</summary>
        public static void Save()
        {
            if (Current == null) { RunStore.Delete(); return; }
            RunStore.Save(Current.ToDTO());
        }

        /// <summary>Whether a resumable run exists on disk.</summary>
        public static bool HasSavedRun => RunStore.Exists();

        /// <summary>
        /// Load a persisted run into <see cref="Current"/>. Returns true if a
        /// run was loaded; false (leaving Current untouched) if none exists.
        /// </summary>
        public static bool TryLoad()
        {
            var dto = RunStore.Load();
            if (dto == null) return false;
            var state = RunState.FromDTO(dto);
            if (state == null) return false;
            Current = state;
            return true;
        }

        /// <summary>Clear the in-memory run and remove its on-disk save.</summary>
        public static void Clear()
        {
            Current = null;
            RunStore.Delete();
        }
    }
}
