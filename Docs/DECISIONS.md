# Decisions — CapsuleWars2

> Short, append-only record of choices that shape the project, so a future
> session doesn't re-litigate them. One entry per decision.

### ADR-001 — Art style: consistent AI-friendly pixel art
**Decided:** adopt a single coherent pixel-art style rather than style-matching
across many character race heads. (Shovel Knight-style pixel art was evaluated
as a strong reference.)
**Why:** consistency + far cheaper to generate/maintain solo than per-race
bespoke matching.

### ADR-002 — Live editing via CoplayDev Unity MCP bridge
**Decided:** Claude Code edits the Unity project through the CoplayDev MCP bridge
(~v9.7.1) connected to the editor.
**Why:** lets Claude read/modify scripts and assets in-place instead of
copy-pasting.
**Implication:** Claude can't see Play Mode results — gameplay/visual changes
must be human-verified (tracked in PROJECT_STATE.md). The bridge also drops on
every domain reload and cycles ports; reconnect and continue.

### ADR-003 — UI is uGUI (not UI Toolkit)
**Decided:** build all new UI (deployment grid, inspection, customization) with
legacy uGUI + the existing theme system (`UIThemePalette`/`UIThemeApplier`).
**Why:** matches all existing UI; lowest friction. `manage_ui` (UXML/USS) is for
UI Toolkit and is deliberately not used.

### ADR-004 — Target mobile + desktop
**Decided:** design input/layout for both. New input uses the Input System
(`activeInputHandler = 1`; legacy `UnityEngine.Input` is off) with mouse +
keyboard + touch (drag/tap, pinch). Build targets include Android/iOS.

### ADR-005 — Armor carries stats; body parts are cosmetic
**Decided:** `Equipment_SO` (armor/weapons) carries `StatBuffs`; `BodyPart_SO`
parts stay visual-only via `UnitCustomization`.
**Why:** keeps the stat/balancing surface small; matches the existing model.

### ADR-006 — Run-scoped persistence (EquipmentDTO + RunStateDTO)
**Decided:** equipment + the in-run party + deployment placements persist at the
run level via `RunStore` (`run.json`), so a mid-run loadout survives an app
restart. Equipment ids resolve through an `IEquipmentDatabase` at spawn time.
**Why:** the realistic "persist through JSON" requirement for between-rounds
customization. `RunStateDTO` stores `NodeType`/coords as ints so the Persistence
assembly never references the Run/Combat assemblies.

### ADR-007 — No StatCalculator class; stats live in UnitStatusController
**Decided:** there is **no** separate `StatCalculator`. Final stats are computed
by `UnitStatusController` read-only getters (`MaxHp/Atk/Def/Speed`) that fold
base + equipment(×rarity) + status effects + class-synergy buffs.
**Why:** the computation was already cleanly encapsulated and read-only, so the
inspection/customization UI consumes it directly — no extraction needed. (Docs
or tasks that mention "StatCalculator" mean this controller.) A lightweight
`OnStatsChanged` event was added so UI can refresh live.

### ADR-008 — Deployment: spawn-then-arrange + cell-based selection
**Decided:** units spawn first (`BattlePartySpawner`), then the player arranges
them on the grid during `PreBattle`; placements persist battle-to-battle
(`RunState.Placements`). Selection is **cell-based** (raycast ground/tiles → look
up the cell's occupant) because the unit prefabs have **no colliders**.
`DeploymentManager.AutoArrange()` seeds every player unit onto a free deploy-zone
cell so it's always selectable. The 9×7 grid is a placement tool only — combat
movement stays NavMesh-based.

### ADR-009 — Feature work on stacked per-slice branches
**Decided:** the battle/customization features were built as separate branches
stacked off `claude/unit-factory`: equipment-persistence → deploy-camera →
unit-inspection → deployment-grid. The pre-existing EditMode baseline fix is its
own commit (`450d3f3`).
**Why:** keeps each slice independently reviewable; `unit-factory` isn't on
`main` yet, so stacking avoids an unprompted merge.

<!-- Add new decisions below as ADR-010, ADR-011, ... -->
