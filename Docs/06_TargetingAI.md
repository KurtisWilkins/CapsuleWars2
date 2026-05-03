# Targeting & AI

## Goal
Each unit autonomously picks a target, navigates to attack range, swings, and reacts to incoming damage. AI is per-unit; there is no central commander.

## Strategy chain (carried forward from Sprite Wars)
```
MovementGetUnits   → returns initial candidate set (e.g. all enemies)
   ↓
MovementFilter[]   → narrows the set in array order
   ↓
MovementTargeting  → picks the final target from the filtered set
```

This is the same pattern abilities use; in fact, ability targeting and movement targeting share the strategy interface.

## Default unit AI
Every unit ships with one of these targeting profiles (set on `UnitClass_SO` or per-unit override):

| Profile | GetUnits | Filters | Pick |
|---|---|---|---|
| **Frontline** | enemies | `InRange(arena)` → `Closest(1)` | first |
| **Ranged DPS** | enemies | `InRange(weapon.range × 1.5)` → `LowestHp(3)` → `Closest(1)` | first |
| **Backline assassin** | enemies | `InRange(arena)` → `BackmostEnemy(3)` → `LowestHp(1)` | first |
| **Healer** | allies | `LowestHpPercent(3)` → `InRange(ability.range)` → `Closest(1)` | first |
| **Tank** | enemies | `HighestThreat(3)` → `Closest(1)` | first |

`isHealerTargeting` (Sprite Wars carry-forward) prevents healers from being yanked off-task by reactive retargeting.

## Reactive retargeting
`UnitMovementController` listens to `OnAttackedBy` events. If the attacker is closer than the current target *and* the unit is not a healer, switch target. This emerges as the "interrupt the squishy" behavior players expect.

Cooldown: a unit cannot reactively retarget more than once per 2 seconds (prevents thrash).

## Movement (3D port)
Sprite Wars used `Rigidbody2D.linearVelocity`. CW2 uses `NavMeshAgent`:
- `agent.speed` = `BaseSpeed * SpeedMod`.
- `agent.stoppingDistance` = `weapon.range * 0.9f`.
- `agent.SetDestination(target.position)` each tick when out of range.
- Facing handled by `transform.LookAt(target)` snapped to Y-axis only (no pitch).

NavMesh is baked per arena. Dynamic obstacles (some abilities spawn them) use `NavMeshObstacle`.

## Attack range check
A unit is "in range" if `Vector3.Distance(self, target) ≤ weapon.range`. 3D distance, not 2D — Y is included so flying units behave correctly.

## Lifecycle
- `OnEnable` — register with `BattleContext.UnitRegistry`.
- `OnDisable` — unregister.
- `BattleStateManager.OnBattleEnd` — disable AI tick globally.

## Pathing edge cases
- Target unreachable (no NavMesh path) — fall back to `MovementFilterClosestReachable`.
- Stuck timer — if the agent hasn't moved >0.1m in 2s, force a re-targeting.

## Animation hand-off
Targeting and movement do not play animations directly. They set state on `UnitAnimationController`:
- `Speed` parameter (0=idle, 0.5=run, 1=stunned).
- Attacks are triggered only when in range and cooldown is up; the controller fires `AttackTrigger` and the Animator decides the rest.

## Open items
- Whether enemy AI uses different profiles than player units (default: same system, different per-unit configs).
- Boss AI — likely needs scripted phases, not just profile selection. Address at M9+.
