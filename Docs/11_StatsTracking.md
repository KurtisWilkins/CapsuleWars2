# Stats Tracking

## Two layers
- **Per-battle scratch** — counters reset every battle. Used for end-of-battle leaderboard ("most damage", "most kills").
- **Per-run scratch** — accumulates across battles in one run. Used for run summary screen.
- **Lifetime** — accumulates across runs into `LegacyUnitProfile`. Only legacy units write here. Roguelike-only units never write to lifetime unless promoted at run end.

## Event surface
```
namespace CapsuleWars.Combat.Stats

class BattleEventBus
    event Action<DamageEvent> OnDamageDealt;
    event Action<DamageEvent> OnDamageTaken;
    event Action<HealEvent> OnHealDone;
    event Action<HealEvent> OnHealReceived;
    event Action<KillEvent> OnKill;
    event Action<DownedEvent> OnDowned;
    event Action<RevivedEvent> OnRevived;
    event Action<AbilityEvent> OnAbilityCast;
    event Action<AbilityEvent> OnAbilityHit;
    event Action<StatusEvent> OnStatusApplied;
    event Action<StatusEvent> OnStatusReceived;
    event Action<StatusEvent> OnStatusExpired;
    event Action<CritEvent> OnCrit;
    event Action<DodgeEvent> OnDodge;
    event Action<BlockEvent> OnBlock;
    event Action OnBattleStart;
    event Action<BattleResult> OnBattleEnd;
```

## Subscriptions
- `BattleStatsAggregator` — subscribes to all damage/heal/kill/status events; updates per-unit per-battle counters.
- `RunStatsTracker` — accumulates per-battle into per-run.
- `LegacyStatWriter` — at run end, for each legacy unit that participated, merges per-run into `LegacyUnitProfile`.
- UI — leaderboards subscribe to display updates.
- VFX — damage popup, crit flash subscribe to relevant events.
- Audio — hit/crit/kill SFX subscribe.

## Counters tracked

### Per-battle (resets every battle)
- `damageDealt`, `attackCountDealt`
- `damageTaken`, `attackCountTaken`
- `healingDone`, `healCountDone`
- `healingReceived`, `healCountReceived`
- `kills`
- `crits`, `dodges`, `blocks`
- `abilitiesCast`
- `buffsApplied`, `buffsReceived`
- `debuffsApplied`, `debuffsReceived`
- `revivesDone`, `revivesReceived`
- `faintsThisBattle` (always 0 or 1)

### Per-run (accumulates across battles)
Same fields; resets at run start.
Plus:
- `battlesWon`, `battlesLost`, `battlesParticipated`
- `nodesVisited`
- `goldEarned`
- `experienceEarned`

### Lifetime (legacy only — accumulates across runs)
Same as per-run, plus:
- `runsParticipated`, `runsWon`
- `daysSinceCreation`
- `legacyXpTotal` (independent from level — used for cosmetic milestones)

## Mode-aware writing
```
RunEnd:
  for each unit that participated:
    if unit.IsLegacy:
      LegacyStatWriter.Merge(unit.PerRunStats → LegacyProfile.Lifetime)
    else if unit.PromotedToLegacy:
      LegacyStatWriter.InitializeFromPerRun(unit, unit.PerRunStats)
    // roguelike-only, not promoted: discarded
```

## End-of-battle leaderboard
Shown post-battle:
- Top damage dealt
- Top damage taken (tank)
- Top healing
- Top kills
- Top revives

Fed by `BattleStatsAggregator` snapshot.

## Run summary
Shown post-run (win or loss):
- Final map progress
- Total gold earned
- Per-unit run stats with deltas (lifetime before/after for legacy units)
- Recruit prompt for surviving roguelike-only units (if win)

## Open items
- Whether to expose stats via a public profile screen for legacy units. Default: yes, low priority.
- Whether per-battle stat snapshots are persisted (post-mortem replay) or discarded. Default: discard at run end.
