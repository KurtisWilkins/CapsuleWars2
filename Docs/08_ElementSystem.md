# Element System

## Structure
**5 families × 3 sub-types = 15 elements.** Families form a 5-way rock-paper-scissors wheel; sub-types differentiate damage profiles within a family.

> **Status: tentative.** Default reading; revisable. The alternative interpretation (15 elements in a strict 5-cycle without families) is documented at the bottom of this file in case we switch later.

## The 5-way wheel
```
          Spirit
         ↗      ↘
      Air         Earth
        ↑          ↑
      Fire ←——— Water
         ↘      ↗
            (each beats the 2 it points to)
```

Each family **strong vs.** the next two clockwise, **weak vs.** the previous two. Specifically:

| Family | Strong vs | Weak vs |
|---|---|---|
| **Fire** | Air, Spirit | Water, Earth |
| **Water** | Fire, Air | Earth, Spirit |
| **Earth** | Water, Fire | Spirit, Air |
| **Spirit** | Earth, Water | Air, Fire |
| **Air** | Spirit, Earth | Fire, Water |

Multipliers applied in damage calc:
- Strong: ×1.5
- Neutral: ×1.0
- Weak: ×0.67

## The 15 element types
Each family has 3 sub-types. Sub-types share their family's matchup table but differ in damage profile.

| Family | Sub-types | Damage profile |
|---|---|---|
| **Fire** | Flame, Magma, Solar | Burst / DoT / cleave |
| **Water** | Tide, Frost, Mist | Sustained / freeze / debuff |
| **Earth** | Stone, Crystal, Sand | Tank / reflect / blind |
| **Spirit** | Holy, Shadow, Soul | Heal / drain / banish |
| **Air** | Wind, Lightning, Sound | Mobility / stun / disrupt |

`ElementType_SO`:
```
├── string nameTermKey            // "Element.Flame.Name"
├── string descTermKey
├── ElementFamily family          // enum: Fire, Water, Earth, Spirit, Air
├── Color uiColor
├── Sprite icon
└── DamageProfile damageProfile   // SO ref or enum
```

`ElementChart_SO` (one asset, source of truth):
- `family × family → multiplier` lookup.
- Defaulted from the table above; tunable.

## Damage calculation
`StatCalculator.GetElementMultiplier(attackerElement, defenderElement)`:
1. Get attacker's family from `ElementType_SO.family`.
2. Get defender's family.
3. Lookup `ElementChart_SO[attackerFamily, defenderFamily]`.
4. Return multiplier.

For dual-element defenders (primary + secondary), take the **less favorable for attacker** multiplier (best defense).

## Sub-type interactions (post-MVP)
Sub-types within a family can have specific quirks:
- Lightning has chain-jump targeting.
- Frost applies bonus chance to inflict the `Frozen` status.
- Holy heals nearby allies on hit.

These are implemented as ability/effect properties tied to `DamageProfile`, not as base damage modifiers.

## UI
- Color-code damage numbers by element family.
- Pre-battle UI shows family wheel with attacker/defender markers.
- Tooltip on each unit lists primary + secondary element.

## Alternative interpretation (kept for reference)
**Reading B — Strict 15-cycle.** All 15 elements arranged in a fixed cycle; each beats the next 3 and loses to the previous 3. No families, no shared matchups. Cleaner math, harder to learn. If we switch, the change is contained: replace `ElementChart_SO` with a 15×15 matrix and remove `ElementFamily`.

## Open items
- Whether family RPS multipliers should be ×1.5/×0.67 or sharper (×2.0/×0.5). Default ×1.5 — tunable.
- Whether sub-types within a family have any RPS interaction with each other. Default: no, neutral within family.
