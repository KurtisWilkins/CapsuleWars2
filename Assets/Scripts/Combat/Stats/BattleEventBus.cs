using System;
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
    /// Per-unit events (UnitHealthController.OnDamageTaken etc.) still fire
    /// — the bus is an additional fan-out hub, not a replacement.
    /// </summary>
    public class BattleEventBus
    {
        public event Action<DamageEvent> OnDamageDealt;
        public event Action<DamageEvent> OnDamageTaken;
        public event Action<DownedEvent> OnDowned;
        public event Action<KillEvent> OnKill;
        public event Action OnBattleStart;
        public event Action<BattleResult> OnBattleEnd;

        public void HookUnit(IUnitRef unit)
        {
            if (unit == null || unit.GameObject == null) return;
            var health = unit.GameObject.GetComponentInParent<UnitHealthController>();
            if (health == null) return;

            health.OnDamageTaken += ForwardDamage;
            health.OnDowned += ForwardDowned;
        }

        public void UnhookUnit(IUnitRef unit)
        {
            if (unit == null || unit.GameObject == null) return;
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
