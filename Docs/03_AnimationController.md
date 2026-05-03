# Animation Controller

## Goal
One Animator per unit, weapon-agnostic at the call-site. Code says "play attack 2"; the Animator routes to the correct weapon's Attack2 state.

## Architecture
```
Animator (top level)
├── Parameter: int WeaponType
├── Parameter: float Speed (0=idle, 0.5=run, 1=stunned)
├── Parameter: trigger AttackTrigger
├── Parameter: int AttackIndex
├── Parameter: trigger DeathTrigger
├── Parameter: trigger ReviveTrigger
├── Parameter: trigger HitTrigger
├── Parameter: bool Blocking
└── Layer 0: Base
    └── Sub-state machines (one per weapon class):
        ├── Unarmed
        ├── 1H_Sword
        ├── 1H_Sword_Shield
        ├── 1H_Mace
        ├── 1H_Dagger
        ├── 1H_Spear
        ├── 2H_Sword
        ├── 2H_Axe
        ├── 2H_Spear
        ├── 2H_Bow
        ├── 2H_Crossbow
        ├── 2H_Staff
        └── 2H_Shooting
└── Layer 1: AbilityOverride (additive, weight 0/1)
```

Each sub-state machine contains the same set of states by name:
`Idle`, `Walk`, `Run`, `Attack1`…`AttackN`, `Death`, `GetHit`, `Stunned`, `Revive`, `Block`.

The `WeaponType` parameter selects which sub-state machine is active via top-level transitions.

## Weapon → sub-SM mapping
`WeaponClass_SO` carries:
- `int weaponTypeId` (matches Animator parameter value)
- `int attackCount` (e.g. bow=2, sword=6, staff=6)
- `WeaponHandedness` (1H, 2H, dual)
- `bool allowsShield`

`UnitAttackController` reads `equippedWeapon.weaponClass.weaponTypeId` and sets `Animator.SetInteger("WeaponType", id)`.

## Attack selection
`UnitAttackController.PlayBasicAttack()`:
1. Pick attack index — strategy on `WeaponClass_SO`: `Random`, `Sequenced`, or `AbilityChosen`.
2. Set `AttackIndex` int parameter.
3. Set `AttackTrigger`.
4. Sub-SM routes to `AttackN` where N matches `AttackIndex`.

If `AttackIndex > weaponClass.attackCount`, clamp.

## Ability override
Some abilities have custom animations (e.g. rogue backstab). Two options, picked per ability:

**Option A — Use weapon's existing attack slot.**
- Ability declares `int useAttackIndex` (e.g. `2` to use the weapon's Attack2).
- Most abilities use this; weapons share visual vocabulary.

**Option B — Override layer.**
- Ability declares an `AnimationClip` and a `bool useOverrideLayer`.
- `UnitAnimationController` enables Layer 1, plays the clip, disables on completion.
- Used for highly distinctive moves that don't map to any weapon attack (transformations, summons).

Weapon-class gating: if the ability declares required `WeaponClass[]` and the equipped weapon is not in the list, the ability is locked at battle start (greyed out in UI). Attack falls back to weapon's basic.

## Animation events
Attack states fire animation events at hit-frames. The state's behaviour is a small `AttackHitEventReceiver` script that calls `UnitAttackController.OnHitFrame(attackIndex)`. That's where damage resolution and ability effects fire — **not** at attack start. This keeps visuals aligned with damage timing.

## Status effect animations
- `Stunned` sub-SM state — looped while `Speed == 1f`.
- `Frozen` — additive layer, plays an ice shader (no sub-SM change needed).
- `Trapped`, `Cursed`, `Marked` — VFX on the unit, not animation states.

See `10_StatusEffects.md` for the full mapping.

## Death + revive
- `Death` state plays once, transitions to a "downed pose" hold (no auto-destroy).
- `Revive` trigger plays an animation that returns to `Idle`.
- For end-of-battle KO, the unit stays in downed pose until the battle ends, then returns next battle (see `04_CombatSystem.md`).

## Open items
- Should equip animations exist, or is gear visually swapped instantly? Default: instant swap during deployment, no equip animation.
- Whether ranged units share a single bow draw cycle or each weapon gets unique timings. Default: per-weapon timings authored in the sub-SM.
