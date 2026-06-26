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
- ~~Final class list~~ **RESOLVED** — see **Canonical Class Roster (16)** below. Threshold/buff numbers are
  first-pass and tunable in a later balance pass.
- Whether enemy teams have synergies. **Default: yes** — same system, makes elite/boss compositions thematic.

---

# Canonical Class Roster (16) — source of truth

> Reconciled from the design owner's roster (build-to-spec session, 2026-06-25). Supersedes the "Example: Warrior"
> placeholder above. The 16-class roster + identities are **LOCKED**; all thresholds/buff values are **FIRST-PASS
> and tunable** (TFT-style **2 / 4 / 6** ladder). Source tags: ✍️ design-owner · 🔎 archetype-inferred.
> **Monk is its own hybrid class (NOT multi-class)** — confirmed this session; one-class-per-unit holds at MVP.
> Term keys: `Class.Barbarian.Name`, `Class.Barbarian.Tier1.Desc`, etc.
>
> **Build note:** pure `StatBuff` tiers (Atk/Def/MaxHp/Speed/Accuracy/Resistance/CritRate/CritDmg %) are
> **[content]** — author directly. Tiers tagged **[code]** are behavioral (armor-pen, DoT, splash, cleanse,
> strike-first, on-hit/kill heal, ramps, conditional-HP, double-first-shot, pierce, backline-open, reposition,
> range/cooldown mods) — build as ability/effect strategies or synergy combat hooks, never faked in the stat
> layer. `globalBuffs` (whole-team tiers, tagged "global") needs the `ClassSynergyTier.globalBuffs` field, which
> the spec above lists but the code lacks — see TASKS. Most classes also need a `WeaponClass_SO`.

| # | Class | Weapon class | Synergy | Tier 2 | Tier 4 | Tier 6 |
|---|---|---|---|---|---|---|
| 1 | Barbarian 🔎 | 2H Melee (axe/greatsword) | Bloodrage | +12% Atk | +25% Atk; **heal 5% maxHP on kill** [code] | +40% Atk; **+15% Atk while <50% HP** [code] |
| 2 | Fighter ✍️ | Dual 1H Melee | Flurry | +15% Speed | +30% Speed | +30% Speed; **every 3rd hit strikes twice** [code] |
| 3 | Archer 🔎 | Bow | Volley | +12% Atk, +10% Accuracy | +25% Atk; **+3%/s atk-speed ramp (cap +30%)** [code] | +25% Atk; **ramp cap +60%** [code] |
| 4 | Spearman 🔎 | Spear | Phalanx | **+1 range** [code]; +10% Accuracy | +20% Def front-row [code-cond] | **strike first each exchange** [code] |
| 5 | Heavy ✍️ | 1H + Tower Shield | Bulwark | +20% Def | +20% Def, +15% MaxHp; **−15% dmg from ranged** [code] | +20% Def, +15% MaxHp; **+8% Def team** (global) |
| 6 | Wizard 🔎 | Staff/Wand | Arcane | +15% elem ability dmg | +30%; **−10% ability cooldown** [code] | +30%; **+10% elem ability dmg team** (global) |
| 7 | Javelin ✍️ | Thrown Javelin | Skirmish | +12% Atk at range | +12% Atk; **reposition dash / 4th throw** [code] | +25% Atk; **pierce first target** [code] |
| 8 | Alchemist ✍️ | Thrown Potion (offensive) | Volatile | **DoT + small splash** [code] | **+25% DoT; splash up** [code] | **+25% DoT; −Def debuff** [code] |
| 9 | Cleric ✍️ | Staff/Holy focus | Blessing | +20% healing | +20%; **+2%/s HP regen team** (global)[code] | +35%; **+4%/s regen team** (global)[code] |
| 10 | Ambrosian ✍️ | Thrown Potion (support) | Elixir | **area heal-potions** [code] | **+25% heal; cleanse 1 on lowest-HP ally** [code] | **+25%; radius up; cleanse 2** [code] |
| 11 | Assassin 🔎 | Dagger | Execute | +15% CritRate, +20% CritDmg | …; **+20% dmg vs <40% HP** [code] | +15% CritRate, +40% CritDmg; **opens on enemy backline** [code] |
| 12 | Monk ✍️ | Fist/Staff | Harmony | +10% Atk; **heal 3% on hit** [code] | +15% Atk; **heal 5% on hit** [code] | +15% Atk, heal 5% on hit; **+5% MaxHp team** (global) |
| 13 | Crossbow 🔎 | Crossbow | Pierce | **ignore 20% Def** [code] | **ignore 35% Def**; +15% CritDmg | **ignore 50% Def**; +30% CritDmg |
| 14 | HandGunner ✍️ | Firearm (Musket) | Gunline | **+flat dmg; ignore 30% Def** [code] | **ignore 50% Def; +20% dmg, −10% Speed** [code] | **ignore 70% Def; first shot double** [code] |
| 15 | Siegebreaker ✍️ | Thrown Bomb | Demolition | **bomb AoE splash** [code] | **+25% splash; −Def armor break** [code] | **+25% splash; armor break + slow** [code] |
| 16 | Paladin 🔎 | 1H + Shield (Holy) | Aegis | +15% Def, +15% Resistance | +15% Def, +15% Res; **+10% Res team** (global) | +20% Def, +20% Res; **+10% Def & Res team** (global) |

**Pure-[content] now:** Paladin (all StatBuff tiers; its team-wide tiers need the `globalBuffs` field first). Every
other class mixes [content] stat buffs with [code] behaviors that depend on the ability/status/hook slices.

**Weapon classes to author (Class → WeaponClass_SO):** 2H-Melee, Dual-1H, Bow, Spear, 1H+TowerShield, Staff, Wand,
Thrown-Javelin, Thrown-Potion×2 (offensive/support), Dagger, Fist (Unarmed exists), Crossbow, Firearm/Musket,
Thrown-Bomb, 1H+Shield(Holy). Only Unarmed exists today; `WC_1HSword` is mislabeled "Unarmed" — fix.

**StatType:** all roster stats (MaxHp/Atk/Def/DefElem/Speed/Accuracy/Resistance/CritRate/CritDmg/Mass) exist in the
enum, BUT `UnitStatusController` only folds modified getters for MaxHp/Atk/Def/Speed — Accuracy/CritRate/CritDmg/
Resistance buffs are computed and never read until those getters are added (see TASKS).
