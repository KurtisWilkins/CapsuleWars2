using System;
using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Combat.Stats
{
    /// <summary>
    /// Battle-wide event surface. Subscribes to per-unit events at unit
    /// registration time and re-fires them as battle-scoped events for
    /// consumers (stats aggregator, UI, audio, VFX) that don't want to
    /// hook each unit individually.
    /// HookUnit/UnhookUnit are idempotent — safe to call multiple times
    /// per unit during initialization sweeps.
    /// </summary>
    public class BattleEventBus : IBattleEvents
    {
        public event Action<DamageEvent> OnDamageDealt;
        public event Action<DamageEvent> OnDamageTaken;
        public event Action<DownedEvent> OnDowned;
        public event Action<KillEvent> OnKill;
        public event Action OnBattleStart;
        public event Action<BattleResult> OnBattleEnd;

        private readonly HashSet<IUnitRef> hooked = new HashSet<IUnitRef>();

        public void HookUnit(IUnitRef unit)
        {
            if (unit == null || unit.GameObject == null) return;
            if (!hooked.Add(unit)) return; // already hooked

            var health = unit.GameObject.GetComponentInParent<UnitHealthController>();
            if (health == null) return;

            health.OnDamageTaken += ForwardDamage;
            health.OnDowned += ForwardDowned;
        }

        public void UnhookUnit(IUnitRef unit)
        {
            if (unit == null || unit.GameObject == null) return;
            if (!hooked.Remove(unit)) return;

            var health = unit.GameObject.GetComponentInParent<UnitHealthController>();
            if (health == null) return;

            health.OnDamageTaken -= ForwardDamage;
            health.OnDowned -= ForwardDowned;
        }

        public void RaiseBattleStart() => OnBattleStart?.Invoke();
        public void RaiseBattleEnd(BattleResult result) => OnBattleEnd?.Invoke(result);

        private void ForwardDamage(DamageEvent e)
        {
            OnDamageTaken?.Invoke(e);
            OnDamageDealt?.Invoke(e);
        }

        private void ForwardDowned(DownedEvent e)
        {
            OnDowned?.Invoke(e);
            if (e.Source != null && !ReferenceEquals(e.Source, e.Target))
                OnKill?.Invoke(new KillEvent(e.Source, e.Target));
        }
    }
}
