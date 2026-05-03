# Unit System

## Concept
A unit is a unique entity with swappable parts, a class, an ability set, equipment, and a stat block. Two units of the same class are different units — different limbs, names, colors, abilities. Identity persists across runs for legacy units.

## Composition (3D)
```
Capsule body
├── Face decals (eyes + mouth — painted texture or decal projector)
├── LeftHand   (independent mesh slot)
├── RightHand  (independent mesh slot)
├── LeftFoot   (independent mesh slot)
├── RightFoot  (independent mesh slot)
├── Head prop  (optional — hat, horns, halo)
└── Weapon     (optional — drives Animator sub-state machine)
```
Left/right are independent — asymmetric mixing is allowed once both options are unlocked.

## Identity
Every unit carries:
- `Guid id` (stable across runs)
- `string nameTermKey` — for localized name (or random name from `UnitNameGenerator`)
- `int level` — resets to 1 every run
- `int evolutionTier` — in-run only, resets at run end (see `04_CombatSystem.md`)
- `UnitClass_SO classRef`
- `ElementType_SO[] elements` — primary + secondary

## Stat block
| Field | Notes |
|---|---|
| `Atk` | physical attack |
| `AtkElem` | elemental attack (uses element chart) |
| `HP` | max hit points |
| `Def` | physical defense |
| `DefElem` | elemental defense |
| `Speed` | move + cooldown rate |
| `Accuracy` | hit chance |
| `Resistance` | status effect resist |
| `CritRate` | crit roll out of 1000 |
| `CritDmg` | crit multiplier |
| `Mass` | knockback resistance |

`UnitDTO` is the serialized form. `UnitStatusController` is the runtime form. **Stat math lives in exactly one place** (`Combat.Stats.StatCalculator`). The Sprite Wars duplication between `UnitDTO.Build*Mod` and `UnitStatusController.Build*Mod` is collapsed.

## Controllers (runtime)
Split, communicating via C# events. One `MonoBehaviour` per controller, sitting on the unit root GameObject.

| Controller | Owns | Fires |
|---|---|---|
| `UnitStatusController` | base + modified stats, status effect flags, level/evolution | `OnStatusApplied`, `OnStatusExpired`, `OnEvolved` |
| `UnitHealthController` | currentHealth, dead flag, hit flash | `OnHealthChanged`, `OnDowned`, `OnRevived` |
| `UnitAttackController` | basic + passive ability refs, cooldown timers | `OnBasicAttack`, `OnPassiveAttack`, `OnAbilityUsed` |
| `UnitMovementController` | NavMesh agent, target acquisition, facing | `OnTargetChanged`, `OnAttackedBy` |
| `UnitAnimationController` | Animator parameter setting, weapon sub-SM selection | `OnAnimationEvent` |
| `UnitLevelUp` | XP accumulation, level threshold checks | `OnLevelUp`, `OnEvolutionThreshold` |

Controllers do not call each other directly — they subscribe to each other's events at `Start()` and unsubscribe in `OnDestroy()`.

## DTO/runtime split
- `UnitDTO` — serializable, no Unity references except by ID.
- `Unit` (runtime composite) — references SOs from `Database`.
- Conversion: `UnitFactory.FromDTO(UnitDTO, Database) → Unit` and `UnitDTO.FromUnit(Unit) → UnitDTO`.

`UnitInventory` from Sprite Wars (46 SpriteRenderer fields) is replaced by `UnitCustomization`:
- Holds slot → mesh-renderer references built at spawn time.
- Applies `BodyPart_SO`, `Palette_SO`, and equipment meshes.
- Single `Refresh()` method rebuilds visuals from the current DTO.

## Customization unlocks
- Player starts with: 2 body types, 2 hand types, 2 foot types, 1 palette.
- Unlocks earned via meta-progression (see `12_RoguelikeRun.md`).
- Asymmetric mixing allowed for any unlocked part.
- Unlocks are **player-account-wide**, not per-unit.

## Naming
- Default: `UnitNameGenerator` returns one of 129 historical warrior names (existing list).
- Player can rename in legacy roster screen.
- Names are display-only; identity uses `Guid id`.

## Lifetime stats (legacy only)
Tracked on `LegacyUnitProfile`, separate from per-battle counters:
- Lifetime kills, damage dealt, damage taken, heals done/received, revives done/received, faints, battles fought, battles won, runs participated in.
- Updated only when a legacy unit finishes a run; roguelike-only units never write here unless promoted at run end.

See `11_StatsTracking.md` for the full event surface.

## Open items
- Final stat ranges and growth curves per level (tuning, post-M5).
- Whether secondary element is unlockable mid-run.
- Whether evolution triggers are class-specific or universal (default: universal level thresholds).
