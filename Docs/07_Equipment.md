# Equipment

## Slots (8, fixed — Sprite Wars carry-forward)
| Slot | Notes |
|---|---|
| `Helmet` | head |
| `Shoulders` | upper torso |
| `Back` | cape, quiver, wings |
| `Chest` | armor body |
| `Arms` | bracers / sleeves |
| `Legs` | greaves / pants |
| `RightHand` | weapon (drives Animator sub-SM) |
| `LeftHand` | shield, off-hand weapon, focus |

Both hand slots can hold weapons (dual-wield) if `WeaponClass_SO.handedness == Dual`. A `2H` weapon occupies both hand slots.

## Data model
```
Equipment_SO (definition)
├── string nameTermKey
├── string descTermKey
├── EquipmentSlot slot
├── Rarity_SO rarity
├── Mesh visualMesh
├── Material[] materials
├── int Atk, AtkElem, HP, Def, DefElem, Speed, Accuracy, CritRate, CritDmg, Resistance, Mass
├── ElementType_SO elementAffinity   // optional
├── WeaponClass_SO weaponClass        // only if slot == hand
└── Ability_SO[] grantedAbilities     // optional
```

```
EquipmentDTO (serializable)
├── Guid id
├── string equipmentSoId
├── string runeSoId       // optional
└── (no stat fields — pulled from SO at runtime)
```

DTO carries only IDs; runtime stats are pulled from `Database` lookup. This avoids the duplication trap in Sprite Wars where stat values were copied into `EquipmentDTO`.

## Rarity
`Rarity_SO`:
- `int tier` (1–5)
- `string nameTermKey`
- `Color uiColor`
- `float statMultiplier` (1.0 / 1.25 / 1.5 / 2.0 / 3.0 by default)

5 tiers: Common, Uncommon, Rare, Epic, Legendary. Stat multipliers tunable on the SO.

## Stat application
At battle start, `StatCalculator.BuildModifiedStats(unit)`:
1. Start with base stats.
2. For each equipped item, add `(item.stat * rarity.statMultiplier)` to the corresponding base stat.
3. Apply class synergy buffs (see `09_ClassSynergies.md`).
4. Apply status effect modifiers (see `10_StatusEffects.md`).
5. Cache. Recalculate when status effects change.

## Set bonuses
Not in MVP. Architecture leaves room: `Equipment_SO` could later carry `EquipmentSet_SO setRef` and a `SetBonus_SO` that activates at N items equipped. Add when there's design pressure.

## Runes
Sprite Wars `Rune_SO` enchants equipment. Carry forward for parity, simple stat-mod overlay applied after rarity multiplier. One rune per item.

## Visual application
- `UnitCustomization.EquipMesh(slot, equipment)` swaps the slot's MeshRenderer.
- Material override applies tint based on rarity (subtle outline color, not the whole mesh).
- Weapon equip swaps the mesh and updates Animator `WeaponType` parameter.

## Drops
- Combat node drops: 0 or 1 piece, weighted by floor depth.
- Elite drops: 1 guaranteed piece, rarity skewed up.
- Treasure node: 1 guaranteed piece + small chance of a second.
- Shop: 4–6 pieces stocked, gold cost = `baseCost * rarity.statMultiplier * floor`.

Tuning lives on a `LootTable_SO` per node type.

## Open items
- Whether legacy units retain equipment between runs or strip on run end. **Default: strip** — equipment is a per-run resource, only the unit's identity persists.
- Whether equipment can be sold for gold. Default: yes, at 50% buy price.
