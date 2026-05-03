# Ability System

## Foundation
Carry forward the Sprite Wars composable pattern: an `Ability_SO` is the data, and behavior is defined by composing strategies.

```
Ability_SO
├── string nameTermKey
├── string descTermKey
├── Sprite icon
├── float cooldown
├── float range
├── ElementType_SO element
├── WeaponClass_SO[] requiredWeaponClasses   // NEW for CW2 — empty = any
├── AbilityTriggerStrategy[] triggers         // evolution-indexed
├── AbilityTargetingStrategy[] targeting      // evolution-indexed
├── AbilityFilterStrategy[] filters           // evolution-indexed
├── AbilityEffectsStrategy[] effects          // evolution-indexed
└── AnimationData animData
```

The four strategy axes (`Trigger`, `Targeting`, `Filter`, `Effects`) are abstract `ScriptableObject` base classes with concrete subclasses dropped into per-ability instances.

## Trigger strategies
Existing:
- `TimeBasedTrigger` — fires every N seconds (cooldown).

New for CW2 (event-based):
- `OnHitTrigger` — fires when the unit lands a basic attack.
- `OnTakeHitTrigger` — fires when the unit is hit.
- `OnKillTrigger` — fires when the unit kills an enemy.
- `OnLowHpTrigger` — fires once when HP crosses a threshold (e.g. 30%).
- `OnAllyDeathTrigger` — fires when an allied unit is downed.
- `OnBattleStartTrigger` — fires once at battle start.

All event triggers subscribe to `BattleEventBus` at battle start, unsubscribe at end. Triggers may have an inner cooldown to prevent spam (e.g. `OnHitTrigger` with `0.5s` internal CD).

## Targeting strategies
- `GetAllTargets` — returns every unit on the battlefield.
- `GetEnemyTargets` — returns enemy units only.
- `GetAllyTargets` — returns ally units only (incl. self).
- `GetSelfTarget` — singleton self.
- `GetAttackerTarget` — the unit that triggered an event (paired with reactive triggers).
- `GetCurrentTarget` — the unit's current movement target.

## Filter strategies (chained)
Applied in array order, each narrows the candidate list:
- `MovementFilterClosest` — keep N nearest to source.
- `MovementFilterRaceClassElement` — keep only matching race/class/element.
- `MovementFilterLowestHp` — keep N with lowest HP%.
- `MovementFilterHighestThreat` — keep N with highest damage dealt this battle.
- `MovementFilterRandom` — keep N random.
- `MovementFilterInRange` — keep within `ability.range`.
- `KeepCurrentTargetFilter` — if attacker has a movement target, prefer it.

## Effect strategies
Existing (carry forward, port 2D → 3D):
- `DamageEffect` — physical or elemental damage with crit.
- `KnockBackEffect` — port from `Rigidbody2D.AddForce` to NavMesh + DOTween.
- `UnitEffectsEffect` — applies one of 24 status effects with duration.
- `ChangeSizeEffect` — visual scale change (used by evolution).
- `AttackSpriteSpawnEffect` → `AttackVfxSpawnEffect` — particle/mesh VFX.
- `SpawnTargetProjectilePrefabEffect` — fires a projectile at target.
- `SpawnTargetPrefabOnProjectileEffect` — chained spawn (impact spawn).
- `NoEffect` — null op (used for "trigger only" abilities).

New for CW2:
- `HealEffect` — restore HP.
- `ReviveEffect` — bring a downed ally back at X% HP.
- `BuffStatEffect` — temporary stat modifier.
- `TeleportEffect` — instant move (uses NavMesh.SamplePosition + DOTween).
- `EvolveEffect` — triggers evolution tier increase (in-run only).

**Removed**: `CaptureUnitEffect` (capture mechanic removed entirely).

## Authoring flow
1. Right-click in `Data/Abilities/` → Create → CapsuleWars/Ability.
2. Configure name term key, cooldown, range.
3. Drag in strategy SO instances from `Data/Abilities/Triggers/`, `/Targeting/`, `/Filters/`, `/Effects/`.
4. Pick required weapon class(es) or leave empty.
5. Test in `PlayMode` test or sandbox scene.

## Cost model
**Cooldown only.** No mana, no energy, no charge meters at MVP. The cooldown is gameplay-tuned per ability; very strong abilities have long cooldowns.

## Weapon coupling (NEW)
Sprite Wars had no weapon class concept. CW2 introduces:
- `WeaponClass_SO` (see `03_AnimationController.md`).
- `Ability_SO.requiredWeaponClasses` — empty array = any weapon, otherwise the equipped weapon must match.
- At battle start, `UnitAttackController` validates each ability against the equipped weapon. Mismatched abilities are flagged `Locked` and don't fire.
- UI displays locked abilities greyed out with a tooltip explaining why.
- Fallback: if all of a unit's abilities are locked, the unit still uses the weapon's basic attack from its sub-state machine.

## Evolution-indexed strategies
Each strategy axis is an array indexed by `evolutionTier`. At evolution, the unit picks `triggers[tier]`, `targeting[tier]`, etc. This allows:
- Tier 0: 1 target, low damage.
- Tier 1: 3 targets, medium damage.
- Tier 2: cleave + knockback, high damage.

If `tier >= array.Length`, clamp to last valid index.

## Stat events fired
- `OnAbilityCast(ability, source)`
- `OnAbilityHit(ability, source, target)`
- `OnAbilityResolved(ability, source, targets[])`

These feed `BattleStatsAggregator` for per-run counters and UI.

## Open items
- Whether to bake the strategy chain into a single compiled `IAbilityRuntime` at battle start for perf, or evaluate the SO chain each cast (default: evaluate per cast — clearer, profile if it's a problem).
- Authoring tool: a Unity Editor window to visualize the strategy chain. Post-MVP polish.
