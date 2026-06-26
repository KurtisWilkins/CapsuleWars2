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
    public class AbilityController : MonoBehaviour
    {
        [Tooltip("Abilities granted to this unit. Order is authoring-only; runtime checks all.")]
        [SerializeField] private List<Ability_SO> abilities = new();

        private readonly List<AbilityRuntime> runtimes = new();
        private UnitRoot root;
        private IBattleEvents subscribedEvents;

        public IReadOnlyList<AbilityRuntime> Runtimes => runtimes;

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

        private void HandleDamageDealt(DamageEvent e) { if (IsSelf(e.Source)) Stamp(EventKind.HitDealt); }
        private void HandleDamageTaken(DamageEvent e) { if (IsSelf(e.Target)) Stamp(EventKind.HitTaken); }
        private void HandleKill(KillEvent e) { if (IsSelf(e.Source)) Stamp(EventKind.Kill); }

        private void HandleDowned(DownedEvent e)
        {
            if (root == null || e.Target == null) return;
            if (IsSelf(e.Target)) return;                 // self death, not an ally death
            if (e.Target.Team != root.Team) return;       // enemy death, not an ally
            Stamp(EventKind.AllyDeath);
        }

        private enum EventKind { HitDealt, HitTaken, Kill, AllyDeath }

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
