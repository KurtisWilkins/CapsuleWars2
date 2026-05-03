# Status Effects

## Catalog (24 total — Sprite Wars carry-forward)
All effects implemented as `StatusEffect_SO` instances. Behavior is data-driven where possible; complex effects get a custom `StatusEffectBehavior` SO.

### Control / debuffs (10)
| Effect | Term key | Behavior |
|---|---|---|
| Stunned | `Status.Stunned` | Cannot act or move; Animator `Speed=1f` triggers Stunned state |
| Frozen | `Status.Frozen` | Cannot act; physical damage taken ×1.5; Frost shader overlay |
| Trapped | `Status.Trapped` | Cannot move; can attack within range |
| Marked | `Status.Marked` | Takes +25% damage from all sources |
| Unlucky | `Status.Unlucky` | Crit chance ÷2; ability roll outcomes skewed worst |
| LastStand | `Status.LastStand` | At <20% HP, gain +50% Atk and damage reduction; one-time |
| Madness | `Status.Madness` | Targets random units (incl. allies) |
| Cursed | `Status.Cursed` | -25% all stats |
| Silenced | `Status.Silenced` | Cannot use abilities (basic attack still works) |
| Bleeding | `Status.Bleeding` | DoT, % of max HP per second |

### Buffs / boons (4)
| Effect | Term key | Behavior |
|---|---|---|
| Protected | `Status.Protected` | Next incoming hit fully negated; consumed on use |
| Shield | `Status.Shield` | Absorbs N flat damage before HP |
| Regenerating | `Status.Regenerating` | HoT, % of max HP per second |
| Empowered | `Status.Empowered` | +25% Atk, persists for duration |

### Stat buff/debuff pairs (10 — buff + broken variant)
Each stat buff has a "boosted" form and a "broken" form (debuff). 5 stats × 2 = 10 effects.

| Stat | Boosted | Broken |
|---|---|---|
| Atk | `Status.AtkUp` | `Status.AtkDown` |
| Def | `Status.DefUp` | `Status.DefDown` |
| Speed | `Status.SpeedUp` | `Status.SpeedDown` |
| Accuracy | `Status.AccuracyUp` | `Status.AccuracyDown` |
| CritRate | `Status.CritUp` | `Status.CritDown` |

(Sprite Wars also tracked CritDmg + Resistance variants — those collapse into the above for simplicity, or expand back if balancing demands.)

## Data model
```
StatusEffect_SO
├── string nameTermKey
├── string descTermKey
├── Sprite icon
├── StatusEffectKind kind            // Buff | Debuff | Control | DoT | HoT
├── float defaultDuration             // seconds; -1 = until manually cleared
├── int maxStacks                     // most are 1
├── StackBehavior stackBehavior       // Refresh | Add | Independent
├── ResistanceCheck resistance        // None | RollOnApply | RollPerTick
├── StatusEffectBehavior behaviorSO   // optional custom logic
└── List<StatBuff> statBuffs          // declarative stat modifiers
```

## Application
1. Effect source calls `target.UnitStatusController.ApplyStatus(StatusEffect_SO, source, durationOverride)`.
2. `UnitStatusController` checks resistance via `RollResist(target.Resistance, effect.resistance)`.
3. If applied, adds to active effects list; tracks remaining duration in `Update`.
4. Fires `OnStatusApplied(effect, source)` on `BattleEventBus`.

## Lifetime management
Sprite Wars stored coroutines per status — fragile (orphaned on unit destroy). CW2 uses a flat `List<ActiveStatus>` with per-frame duration ticking. `OnDestroy()` clears the list cleanly. No dangling coroutines.

## Stacking
- `Refresh` — reapplying refreshes duration to max (default).
- `Add` — duration adds (capped).
- `Independent` — multiple independent instances tick separately.

## Animator integration
Some effects map to Animator state changes:
- `Stunned` → Stunned sub-state.
- `Frozen` → ice shader + `Speed=0`.
- `Madness` → no animator change; targeting changes only.
- Most effects → VFX only, no animator change.

## Resistance
`UnitStatusController.RollResist(resistance, effectResistance)`:
- If `effectResistance == None`, always applies.
- If `RollOnApply`, roll once: `Random < (effectAccuracy - resistance) / 1000`.
- If `RollPerTick`, roll each tick.

## Localization
- Names + descriptions all I2 term keys.
- DoT/HoT damage numbers color-coded per effect kind.

## Open items
- Final balance numbers per effect.
- Whether `Madness` should target only allies (not self) — default no, fully random.
- Whether `Protected` stacks (multiple guaranteed negations) — default no, idempotent.
