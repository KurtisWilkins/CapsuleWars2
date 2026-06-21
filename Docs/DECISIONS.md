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

### ADR-010 — Deployment Phase: place-then-spawn, confirm-gated, 7×9
**Decided:** a pre-combat Deployment Phase on a **7×9** grid (columns = X/width, rows = Z/depth;
player zone = near rows). Model = **place-then-spawn**: during deployment the field has no player
units — the player places party **tokens** from a bench (`DeploymentTray`) onto cells, and on
**Assemble** the party spawns at the placed cells (`BattlePartySpawner` deferred to
`DeploymentPhaseController.OnConfirmed`). Combat is **gated**: `BattleStateManager.StartBattle`
won't reach Active while `DeploymentRequired && !DeploymentConfirmed`. The camera auto-frames the
board on entry and restores the battle pose on Assemble.
**Why:** matches the "tray of available units → units spawn where I placed them" intent, and reuses
the existing `RunState.Placements` + spawner. Late-spawned units register fine via
`UnitRoot.OnEnable → registry.OnUnitRegistered`, so deferring spawn is safe.
**Implication / fallback:** if no `DeploymentPhaseController` is in the scene (or no run party),
`BattlePartySpawner` spawns immediately as before — existing combat is unaffected. Gate flags
default off. Selection is cell-based (units have no colliders).

### ADR-011 — Deployment: spawn-on-place + bigger split-zone board (supersedes ADR-010 token model)
**Decided:** placing a unit now **spawns the real unit instance at the cell** (visible during
setup), instead of placing a data-only token. `DeploymentTray` drives
`BattlePartySpawner.SpawnOrMoveAt/Despawn/DespawnAll`; the placed instances ARE the combat units
(spawned during PreBattle they stay idle — all combat controllers gate on `Phase == Active` — and
simply join combat on Assemble, **no deferred double-spawn**). The board is enlarged (`cellSize`
1.5→**3.5**) and split into a near **player zone** (rows 0–2), a neutral middle (3–5), and a far
**enemy zone** (rows 6–8, new `enemyRowMin/Max` + `InEnemyZone`). The deployment camera **auto-computes**
its framing from the grid (fits board width/depth for the current aspect), so it always frames the
board and follows cellSize/row changes.
**Why:** the player asked to *see* units as they deploy and for clearly separated sides; the token
model showed neither. Spawning during PreBattle gives a live preview for free.
**Implication:** units left on the bench aren't deployed (don't enter combat). The enlarged arena
needs the ground Plane enlarged (done) and the **NavMesh re-baked** for combat movement. The
no-deployment-phase fallback (immediate `SpawnParty`) is unchanged. Enemies are still scene-placed
(now repositioned into the enemy zone); a runtime enemy-zone placer is a follow-up if they become dynamic.

### ADR-012 — Equipment visuals via named sockets + UnitEquipmentVisuals; customization brought to front in code
**Decided:** equipped items render as child objects on **named sockets**. `Equipment_SO` gains
`attachSocketName` + optional `visualPrefab` (falls back to the existing `visualMesh`/`visualMaterials`).
New `UnitEquipmentVisuals` (Units layer, on the unit prefab) holds a serialized `name→Transform` socket
list and **diff-rebuilds** attached visuals on `UnitStatusController.OnStatsChanged`. Sockets are empties
**under the unit root** (not animated bones) for now. The customization screen + launcher **guarantee
foreground in code** (`EnsureForeground`: add a nested `Canvas` with `overrideSorting` + high
`sortingOrder` + `GraphicRaycaster` + `CanvasGroup` on open) rather than relying on scene wiring.
**Why:** the player wanted to *see* gear on the unit live and in combat, and the panel reliably on top
and clickable. Because `UnitFactory.ApplyEquipment` calls `Status.Equip` (→ `OnStatsChanged`), the same
component makes visuals appear automatically on deployment/combat units — no spawner changes.
**Implication:** root-relative sockets don't follow limb animation (re-parent under bones later if needed);
item meshes are placeholder cubes (`EquipVisual_Cube`) to swap for real art. Starter items live in the
`EquipmentCatalog` (the screen also unions a serialized `starterItems`); a persistent run-scoped inventory
is still a follow-up.

### ADR-013 — Branching, seeded, infinite run map (Slay-the-Spire style); runs end only on loss
**Decided:** the linear `RunMap`/`MapGenerator` is replaced by a **branching graph** generated one
**segment** at a time, seeded per run for reproducibility. `MapNode` gains Row/Column/Edges;
`MapGenerator.GenerateInitial`/`AppendSegment` walk bottom-to-top paths (adjacency-biased) + repair
reachability/outgoing edges + assign types by rules (bottom=Combat, top=single Boss gate, a Rest near
the top, no adjacent Rests) and weights (`MapGenConfig`). `RunState` tracks graph position
(`CurrentNodeId`), depth (`CurrentFloor`), `Seed`, `SegmentIndex`, and a `DifficultyMultiplier`;
`TravelTo`/`ReachableNodeIds` enforce edge-following choice. The player picks any bottom node to start,
then any node connected by an outgoing edge. Clearing the top Boss **stitches a new segment** on
(`AppendNextSegment`), so the climb is **infinite** — the run ends **only on loss** (the old
boss-win/recruit completion path is dormant; unlock points are awarded by depth on loss). The visual
map is `UI/Map/MapView` (+`MapNodeView`): a ScrollRect of colour-coded clickable nodes + edge lines,
data-driven off `RunState.Map`, reusing the existing encounter entry point (`RunController.EnterCurrentNode`).
**Why:** the requested player-driven branching progression; segment-at-a-time generation makes "infinite"
natural and keeps generation pure/testable (8 invariant tests). Persistence bumped to SaveVersion 2
(pre-v2 linear saves are discarded on load).
**Implication:** node/edge prefabs + the ScrollRect container are assembled/wired in `Test_M7_Map` per the
editor checklist (Claude can't see Play mode). Node visuals are procedural placeholders (coloured squares
+ a letter). A persistent run-scoped item inventory + a win/recruit path for infinite runs remain follow-ups.

### ADR-014 — Deployment click path: frame board above the HUD; enemy inspection via enemy-zone cell taps
**Decided:** the deployment placement bug (HUD bar covering the player-zone cells, so taps were dropped by
`DeploymentTray`'s `IsPointerOverGameObject` gate) is fixed by **moving the view, not making UI
click-through**: `DeploymentCameraController.bottomViewportInset` frames the board into the upper screen
above a clear bottom band for the HUD (+ a `framingOffset` nudge). The legacy `DeploymentView` (a second,
now-redundant click handler that no-ops under spawn-on-place) is **disabled**. Enemy stat inspection is
**cell-based, collider-free**: tapping an enemy-zone cell (`InEnemyZone`) finds the `Team.Enemy` `UnitRoot`
there and shows the shared `UnitInspectionPanel` (real `UnitStatusController` stats), anchored top-right so
it never overlaps the player zone — read-only, no placement effect.
**Why:** the user preferred a camera/UI move over click-through; cell-based enemy selection reuses the
existing ground raycast + `InEnemyZone` and respects ADR-008 (units have no colliders). Keeps a single
active click handler (`DeploymentTray`).
**Implication:** `bottomViewportInset`/`framingOffset` + the `EnemyInspectionPanel` RectTransform are
serialized/scene-tunable; verify in Play mode.

<!-- Add new decisions below as ADR-011, ADR-012, ... -->
