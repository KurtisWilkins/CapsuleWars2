namespace CapsuleWars.Core
{
    /// <summary>
    /// Static service locator for combat-time singletons.
    /// BattleContext sets <see cref="Registry"/> in Awake; Units.* reads it
    /// without referencing the Combat assembly. The static is a deliberate
    /// stepping stone for M2 — proper DI (BattleContext injected at spawn)
    /// is planned for M3 when we introduce the BattleSpawner.
    /// </summary>
    public static class CombatServices
    {
        public static ICombatRegistry Registry { get; set; }
    }
}
