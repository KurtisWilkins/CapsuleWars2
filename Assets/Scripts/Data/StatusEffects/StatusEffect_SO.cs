using System.Collections.Generic;
using CapsuleWars.Core;
using UnityEngine;

namespace CapsuleWars.Data.StatusEffects
{
    /// <summary>
    /// Data-driven status effect definition. Supports all 24 effects from
    /// Docs/10_StatusEffects.md via configuration — UnitStatusController
    /// reads the data and applies behavior.
    ///
    /// Example configurations:
    /// - <b>Stunned</b>: kind=Control, preventsAction=true, preventsMovement=true, defaultDuration=2.
    /// - <b>AtkUp</b>: kind=Buff, statBuffs=[{Atk, Percent, 25}], defaultDuration=10.
    /// - <b>Bleeding</b>: kind=DoT, tickInterval=1, tickAmount=-5, defaultDuration=5.
    /// - <b>Regenerating</b>: kind=HoT, tickInterval=1, tickAmount=10, defaultDuration=5.
    /// - <b>Silenced</b>: kind=Debuff, preventsAbilities=true, defaultDuration=4.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "CapsuleWars/Status Effects/Status Effect", order = 80)]
    public class StatusEffect_SO : ScriptableObject
    {
        [Tooltip("Stable ID used by save files and Database lookup.")]
        [SerializeField] private string statusId;

        [Tooltip("I2 term key for the effect's display name (e.g. Status.Stunned.Name).")]
        [SerializeField] private string nameTermKey;

        [Tooltip("I2 term key for the effect's description.")]
        [SerializeField] private string descTermKey;

        [Tooltip("UI icon shown over the affected unit / in tooltips.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Category for UI presentation and routing (e.g. cleanse spells targeting Debuff/Control only).")]
        [SerializeField] private StatusEffectKind kind = StatusEffectKind.Debuff;

        [Tooltip("Seconds the effect lasts after application. -1 = until manually cleared.")]
        [SerializeField] private float defaultDuration = 5f;

        [Tooltip("Max independent stacks (only meaningful when stackBehavior = Independent).")]
        [SerializeField, Min(1)] private int maxStacks = 1;

        [SerializeField] private StackBehavior stackBehavior = StackBehavior.Refresh;
        [SerializeField] private ResistanceCheck resistance = ResistanceCheck.None;

        [Tooltip("Hit accuracy vs the target's Resistance stat (Docs/10). Apply chance = (accuracy - resistance)/1000, " +
                 "clamped 0–1. 1000 = always lands. Only consulted when resistance is RollOnApply / RollPerTick.")]
        [SerializeField, Min(0)] private int effectAccuracy = 1000;

        [Header("Stat Modifiers (Buff / Debuff)")]
        [Tooltip("Applied to the unit's stat block while this effect is active. Empty list = no stat changes.")]
        [SerializeField] private List<StatBuff> statBuffs = new();

        [Header("DoT / HoT")]
        [Tooltip("Seconds between ticks. Ignored if tickAmount = 0.")]
        [SerializeField, Min(0.1f)] private float tickInterval = 1f;

        [Tooltip("HP change per tick. Positive = heal, negative = damage. 0 disables ticking.")]
        [SerializeField] private int tickAmount = 0;

        [Tooltip("If true, tickAmount is treated as a percent of target MaxHp (e.g. -5 = -5% MaxHp per tick).")]
        [SerializeField] private bool tickIsPercentOfMaxHp = false;

        [Header("Behavior Flags")]
        [Tooltip("Blocks basic attacks and ability casts. (Stunned, Frozen.)")]
        [SerializeField] private bool preventsAction = false;

        [Tooltip("Blocks movement. Unit can still rotate, animate, and may still attack if in range. (Trapped.)")]
        [SerializeField] private bool preventsMovement = false;

        [Tooltip("Blocks ability casts only (basic attacks still work). (Silenced.)")]
        [SerializeField] private bool preventsAbilities = false;

        public string StatusId => statusId;
        public string NameTermKey => nameTermKey;
        public string DescTermKey => descTermKey;
        public Sprite Icon => icon;
        public StatusEffectKind Kind => kind;
        public float DefaultDuration => defaultDuration;
        public int MaxStacks => maxStacks;
        public StackBehavior StackBehavior => stackBehavior;
        public ResistanceCheck Resistance => resistance;
        public int Accuracy => effectAccuracy;
        public IReadOnlyList<StatBuff> StatBuffs => statBuffs;
        public float TickInterval => tickInterval;
        public int TickAmount => tickAmount;
        public bool TickIsPercentOfMaxHp => tickIsPercentOfMaxHp;
        public bool PreventsAction => preventsAction;
        public bool PreventsMovement => preventsMovement;
        public bool PreventsAbilities => preventsAbilities;
    }
}
