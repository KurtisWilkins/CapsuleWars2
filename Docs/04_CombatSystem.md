# Combat System

## Type
Real-time, continuous (not turn-based). Units move and attack autonomously on a NavMesh-baked arena. Combat is fully automated once the player confirms deployment.

## Battle lifecycle
```
1. Pre-battle
   1a. Arena loaded (preset + variant prop placement)
   1b. Player places deployable units on a deployment grid
   1c. Player confirms → Battle starts
2. Combat
   2a. AI loops: target → move → attack → check abilities → react
   2b. Stats events fire → BattleStatsAggregator
3. Resolution
   3a. Win condition met → BattleEnd
   3b. Reward sequence (XP → Gold/Equipment → Recruit prompt)
   3c. Map screen (roguelike) or run summary
```

## Win/loss conditions
- **Win**: all enemy units downed.
- **Loss**: all your deployable units downed *and* bench is empty for this battle.
- **Timer fallback**: if 90s elapse with no KOs, sudden-death damage multiplier (×2) ramps until resolution. (Tunable; default 90s.)

## Downed rule
- A unit at 0 HP enters a `Downed` state for the rest of the current battle.
- Cannot be targeted, cannot act.
- At the **next** battle, the unit returns at exactly **50% max HP**. No stacking — if it's downed again next battle, it returns at 50% the battle after that, not 25%.
- Battle after that → restored to 100%.
- Mid-battle revive abilities (e.g. priest) bring a downed ally back at the ability's heal value, not 50%.

## Run loss
- Roguelike run ends when a battle is lost. No retries.
- Legacy units lose XP gained that run (level resets to 1 next run anyway).
- Recruited roguelike-only units that survived the lost battle are **not** offered for legacy promotion (the run failed).

## AI tick
Each `UnitMovementController` ticks in `FixedUpdate`:
1. If `currentTarget == null || target.IsDowned` → call `targetingChain.GetTarget()`.
2. If `target` outside attack range → NavMesh approach.
3. If in range → halt, face target, signal `UnitAttackController.TryAttack()`.
4. `UnitAttackController` checks cooldown; if ready, plays attack via `UnitAnimationController`.
5. Animation hit-frame fires `OnHitFrame()` → ability/effect chain runs.
6. `UnitMovementController` listens for `OnAttackedBy` events to retarget reactively (Sprite Wars carry-forward).

## Battle controllers (split from Sprite Wars god class)
The 833-line `BattleController.cs` is replaced by:

| Component | Responsibility |
|---|---|
| `BattleStateManager` | lifecycle (`PreBattle`, `Active`, `Resolved`), arena loading, win/loss check, time tracking |
| `BattleStatsAggregator` | subscribes to `BattleEventBus`, writes per-unit + per-battle counters |
| `BattleRewardController` | post-battle XP, gold drops, equipment rolls, recruit prompts |
| `DeploymentManager` | grid, placement validation, bench swap between battles |
| `BattleSpawner` | unit instantiation, replaces Sprite Wars `UnitSpawner.Instance` singleton with `BattleContext` injection |

A `BattleContext` is created at battle start and passed to every component that needs it — replaces global static `deployedUnits` list.

## Damage pipeline
```
Attacker.Atk + Atk(class+equip+status) +
ElementMod(attacker.element vs defender.element) +
CritRoll() →
Defender.Def(class+equip+status) →
Final damage → UnitHealthController.TakeDamage(amount, source)
   → fires OnHealthChanged
   → if HP ≤ 0: fires OnDowned
```
All multipliers and adders go through `Combat.Stats.StatCalculator`. No per-effect ad-hoc math.

## Knockback (3D port)
Sprite Wars used `Rigidbody2D.AddForce`. CW2 uses one of:
- `Rigidbody.AddForce(dir * force * (1f / mass), ForceMode.Impulse)` if the unit has a Rigidbody.
- Direct `NavMeshAgent.Warp()` + DOTween position tween for visual knockback if NavMesh-only.

Default: NavMesh + DOTween. Knockback distance scales with `attackerForce / defenderMass`.

## Capture mechanic
**Removed.** `CaptureUnitEffect.cs` and `CaptureButtonMovement.cs` are deleted; recruitment happens at end of run only (see `12_RoguelikeRun.md`).

## Open items
- Sudden-death timer default (90s) needs playtest validation.
- Whether bench swaps cost a turn or are free between battles. Default: free.
- Friendly-fire — default off; some abilities may explicitly opt in.
