namespace CapsuleWars.Core
{
    /// <summary>
    /// Static service locator for combat-time singletons.
    /// BattleStateManager / BattleContext set these properties in Awake;
    /// other assemblies read them without taking a direct dependency on
    /// the Combat assembly. The static surface is a deliberate stepping
    /// stone — proper DI (context injected at spawn) is planned post-M4.
    /// </summary>
    public static class CombatServices
    {
        public static ICombatRegistry Registry { get; set; }
        public static BattlePhase Phase { get; set; } = BattlePhase.PreBattle;
        public static IElementChart ElementChart { get; set; }
        public static IBattleEvents Events { get; set; }
    }
}


