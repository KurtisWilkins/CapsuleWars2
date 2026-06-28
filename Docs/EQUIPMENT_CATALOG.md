# CapsuleWars2 — Equipment Catalog (canonical)

> **Pipeline role:** the equipment analog of `RACE_ROSTER.md`. Every weapon and armor piece
> occupies **one of six slots**. Gear generates through the SAME grayscale Grok→Meshy pipeline as
> body parts, and is colored by the SAME region-tint system (primary = metal, secondary =
> leather/cloth/wood, accent = trim/gems/glow). **Gear is shared across all 213 races** — a sword
> is a sword regardless of who holds it.

## The six equipment slots
| # | Slot | Holds |
|---|------|-------|
| 1 | **Helmet** | helmets, hoods, hats |
| 2 | **Chest** | chest armor |
| 3 | **Shoulders** | shoulder pads |
| 4 | **Back** | cape · shield-on-back · quiver |
| 5 | **Right hand** | weapons / held items |
| 6 | **Left hand** | weapons / held items / shields |

Everything below maps into exactly one of these. There are no other slots.

---

## Slot 1 — Helmet
Helmets · Hoods · Hats

## Slot 2 — Chest
Chest armor (plate, robe, leather, etc.)

## Slot 3 — Shoulders
Shoulder pads / pauldrons

## Slot 4 — Back  *(one item at a time)*
Cape · Shield-on-back · Quiver
> If a unit ever needs cape + quiver simultaneously, Back becomes a 2-sublayer slot — flag if so.

## Slots 5 & 6 — Hands  (the weapon system)
All weapons and held items live here, governed by a **handedness** tag:

| Handedness | Slot use | Items |
|------------|----------|-------|
| **1H (one-handed)** | either hand, 1 slot | sword · mace · axe · spear · dagger · hammer/blunt · javelin |
| **2H (two-handed)** | consumes BOTH hands | greatsword · 2H mace · 2H axe · 2H spear · 2H hammer · bow · crossbow · musket · wizard staff |
| **Off-hand** | left/off hand, pairs with a 1H main | shield · spellbook · potion · dagger (off) |
| **Dual / paired** | one per hand (both) | dual claws (matched mirror pair) |
| **Thrown** | hand, consumed/launched | bomb · javelin (also held) |

### Weapon families (1H and 2H where noted)
- **Sword** — 1H, 2H
- **Mace** — 1H, 2H
- **Axe** — 1H, 2H
- **Spear** — 1H, 2H
- **Hammer / blunt** — 1H, 2H
- **Dagger** — 1H / off-hand
- **Javelin** — 1H / thrown
- **Bow** (regular) — 2H
- **Crossbow** — 2H
- **Musket** — 2H
- **Wizard staff** — 2H
- **Spellbook** — off-hand
- **Dual claws** — paired, one per hand
- **Shield** — off-hand (many variants: round, kite, tower, buckler…)
- **Potion** — off-hand / consumable
- **Bomb** — thrown

### Hand-slot rules
- A **2H** weapon fills both hand slots → no off-hand/shield possible while equipped.
- **Dual claws** are a matched **mirror pair** — generate one, mirror for the other hand (existing mirror utility).
- **Shield** is off-hand → pairs with any 1H main-hand.
- A 1H weapon in the **left hand** may be the mirror of the right-hand mesh.

---

## Not slot items — projectiles / VFX
- **Arrow** — ammunition fired by bow/crossbow; a projectile handled by the combat/VFX system, NOT
  an equippable slot item. (The **quiver** is the Back-slot cosmetic.)
- **Bomb / javelin in flight** — the held item is the slot item; the in-air projectile is VFX.

---

## Open design decisions (flagged, not assumed)
1. **Gender on gear — recommend GENDER-NEUTRAL.** Bodies are M/F, but gear is slot-attached in the
   floating-limb style and should read on both. Making gear gender-neutral avoids doubling every
   armor/weapon mesh (gear already serves all 213 races; don't also 2× it for gender). The only
   edge case is body-conforming armor (**chest**, **shoulders**) — revisit those two only if they
   visibly don't fit both torsos. Everything else: one mesh, both genders.
2. **Handedness is runtime CODE, not assets.** The 1H/2H/off-hand/dual rules above (2H consumes both
   slots, dual = paired, shield off-hand) are equipment-system logic — a code task to track in
   `TASKS.md`, like the gender-selection logic. Assets just carry a handedness tag.
3. **Back slot single vs layered** — currently one item; layer only if cape + quiver must coexist.

## Coloring (region-tint, same as bodies)
Gear uses primary / secondary / accent slots: **primary = metal**, **secondary = leather / cloth /
wood**, **accent = trim / gems / glow**. Region masks define which area is which. Gender-agnostic.
