using System;
using System.Collections.Generic;
using CapsuleWars.Combat.Stats;
using CapsuleWars.Core;
using CapsuleWars.Units.Controllers;
using UnityEngine;

namespace CapsuleWars.Combat.State
{
    /// <summary>
    /// Owns a single battle's lifecycle (PreBattle → Active → Resolved),
    /// the BattleEventBus, the BattleStatsAggregator, and win/loss logic.
    /// One per scene. Pairs with a <see cref="BattleContext"/> on the same
    /// or sibling GameObject — finds it via FindAnyObjectByType at Awake.
    /// </summary>
    [DisallowMultipleComponent]
    public class BattleStateManager : MonoBehaviour
    {
        [Header("Timing")]
        [Tooltip("Seconds before sudden death activates if no team has been wiped out.")]
        [SerializeField, Min(10f)] private float suddenDeathTimer = 90f;

        [Tooltip("Damage multiplier applied to all sources once sudden death engages.")]
        [SerializeField, Min(1f)] private float suddenDeathMultiplier = 2f;

        [Tooltip("Auto-start the battle in Start() instead of waiting for a button.")]
        [SerializeField] private bool autoStart = false;

        public BattlePhase Phase { get; private set; } = BattlePhase.PreBattle;
        public BattleEventBus EventBus { get; } = new BattleEventBus();
        public BattleStatsAggregator Stats { get; } = new BattleStatsAggregator();
        public bool SuddenDeathEngaged { get; private set; }

        public event Action<BattleResult, IReadOnlyList<BattleLeaderboardEntry>> OnBattleEnded;
        public event Action OnBattleStarted;
        public event Action<BattlePhase> OnPhaseChanged;

        private ICombatRegistry registry;
        private float battleStartTime;

        private void Awake()
        {
            registry = FindAnyObjectByType<BattleContext>();
            if (registry == null)
            {
                Debug.LogError("[BattleStateManager] No BattleContext in scene. Add one before pressing Play.", this);
                enabled = false;
                return;
            }

            CombatServices.Phase = BattlePhase.PreBattle;
            Stats.HookBus(EventBus);

            registry.OnUnitRegistered += HandleUnitRegistered;
            registry.OnUnitUnregistered += HandleUnitUnregistered;
        }

        private void Start()
        {
            // Belt-and-suspenders: if any UnitRoot was activated before
            // BattleContext.Awake set CombatServices.Registry (race on
            // initial scene load), its OnEnable will have no-op'd.
            // Sweep the scene once and re-register; Registry.Register
            // is idempotent.
            var allRoots = FindObjectsByType<UnitRoot>(FindObjectsSortMode.None);
            for (int i = 0; i < allRoots.Length; i++)
            {
                if (allRoots[i] != null) registry.Register(allRoots[i]);
            }

            // Apply -50% rule for units coming back from a previous downed state.
            foreach (var unit in registry.Units) ApplyDownedCarryForward(unit);

            if (autoStart) StartBattle();
        }

        private void OnDestroy()
        {
            if (registry != null)
            {
                registry.OnUnitRegistered -= HandleUnitRegistered;
                registry.OnUnitUnregistered -= HandleUnitUnregistered;
            }
            Stats.UnhookBus();
        }

        public void StartBattle()
        {
            if (Phase != BattlePhase.PreBattle) return;
            SetPhase(BattlePhase.Active);
            battleStartTime = Time.time;
            SuddenDeathEngaged = false;
            EventBus.RaiseBattleStart();
            OnBattleStarted?.Invoke();
        }

        private void Update()
        {
            if (Phase != BattlePhase.Active) return;

            if (!SuddenDeathEngaged && Time.time - battleStartTime >= suddenDeathTimer)
                SuddenDeathEngaged = true;

            EvaluateWinCondition();
        }

        private void EvaluateWinCondition()
        {
            int playersAlive = 0;
            int enemiesAlive = 0;
            foreach (var u in registry.Units)
            {
                if (u == null || u.IsDowned) continue;
                if (u.Team == Team.Player) playersAlive++;
                else if (u.Team == Team.Enemy) enemiesAlive++;
            }

            if (playersAlive > 0 && enemiesAlive > 0) return;

            Team? winner = null;
            BattleEndReason reason = SuddenDeathEngaged ? BattleEndReason.SuddenDeath : BattleEndReason.KnockOut;

            if (playersAlive == 0 && enemiesAlive == 0)
            {
                reason = BattleEndReason.Draw;
                winner = null;
            }
            else if (playersAlive > 0) winner = Team.Player;
            else winner = Team.Enemy;

            ResolveBattle(new BattleResult(winner, reason, Time.time - battleStartTime));
        }

        private void ResolveBattle(BattleResult result)
        {
            SetPhase(BattlePhase.Resolved);

            // Persist downed flags for the next battle.
            foreach (var u in registry.Units)
            {
                if (u == null || !u.IsDowned) continue;
                var root = u.GameObject.GetComponentInParent<UnitRoot>();
                if (root != null) BattleRosterState.MarkDownedThisBattle(root.UnitId);
            }
            BattleRosterState.CommitBattleResult();

            var leaderboard = Stats.BuildLeaderboard();
            EventBus.RaiseBattleEnd(result);
            OnBattleEnded?.Invoke(result, leaderboard);
        }

        private void SetPhase(BattlePhase phase)
        {
            if (Phase == phase) return;
            Phase = phase;
            CombatServices.Phase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        private void HandleUnitRegistered(IUnitRef unit)
        {
            EventBus.HookUnit(unit);
            var root = unit.GameObject != null ? unit.GameObject.GetComponentInParent<UnitRoot>() : null;
            if (root != null) Stats.RegisterUnit(root.UnitId, root.DisplayName);
        }

        private void HandleUnitUnregistered(IUnitRef unit)
        {
            EventBus.UnhookUnit(unit);
        }

        private void ApplyDownedCarryForward(IUnitRef unit)
        {
            if (unit == null || unit.GameObject == null) return;
            var root = unit.GameObject.GetComponentInParent<UnitRoot>();
            if (root == null || root.Health == null) return;

            if (BattleRosterState.WasDownedPreviousBattle(root.UnitId))
                root.Health.RestoreToPercent(0.5f);
            else
                root.Health.RestoreToPercent(1f);
        }
    }
}
