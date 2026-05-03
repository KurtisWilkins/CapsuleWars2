# Roguelike Run

## Run shape
- Length: ~15 floors (target; tunable).
- Branching map, StS-style: nodes connect upward; player chooses one of several next nodes per step.
- Run ends at the boss node or on first battle loss.

## Node types
| Type | Description |
|---|---|
| **Combat** | Standard battle. Reward: gold + chance of equipment + chance of unit recruit. |
| **Elite** | Harder battle. Reward: guaranteed equipment, larger gold drop, +1 unit recruit chance. |
| **Shop** | Spend gold on equipment, unit slots, run-modifiers, heals. |
| **Event** | Narrative choice with stat or item outcomes. |
| **Rest** | Heal a percentage of HP across roster, or upgrade one ability/equipment. |
| **Treasure** | Free equipment drop. No combat. |
| **Boss** | Floor 15 (or final). Run ends on result. |

Node frequencies per floor follow a curve; first 3 floors are mostly Combat to teach pacing.

## Map generation
`MapGenerator` builds a node graph at run start:
1. Start node + boss node fixed.
2. Generate N intermediate floors with M paths each.
3. Place node types using a weighted distribution per floor depth.
4. Validate connectivity (every path reaches boss).
5. Persist to `RunStateDTO` for save/resume.

## Economy

### Gold sources
- Combat win: 10–25 base, +1 per unit downed cleanly (no faints).
- Elite win: 30–60.
- Treasure: 0 (gives equipment, not gold).
- Event: variable.

### Gold sinks
- Shop equipment: `baseCost × rarity.statMultiplier × (1 + floor × 0.1)`.
- Shop unit slot expansion: 50/100/200 (escalating).
- Shop heal: 30 to restore one unit to full.
- Shop ability/equipment reroll: 25.

Numbers are stubs; tune in M7.

## Difficulty scaling
`DifficultyCurve` is an `AnimationCurve` SO mapping floor → enemy stat multiplier.
- Floor 1: ×1.0
- Floor 5: ×1.4
- Floor 10: ×2.0
- Floor 15 (boss): ×3.0

Stats scaled: enemy HP, Atk, Def. Speed and abilities are not scaled (would feel unfair).

## Random unit generation (in-run)
At Combat / Elite / Event, drops can include a recruitable unit. Generation:
1. Pick `UnitClass_SO` weighted by floor (some classes only appear later).
2. Pick `ElementType_SO` (primary + secondary).
3. Pick body/hand/foot parts from unlocked pool.
4. Generate name from `UnitNameGenerator`.
5. Roll abilities: 1 basic + 0–1 passive, weighted by class.
6. Roll starting equipment: 0–2 pieces.
7. Stat block: class baseline × rarity multiplier × small random variance.

The unit lives in the roguelike-only pool until run end, when surviving units are offered for legacy promotion.

## Run state persistence
A run can be saved and resumed. `RunStateDTO`:
```
├── Guid runId
├── DateTime started
├── int seed
├── List<UnitDTO> deployedRoster        // includes legacy + roguelike-only
├── int gold
├── List<EquipmentDTO> inventory
├── MapStateDTO map                     // graph + visited + current node
├── RunStatsTrackerDTO runStats
└── int saveVersion
```

Saved at every node transition. Loaded if user reopens app mid-run.

## Customization unlocks
Meta-progression earned by run **completion** (win or loss above floor X) — not per battle. Adds points to `PlayerProfile.unlockPoints`. Player spends these in the Customization screen between runs.

Unlock examples:
- New body shape: 3 points.
- New hand variant: 2 points.
- New palette: 1 point.
- New class slot in random generation: 5 points.

## Open items
- Whether unit slots are bought permanently or per-run. Default: per-run (forces tradeoff).
- Whether Rest nodes can permanently upgrade an ability tier or just heal. Default: heal only at MVP; upgrade post-MVP.
- Boss design — scripted vs. procedurally composed. Default: hand-authored boss `UnitDTO` + scripted phases.
