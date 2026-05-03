# Legacy Mode

## Purpose
Persistent roster of unique units the player owns across runs. Visual identity persists; level/stats reset each run; lifetime stats accumulate forever.

## Constraints
- Cap: ~100 units stored on device (configurable, soft cap).
- Per-run: deploy up to ~10 in any single battle, with 10–20 on bench (swappable between battles).
- Bench size scales with floor or shop upgrades — TBD.

## Drafting flow
Pre-run:
1. Player opens "New Run".
2. Legacy roster screen appears with all owned units.
3. Player selects up to N units (default 20) to bring into the run as the starting roster.
4. Player confirms; run begins.

There is no randomization in *which* legacy units appear — the player picks deterministically. Randomness is reserved for roguelike-only unit drops during the run.

## Unit lifecycle
| Phase | What happens to a legacy unit |
|---|---|
| Stored on device | Visuals + identity preserved; level=1, stats reset to base per save. |
| Drafted to a run | Loaded into runtime as `Unit`. Level = 1. Lifetime stats untouched. |
| Battle | Earns XP, can be downed (returns next battle at 50% HP). |
| Between battles | Bench swap allowed; equipment can be reassigned. |
| Run ends (win) | Per-run stats merged into `LegacyUnitProfile.Lifetime`. Level discarded. |
| Run ends (loss) | Same as win — per-run stats merge regardless of outcome. |

## Equipment policy
Equipment is **per-run only**. At run end, all equipment is discarded. Legacy units strip down to base parts. This keeps legacy progression about identity and lifetime stats, not gear hoarding.

If we later want persistent equipment, that's a separate "vault" system.

## Recruiting roguelike-only units
End of run (win only):
1. Show all surviving roguelike-only (non-legacy) units on a recruit screen.
2. Player picks up to K units to promote (K default = 3, scaling with floor reached).
3. Promoted units join the legacy roster.
4. If roster is at cap, prompt to release one to make room.

End of run (loss): no promotion offered. Roguelike-only units discarded.

## Roster management UI
- Grid view of all legacy units.
- Filters: class, element, primary stat.
- Sort: created date, lifetime kills, level (during a run).
- Per-unit detail screen: appearance editor, lifetime stats, run history.
- Rename, retire (delete), favorite-tag.

## Lifetime stats display
Per-unit profile shows:
- Total kills, damage dealt/taken, heals, revives, faints across all runs.
- Best run reached.
- Battles won/lost.
- "Days alive" (since creation).
- Run participation list (last 10).

Drives social/cosmetic milestones later (e.g. "Veteran" badge at 100 kills).

## Persistence
`LegacyProfileDTO`:
```
├── int saveVersion
├── int rosterCap
├── List<LegacyUnitDTO> roster
└── PlayerProfileDTO playerProfile     // unlocks, settings
```

`LegacyUnitDTO`:
```
├── UnitDTO baseUnit                    // identity, parts, abilities
├── LifetimeStatsDTO lifetime
├── DateTime created
├── DateTime lastPlayed
└── List<RunSummaryDTO> recentRuns      // last 10
```

See `14_Persistence.md` for storage details.

## Open items
- Whether to allow gifting/sharing units between players' devices (QR / file export). Post-MVP.
- Whether legacy units have evolution unlocks that persist (some piece of progression that *is* permanent). Default: no — keeps the per-run progression hook clean.
