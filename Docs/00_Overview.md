# CapsuleWars2 — Overview

## Vision
A 3D auto-battler with a Rayman-style capsule aesthetic (floating limbs, simple geometry, distinct silhouettes). Players build armies of customizable units that auto-fight procedurally encountered enemies. Two modes share the same combat engine: **Roguelike** (StS-style map runs from scratch) and **Legacy** (persistent ~100-unit roster across runs).

## Pillars
1. **Visual identity ownership.** Every unit is a unique instance. No "same unit upgraded" — different limbs, colors, abilities, names.
2. **Roguelike + Legacy duality.** Same combat engine, different progression metaphors.
3. **No permadeath.** KO'd units return the next battle at exactly -50% max HP, then fully restored the battle after.
4. **Cohesive aesthetic.** Trolls, dragons, bosses — everything is a capsule with floating parts.
5. **Offline-first.** Core experience works on a plane with no signal. Multiplayer (peer-to-peer) is additive.

## Modes

### Roguelike
- Fresh start every run.
- StS-style branching map: ~15 floors, node types: Combat / Elite / Shop / Event / Rest / Boss / Treasure.
- Random unit drops, equipment, gold, shops.
- Per-run stats only (not written to legacy lifetime totals).
- Run loss = run ends.

### Legacy
- Up to ~100 persistent units on device.
- Player drafts a roster pre-run; deploy ~10 per battle with 10–20 on bench, swappable between battles.
- Levels reset to 1 every run; lifetime stats accumulate to the legacy profile.
- End-of-run recruit prompt: surviving roguelike-only units can be promoted into the legacy roster (subject to ~100 cap).

## Glossary
- **Unit** — battle entity with swappable body/hands/feet/head/weapon parts and an `Ability_SO` set.
- **Legacy unit** — persisted on disk with lifetime stats.
- **Roguelike unit** — per-run only, generated during a run; persists only if promoted at run end.
- **Run** — one playthrough of the roguelike map, start node to boss or death.
- **Battle** — one auto-resolved combat encounter.
- **Downed** — KO'd for the rest of a battle. Returns next battle at -50% max HP.
- **Evolution** — in-run progression that physically resizes/reshapes a unit. Resets at run end.
- **Family** — one of 5 element categories on the RPS wheel.
- **Class synergy** — TFT-style buff when N+ units of a class are deployed together.

## Tech stack
| Layer | Choice |
|---|---|
| Engine | Unity URP 6.x |
| Tweening | DOTween + DOTweenPro (mandatory for all tweens) |
| Localization | I2 Localization (mandatory for all user-facing strings) |
| Input | Unity Input System |
| Persistence | Newtonsoft.Json |
| Animations | ExplosiveLLC RPG Character Mecanim Animation Pack |
| Visual baseline | AssetHunts! Capsule kit |

## Standing rules
1. All user-facing strings → I2 term keys (`UI.*`, `Ability.*`, `Status.*`, `Element.*`, `Class.*`).
2. All tweening → DOTween. No hand-rolled `Lerp` coroutines.
3. ScriptableObjects for all data definitions.
4. Event-driven combat. UI / audio / effects subscribe; combat fires.
5. DTO/runtime split for anything that persists.
6. `.asmdef` per top-level folder; dependencies flow one direction.
7. Tests live under `Assets/Scripts/Tests/`; EditMode tests at minimum from M0.
