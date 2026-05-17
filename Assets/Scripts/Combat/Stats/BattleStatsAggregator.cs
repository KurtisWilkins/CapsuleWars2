using System.Collections.Generic;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;

namespace CapsuleWars.Combat.Stats
{
    /// <summary>
    /// Builds per-unit per-battle counters by subscribing to a
    /// <see cref="BattleEventBus"/>. Owned by BattleStateManager;
    /// reset at battle start, queried at battle end to populate the
    /// leaderboard UI.
    /// </summary>
    public class BattleStatsAggregator
    {
        private readonly Dictionary<string, UnitBattleStats> byUnitId = new();
        private BattleEventBus bus;

        public IReadOnlyDictionary<string, UnitBattleStats> Stats => byUnitId;

        public void HookBus(BattleEventBus eventBus)
        {
            UnhookBus();
            bus = eventBus;
            if (bus == null) return;
            bus.OnDamageTaken += OnDamageTaken;
            bus.OnKill += OnKill;
            bus.OnDowned += OnDowned;
        }

        public void UnhookBus()
        {
            if (bus == null) return;
            bus.OnDamageTaken -= OnDamageTaken;
            bus.OnKill -= OnKill;
            bus.OnDowned -= OnDowned;
            bus = null;
        }

        public void Reset()
        {
            byUnitId.Clear();
        }

        public void RegisterUnit(string unitId, string displayName)
        {
            if (string.IsNullOrEmpty(unitId)) return;
            if (byUnitId.ContainsKey(unitId)) return;
            byUnitId[unitId] = new UnitBattleStats { UnitId = unitId, DisplayName = displayName };
        }

        public UnitBattleStats Get(string unitId)
        {
            return string.IsNullOrEmpty(unitId) || !byUnitId.TryGetValue(unitId, out var s) ? null : s;
        }

        public List<BattleLeaderboardEntry> BuildLeaderboard()
        {
            var list = new List<BattleLeaderboardEntry>(byUnitId.Count);
            foreach (var kv in byUnitId)
            {
                var s = kv.Value;
                list.Add(new BattleLeaderboardEntry(s.UnitId, s.DisplayName, s.DamageDealt, s.DamageTaken, s.Kills));
            }
            return list;
        }

        private void OnDamageTaken(DamageEvent e)
        {
            var sourceId = GetUnitId(e.Source);
            var targetId = GetUnitId(e.Target);

            if (!string.IsNullOrEmpty(sourceId) && byUnitId.TryGetValue(sourceId, out var sourceStats))
                sourceStats.RecordDamageDealt(e.Amount);

            if (!string.IsNullOrEmpty(targetId) && byUnitId.TryGetValue(targetId, out var targetStats))
                targetStats.RecordDamageTaken(e.Amount);
        }

        private void OnKill(KillEvent e)
        {
            var sourceId = GetUnitId(e.Source);
            if (!string.IsNullOrEmpty(sourceId) && byUnitId.TryGetValue(sourceId, out var s))
                s.RecordKill();
        }

        private void OnDowned(DownedEvent e)
        {
            var targetId = GetUnitId(e.Target);
            if (!string.IsNullOrEmpty(targetId) && byUnitId.TryGetValue(targetId, out var s))
                s.RecordFaint();
        }

        private static string GetUnitId(IUnitRef u)
        {
            if (u == null || u.GameObject == null) return null;
            var root = u.GameObject.GetComponentInParent<UnitRoot>();
            return root != null ? root.UnitId : u.GameObject.name;
        }
    }
}
