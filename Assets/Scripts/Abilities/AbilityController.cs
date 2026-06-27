using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Abilities
{
    /// <summary>
    /// MonoBehaviour that hosts a unit's abilities. Ticks each ability's
    /// trigger every frame during the Active phase. Resolves weapon-class
    /// compatibility at Awake — abilities the unit's weapon doesn't
    /// satisfy are marked Locked and never fire.
    /// </summary>
    public class AbilityController : MonoBehaviour, ISynergyBehaviorSink
    {
        [Tooltip("Abilities granted to this unit. Order is authoring-only; runtime checks all.")]
        [SerializeField] private List<Ability_SO> abilities = new();

        private readonly List<AbilityRuntime> runtimes = new();
        private UnitRoot root;
        private IBattleEvents subscribedEvents;

        // [code] class-synergy behaviors (Docs/09), pushed by SynergyResolver via ISynergyBehaviorSink each recompute.
        private readonly List<SynergyEffect> synergyEffects = new();

        public IReadOnlyList<AbilityRuntime> Runtimes => runtimes;

        /// <summary>ISynergyBehaviorSink — SynergyResolver pushes this unit's active [code] synergy effects here.
        /// Passing null/empty clears them (tier no longer active).</summary>
        public void SetSynergyEffects(IReadOnlyList<SynergyEffect> effects)
        {
            synergyEffects.Clear();
            if (effects == null) return;
            for (int i = 0; i < effects.Count; i++) synergyEffects.Add(effects[i]);
        }

        private void Awake()
        {
            root = GetComponentInParent<UnitRoot>();
            var equipped = root != null && root.Attack != null ? root.Attack.WeaponClass : null;

            for (int i = 0; i < abilities.Count; i++)
            {
                var def = abilities[i];
                if (def == null) continue;
                var rt = new AbilityRuntime(def, root)
                {
                    IsLocked = !def.IsWeaponCompatible(equipped)
                };
                runtimes.Add(rt);
            }
        }

        private void Update()
        {
            if (CombatServices.Phase != BattlePhase.Active) return;
            EnsureSubscribed();
            if (root == null || root.Health == null || root.Health.IsDowned) return;
            if (root.Status != null && root.Status.CannotUseAbilities) return;

            float now = Time.time;
            for (int i = 0; i < runtimes.Count; i++)
            {
                runtimes[i].Tick(now);
            }
        }

        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();

        // -----------------------------------------------------------------
        // Combat-event triggers: subscribe to the BattleEventBus (via CombatServices, Core interface) and stamp
        // per-runtime event timestamps. Event triggers fire when their event is newer than the runtime's last cast.
        // -----------------------------------------------------------------

        private void EnsureSubscribed()
        {
            var ev = CombatServices.Events;
            if (ev == null || ReferenceEquals(ev, subscribedEvents)) return;
            Unsubscribe();
            ev.OnDamageDealt += HandleDamageDealt;
            ev.OnDamageTaken += HandleDamageTaken;
            ev.OnKill += HandleKill;
            ev.OnDowned += HandleDowned;
            subscribedEvents = ev;
        }

        private void Unsubscribe()
        {
            if (subscribedEvents == null) return;
            subscribedEvents.OnDamageDealt -= HandleDamageDealt;
            subscribedEvents.OnDamageTaken -= HandleDamageTaken;
            subscribedEvents.OnKill -= HandleKill;
            subscribedEvents.OnDowned -= HandleDowned;
            subscribedEvents = null;
        }

        private bool IsSelf(IUnitRef u) => u != null && root != null && u.GameObject == root.gameObject;

        private void HandleDamageDealt(DamageEvent e) { if (!IsSelf(e.Source)) return; Stamp(EventKind.HitDealt); ApplySynergyHeal(SynergyEffectKind.HealOnHit); }
        private void HandleDamageTaken(DamageEvent e) { if (IsSelf(e.Target)) Stamp(EventKind.HitTaken); }
        private void HandleKill(KillEvent e) { if (!IsSelf(e.Source)) return; Stamp(EventKind.Kill); ApplySynergyHeal(SynergyEffectKind.HealOnKill); }

        private void HandleDowned(DownedEvent e)
        {
            if (root == null || e.Target == null) return;
            if (IsSelf(e.Target)) return;                 // self death, not an ally death
            if (e.Target.Team != root.Team) return;       // enemy death, not an ally
            Stamp(EventKind.AllyDeath);
        }

        private enum EventKind { HitDealt, HitTaken, Kill, AllyDeath }

        // [code] synergy heals (Docs/09): heal magnitude% of MaxHp on the matching combat event for this unit.
        private void ApplySynergyHeal(SynergyEffectKind kind)
        {
            if (synergyEffects.Count == 0 || root == null || root.Health == null || root.Status == null) return;
            if (root.Health.IsDowned) return;
            int maxHp = root.Status.MaxHp;
            if (maxHp <= 0) return;
            for (int i = 0; i < synergyEffects.Count; i++)
            {
                var se = synergyEffects[i];
                if (se.kind != kind || se.magnitude <= 0f) continue;
                int heal = Mathf.RoundToInt(maxHp * (se.magnitude / 100f));
                if (heal <= 0) continue;
                int newHp = Mathf.Min(maxHp, root.Health.CurrentHp + heal);
                root.Health.RestoreToPercent((float)newHp / maxHp);
            }
        }

        private void Stamp(EventKind kind)
        {
            float now = Time.time;
            for (int i = 0; i < runtimes.Count; i++)
            {
                var rt = runtimes[i];
                switch (kind)
                {
                    case EventKind.HitDealt: rt.LastHitDealtTime = now; break;
                    case EventKind.HitTaken: rt.LastHitTakenTime = now; break;
                    case EventKind.Kill: rt.LastKillTime = now; break;
                    case EventKind.AllyDeath: rt.LastAllyDeathTime = now; break;
                }
            }
        }
    }
}
