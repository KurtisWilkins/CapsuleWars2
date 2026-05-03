# Class Synergies

## Concept
TFT-style. When N+ deployed units share a class, the team gains a buff that stacks at thresholds. Race-based synergies are dropped; race remains as visual flavor only.

## Data
`UnitClass_SO`:
```
├── string nameTermKey         // "Class.Warrior.Name"
├── string descTermKey
├── Sprite icon
├── ClassSynergyTier[] tiers   // sorted by threshold
└── DefaultTargetingProfile defaultProfile  // see 06_TargetingAI.md
```

`ClassSynergyTier`:
```
├── int threshold              // e.g. 3, 5, 7
├── string descTermKey         // localized buff description
├── StatBuff[] teamBuffs       // applied to all units of this class
└── StatBuff[] globalBuffs     // applied to entire team (rare, high tiers)
```

## Example: Warrior
| Threshold | Effect |
|---|---|
| 3 | +15% HP to all Warriors |
| 5 | +30% HP, +10% Atk to all Warriors |
| 7 | +50% HP, +20% Atk, all team gains +5% Atk |

## Resolution timing
Synergies recompute at:
- Battle start (after deployment confirmed).
- Whenever a unit is downed (synergy may fall below threshold).
- Whenever a unit is revived.

`Combat.Stats.SynergyResolver.RecomputeSynergies(BattleContext)` rebuilds buffs and re-runs `StatCalculator.BuildModifiedStats` for affected units.

## Stacking
A unit can contribute to multiple synergies if it has multiple classes. **MVP: one class per unit.** Multi-class is post-MVP; carrying that flexibility in the data model now.

## UI
- Pre-battle deployment screen shows live synergy preview as units are placed.
- Active synergies bar above the battlefield during combat (compact, non-intrusive).

## Open items
- Final class list and threshold tuning (post-balance pass).
- Whether enemy teams have synergies. **Default: yes** — same system, makes elite/boss compositions thematic.
