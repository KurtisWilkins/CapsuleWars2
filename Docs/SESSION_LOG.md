# Session Log — CapsuleWars2

> **Append-only.** Newest entry at the TOP. Read only the last 1–2 entries when
> resuming — do not load the whole file into context.
> Copy the template at the bottom for each new entry.

<!-- NEW ENTRIES GO HERE (top = newest) -->

## 2026-06-21 — GPU crash fixed: force Direct3D11 on Windows
**Goal this session:** the editor kept crashing in Play with "d3d12: Unrecoverable GPU device error!" —
find the cause and fix it.

**Done (committed on `claude/deployment-grid`):**
- **Diagnosis (from crash logs):** `%LOCALAPPDATA%\Temp\Unity\Editor\Crashes\` had 5 dumps today; the
  current Editor.log shows the GPU is a **Qualcomm Adreno X1-85 (Snapdragon X ARM64), driver 31.0.133.1**,
  running **Direct3D 12**. The crash modules are the Qualcomm D3D12 driver DLLs + `D3D12Core.dll` → an
  unrecoverable D3D12 device-removed. It even crashed on the *draft screen* (no game code) → environmental,
  not our code.
- **Fix:** Project Settings → Player → Other Settings → **unchecked Auto Graphics API for Windows** and
  **removed Direct3D12** (Direct3D11 only). Restarted the Editor → title bar now `<DX11>`. Re-ran the
  playthrough: Play no longer crashes at the point it previously died. Committed `ProjectSettings.asset`.
  `-force-d3d11` is no longer needed.

**Verified via computer-use Play mode:** editor on DX11, Play runs without the crash.

**Next session starts with:** the (now-unblocked) deployment placement playthrough — draft a party → battle →
drop a unit on a player cell; plus the branching-map play-test.

## 2026-06-21 — deployment placement fix + enemy stat inspection
**Goal this session:** placing a unit on a cell didn't work (HUD covered the player-zone cells); also add
clicking an enemy to see its stats.

**Done (committed on `claude/deployment-grid`; 162/162 EditMode green):**
- **Root cause:** `DeploymentTray.Update` drops taps where `EventSystem.IsPointerOverGameObject()` is true;
  the `DeploymentHUD` is a full-width 230px bottom bar and the camera framed the whole board near-top-down,
  so the player zone (near rows) sat at the bottom under the HUD → taps dropped.
- **FIX 1 (camera/UI move):** `DeploymentCameraController.bottomViewportInset` (default 0.22) frames the
  board into the upper screen above a clear bottom band; added `framingOffset` nudge. Disabled the legacy
  `DeploymentView` (redundant 2nd click handler, no-ops under spawn-on-place).
- **FIX 2 (enemy inspection):** `DeploymentTray` — tapping an enemy-zone cell (`InEnemyZone`) finds the
  `Team.Enemy` `UnitRoot` there and shows the shared `UnitInspectionPanel` (real `UnitStatusController`
  stats), read-only, returns without placing. Instantiated `UnitInspectionPanel.prefab` as
  `EnemyInspectionPanel` (top-right, clear of the player zone) in `Test_M3_Battle` + wired the ref.

**Compiled clean:** yes. **EditMode 162/162.**

**Needs human verification (Play Mode, `-force-d3d11`):** see PROJECT_STATE — place a unit on a player cell;
click an enemy to see stats without blocking placement; tune `bottomViewportInset`/`framingOffset` + panel
RectTransform if needed.

**Decisions:** ADR-014 (frame board above HUD; collider-free enemy-zone-cell inspection; DeploymentView retired).

**Next session starts with:** play-test the deployment placement fix + enemy inspection (TASKS top item).

## 2026-06-21 — branching run map (Slay-the-Spire: seeded, infinite, choice-based)
**Goal this session:** replace the linear "advance" route with a visual branching node map — pick a
start, follow edges upward, infinite segments stitch on, run ends only on loss.

**Done (committed on `claude/deployment-grid`; 162/162 EditMode green):**
- **Model/gen** (`Run/Map/`): `MapNode` +Row/Column/Edges; `RunMap` graph helpers; new `MapGenConfig`
  (rows/segment, nodesPerRow, pathCount, type weights, rules); `MapGenerator.GenerateInitial`/`AppendSegment`
  — seeded bottom-to-top path-walk edges + reachability/outgoing repair + rule/weight types + segment
  stitching. 8 invariant tests (reachability, outgoing, bottom=Combat, top=Boss, no adjacent Rests, range,
  determinism, stitch). Legacy linear `Generate` kept only to ease the migration.
- **Run state/flow:** `RunState` graph-based (`CurrentNodeId`/-1, depth=`CurrentFloor`, `Seed`,
  `SegmentIndex`, `TravelTo`/`ReachableNodeIds`/`MarkCurrentCleared`/`AppendNextSegment`/`DifficultyMultiplier`);
  DTOs v2 (edges/row/col + seed + node id; pre-v2 saves discarded). `RunController` builds a seeded first
  segment, `TravelToNode` for the map UI, stitches on top-row clear, ends only on loss (points by depth).
  `BattleNodeReturn` marks cleared (no auto-advance); `RunBattleSetup` scales enemies by depth; `RunHud`
  shows depth/segment; `RunEndPanel` shows depth. Rewrote `RunStateTests`/`RunStatePersistenceTests`.
- **UI:** `UI/Map/MapView` (ScrollRect of colour/state-coded nodes + edge lines, click→TravelToNode,
  auto-scroll, foreground) + `MapNodeView`; procedural node/edge prefabs (`Assets/Prefabs/Map/`).

**Compiled clean:** yes. **EditMode 162/162.**

**Mid-session blocker:** Unity Editor closed/crashed during planning; I kept edits additive + paused the
coupled migration until it was relaunched, then did it with test feedback.

**Needs human verification (Play Mode):** scene assembly + test — see PROJECT_STATE (add a Scroll View to
the map panel, wire `MapView` + the 2 prefabs, assign a Label font, set `RunController` MapGenConfig/seed,
then run a branching climb). Launch `-force-d3d11`.

**Decisions:** ADR-013 (branching/infinite seeded map; graph run-state; loss-only end; MapView UI).

**Next session starts with:** assemble + play-test the branching map (TASKS top item).

## 2026-06-21 — customization v2 (front/clickable + starter items + item meshes on sockets)
**Goal this session:** fix three customization-screen gaps — (1) not in front / hard to click,
(2) no items to test with, (3) equipping changes stats but shows nothing on the unit.

**Done (all committed on `claude/deployment-grid`; 160/160 EditMode green):**
- **Front + clickable (CHANGE 1, code):** `CustomizationScreen.EnsureForeground` + `CustomizationLauncher.EnsureForeground`
  add an overriding high-sort `Canvas` (100 / 90) + `GraphicRaycaster` + `CanvasGroup` to the panel/picker on
  open — guarantees foreground + raycasts above other map UI regardless of scene wiring (both map canvases were
  Screen Space–Overlay at sort 0). Equip became a **toggle** with a green selected highlight.
- **Starter items (CHANGE 2):** 4 `Equipment_SO` (sword/shield/helm/plate across slots) added to
  `EquipmentCatalog.asset` (now 6) + a serialized `starterItems` union on the screen.
- **Item meshes (CHANGE 3):** `Equipment_SO` gained `attachSocketName` + `visualPrefab`; new
  `UnitEquipmentVisuals` (on `Unit_Sample_Prefab`) holds named sockets (RightHand/LeftHand/Helmet/Chest as
  root empties) and diff-rebuilds attached meshes on `OnStatsChanged` — live in preview AND on combat units
  (UnitFactory.Equip fires the event). Placeholder `EquipVisual_Cube` prefab as the visual.

**Compiled clean:** yes. **EditMode 160/160.**

**Needs human verification (Play Mode, `-force-d3d11`):** see PROJECT_STATE — open Customize → in front +
clickable → 4 items listed → click ⇒ cube on socket + highlight → Close → combat unit shows the cube. Swap
the placeholder cube for real meshes.

**Decisions:** ADR-012 (item visuals via named sockets + UnitEquipmentVisuals; customization foreground in code).

**Next session starts with:** re-bake NavMesh (deployment v2) + Play-mode test both v2 passes (TASKS top items).

## 2026-06-21 — deployment v2 (spawn-on-place + split-zone board + grid-fit camera)
**Goal this session:** fix two deployment problems — (1) you couldn't *see* units as you placed them
(placement was data-only), and (2) the board was tiny with no enemy zone (sides overlapped).

**Done (all committed on `claude/deployment-grid`; 160/160 EditMode green):**
- Camera framing fix (prior turn): deployment pose was pitched forward past the deploy zone; re-aimed
  near-top-down. Then generalised into auto-framing (below).
- **Spawn-on-place:** `BattlePartySpawner` gained `SpawnOrMoveAt/Despawn/DespawnAll` (instance dict +
  `DeployedUnits` container, cached DB); `DeploymentTray` calls them on place/bench/clear. Dropped the
  deferred OnConfirmed spawn — placed instances (idle in PreBattle) become the combat units on Assemble,
  no double-spawn. No-deployment fallback unchanged.
- **Bigger split-zone board:** `DeploymentGridConfig` cellSize 1.5→3.5; added `enemyRowMin/Max` (6–8) +
  `InEnemyZone`. Renderer + gizmo colour the enemy zone. Scene: cellSize 3.5 on both grid configs, enemy
  moved to (10.5,0,24.5), Plane enlarged (scale 4 @ (10.5,0,14)), wider camera bounds/zoom.
- **Camera auto-frame:** `DeploymentCameraController` computes the framing pose from the grid (fits
  board width/depth for the aspect, near-top-down tilt); falls back to the manual pose if no grid.
- Tests: zone-disjoint + cellSize tests; fixed the default-cellSize assertion in DeploymentManagerTests.

**Compiled clean:** yes. **EditMode 160/160.**

**Needs human verification (Play Mode):** see PROJECT_STATE. **Re-bake the NavMesh** (Plane enlarged)
first; then with a drafted run, place units (they should appear at the cell), Assemble → those units
fight; enemy on the far side; camera auto-frames. Launch with `-force-d3d11`.

**Decisions:** ADR-011 (deployment spawn-on-place + split-zone board + grid-fit camera; supersedes ADR-010 tokens).

**Next session starts with:** re-bake the NavMesh, then Play-mode test deployment v2 (TASKS top items).

## 2026-06-21 — deployment phase (7×9 grid, gate, tray HUD, camera)
**Goal this session:** add a Deployment Phase before combat — a 7×9 grid with a visible tray HUD
to place units, camera pulled back to frame the board, confirm-to-start.

**Done (all committed on `claude/deployment-grid`; 158/158 EditMode green):**
- Grid → 7×9 (columns = X/width, rows = Z/depth); updated the scene's DeploymentManager +
  BattlePartySpawner configs. New `DeploymentGizmos` draws the grid + player zone in the Scene view.
- `DeploymentPhaseController` gates `BattleStateManager.StartBattle` (DeploymentRequired/Confirmed);
  `Confirm()` (Assemble) spawns + starts. `BattlePartySpawner` defers spawn to `OnConfirmed`
  (place-then-spawn); falls back to immediate spawn when no deployment phase. Late-spawned units
  still register via `UnitRoot.OnEnable → registry.OnUnitRegistered` (verified).
- `DeploymentManager` token placement (`PlaceToken/RemoveToken/ClearAll`). `DeploymentTray` HUD:
  bench from `RunSession.Party`, tap-select → tap-cell to place, tap a placed cell to bench it,
  Assemble/Clear; mirrors `RunState.Placements`; refreshes the grid renderer for valid/invalid colours.
- `DeploymentCameraController` auto-frames the board on deploy, restores the battle pose on Assemble.
- Wired in `Test_M3_Battle`: `DeploymentPhase` + `DeploymentHUD` (bottom bench bar + Assemble/Clear +
  selection label) + the gizmo component. HUD bar + buttons render (confirmed in the Simulator).

**Compiled clean:** yes. **EditMode 158/158** (+gate, +token, +7×9 fix tests).

**Needs human verification (Play Mode):** see PROJECT_STATE. Core loop: load a combat node with a
drafted party → HUD bench populates → place units → Assemble → units spawn at placed cells → combat
runs. Camera deployment pose + grid origin/zone need tuning to the arena.

**Decisions:** ADR-010 (deployment phase: place-then-spawn + confirm gate + 7×9 + camera auto-frame).

**Next session starts with:** Play-mode test of the full deployment loop with a drafted run (TASKS top item).

## 2026-06-21 — continuity setup + battle/customization UI
**Goal this session:** ship the battle/customization UI feature set, then stand up a
cross-session continuity system (this doc system).

**Done:**
- Persistence foundation — `UnitDTO.Equipment` + `IEquipmentDatabase`/`EquipmentDatabase`/
  `EquipmentCatalog_SO`; `UnitFactory` applies/captures equipment; `RunStateDTO`+`MapNodeDTO`+
  `UnitPlacementDTO`, `RunStore` (run.json), `RunState.ToDTO/FromDTO`, `RunSession` save/load.
- `UnitStatusController.OnStatsChanged` event (fires on equip/unequip/synergy).
- `DeploymentCameraController` (UI/Camera) — pan/zoom/clamp, PreBattle-gated; on battle Main Camera.
- `UnitInspectionPanel` (UI/Inspection) built in Test_M3_Battle, extracted to
  `Assets/Prefabs/UnitInspectionPanel.prefab`.
- Deployment grid: `Combat/Deployment/` model (GridCoord/Config/Grid/CellState) +
  `DeploymentManager` (AutoArrange, cell-based) + `UI/Deployment/` view + `DeploymentGridRenderer`
  + `DeploymentCell.prefab`; `BattlePartySpawner` spawn-then-arrange.
- `CustomizationScreen` (UI/Customization) built in Test_M7_Map + `EquipButton.prefab` +
  `EquipmentCatalog.asset`.
- Fixed the pre-existing EditMode baseline regression (UnitHealthController lazy init;
  BattleStatsAggregator id fallback).

**Compiled clean:** yes. **EditMode tests: 155/155 green.**

**Needs human verification in editor (Play Mode):**
- Deployment: click unit cell → inspection shows; click empty cell → unit moves.
- Customization screen: not yet triggered (no `Show` caller) and needs a run session to
  populate; verify equip → live stats → persist once wired.
- Camera feel/bounds; grid config alignment to arena; battle-end "New Text" placeholders.

**Decisions:** ADR-003…009 added (uGUI, mobile+desktop, armor=stats, run-scoped persistence,
no StatCalculator, spawn-then-arrange/cell-based deployment, stacked feature branches).

**Next session starts with:** wire the between-rounds trigger that calls
`CustomizationScreen.Show(unitId)` (TASKS.md top item).

---
---

<!-- ============ ENTRY TEMPLATE — copy this block ============ -->
<!--
## YYYY-MM-DD — session <n>
**Goal this session:** (one line)

**Done:**
- (file touched) — (what changed, why)

**Compiled clean:** yes / no — (notes if no)

**Needs human verification in editor:**
- (visual/gameplay things Claude couldn't confirm)

**Decisions:** (link ADR id if added) / none

**Next session starts with:** (the exact next step — must match TASKS.md top item)
-->
