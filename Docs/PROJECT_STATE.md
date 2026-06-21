# Project State — CapsuleWars2

> **This is a SNAPSHOT, not a log.** Overwrite stale lines every handoff so it
> always describes the project *right now*. Keep it short enough to read in 30s.

_Last updated: 2026-06-21, after the Deployment placement fix + enemy inspection — branch `claude/deployment-grid`_

## One-line status
**Deployment placement fix (NEW):** the full-width bottom HUD bar covered the player-zone cells, so those
clicks were dropped as "over UI". Fixed by framing the board into the upper screen (new
`DeploymentCameraController.bottomViewportInset`, default 0.22) above a clear bottom band, and disabling
the redundant legacy `DeploymentView` click handler. Also added **click an enemy (its zone) to inspect its
stats** (shared `UnitInspectionPanel`, read-only, top-right, doesn't block placement). **162/162 EditMode
green.** Prior passes (Branching Map, Deployment v2, Customization v2) still await their Play-mode checks.

## What currently works
- Milestone base through ~M9 (draft → battle → recruit; combat, abilities, elements, synergies,
  status, stats; run/map; legacy + customization-unlock pipelines).
- Equipment + run-state + placement persistence (`UnitFactory` equipment, `RunStore`). EditMode-tested.
- Stat pipeline in `UnitStatusController` (+ `OnStatsChanged`). Tested.
- **Deployment phase (v2):** 7×9 grid (cellSize 3.5) with near player zone (rows 0–2) + far enemy
  zone (rows 6–8, `InEnemyZone`); `DeploymentTray` bench HUD; `DeploymentPhaseController` gate (combat
  blocked until Assemble). **Spawn-on-place:** `BattlePartySpawner.SpawnOrMoveAt/Despawn/DespawnAll`
  spawns the real unit at each placed cell (idle in PreBattle), and those instances become the combat
  units on Assemble. `DeploymentCameraController` auto-computes framing from the grid. Renderer +
  gizmo colour the enemy zone. All committed; gate/zone/grid logic unit-tested (160 green).
  **Placement fix:** the camera frames the board above a clear bottom band (`bottomViewportInset`) so the
  player-zone cells aren't hidden behind the HUD (clicks were being dropped as "over UI"); the legacy
  `DeploymentView` (redundant click handler) is disabled. **Enemy inspection:** tapping an enemy-zone cell
  shows that enemy's stats in a shared `UnitInspectionPanel` (top-right, read-only) — wired in `Test_M3_Battle`.
- **Branching run map (NEW, code-complete):** `Run/Map/` — `MapNode` (Row/Column/Edges), `RunMap` (graph
  helpers), `MapGenConfig` (rows/segment, nodesPerRow, pathCount, type weights, rules), `MapGenerator`
  (`GenerateInitial`/`AppendSegment`: seeded path-walk edges + reachability repair + rule/weight types +
  stitching). `RunState` is graph-based (`CurrentNodeId`, depth, `Seed`, `SegmentIndex`, `TravelTo`,
  `ReachableNodeIds`, `DifficultyMultiplier`, `AppendNextSegment`); `RunController.TravelToNode` + stitch
  on top-row clear + loss-only end. `UI/Map/MapView`+`MapNodeView` render it. Needs scene assembly (below).
- Unit inspection panel + between-rounds customization screen + launcher (reachable).
- **Customization (v2):** screen/launcher self-promote to the front + clickable (`EnsureForeground`:
  overriding high-sort Canvas + GraphicRaycaster + CanvasGroup added on open). Equip is a **toggle** with
  a selected highlight; the list = `EquipmentCatalog` ∪ serialized `starterItems` (4 starter items added
  to the catalog across slots). Equipped items render as meshes on **named sockets** via the new
  `UnitEquipmentVisuals` on `Unit_Sample_Prefab` (sockets RightHand/LeftHand/Helmet/Chest); visuals
  follow equipment live in the preview and on combat units (driven by `OnStatsChanged`).

## In progress
- Nothing mid-edit. The deployment feature is code-complete + scene-wired; only the Play-mode
  verification + arena tuning remain (below).

## Needs human verification (Claude can't see Play Mode)
- **Deployment placement fix + enemy inspection (`Test_M3_Battle`, `-force-d3d11`, drafted run):**
  1. **Placement:** the board frames above the bottom HUD; select a bench unit → click any **player-zone
     cell** ⇒ the unit places there. If the HUD still overlaps the near cells, raise `Main Camera` →
     `DeploymentCameraController.bottomViewportInset` (0.22→~0.3) or nudge `framingOffset`; the legacy
     `DeploymentView` on `Deployment` should be **disabled** (done). (Fallback if needed: set the HUD bar
     background Image `raycastTarget = false`.)
  2. **Enemy inspection:** click an **enemy** (its far-zone cell) ⇒ the top-right stats panel shows its
     name + HP/ATK/DEF/SPD (matching combat); Close (or click a player cell) hides it and **placement still
     works** — the panel is top-right and never covers the player zone. Tune the `EnemyInspectionPanel`
     RectTransform if its position/scale needs adjusting (instantiated via MCP).
  Built: under **Map Panel** → `MapScrollView` (ScrollRect, dark bg) → masked `Viewport` → `Content`
  (bottom-centre anchor/pivot); `MapView` added to Map Panel with `scrollRect`/`content`/`nodePrefab`
  (MapNode_View)/`edgePrefab` (Edge_Line) wired; node Label font = LiberationSans.
  - **Test:** start a run → a **branching map** renders; bottom-row nodes are clickable (rest dimmed) →
    click a start → its encounter runs via the existing battle/panel entry → back to the map at the new
    position → only edge-connected nodes clickable → climb to + clear the **Boss** (top row) → **a new
    segment generates + stitches on** and the map extends upward; repeat. **Lose** → run-end (Defeat).
    Restart the app mid-run → the same graph + position reloads.
  - **Double-check while testing:** (a) on `RunController` set `MapGenConfig`/`fixedSeed`/`difficultyPerDepth`
    if you want non-defaults; (b) the map fills/scrolls correctly (tune Content width or rowSpacing/colSpacing
    on MapView if cramped); (c) the **old Map Panel content** (pre-existing children) sits behind the new
    dark scroll view — delete it if it peeks through.
- **⚠ Re-bake the NavMesh first** (`Plane` → `NavMeshSurface` → Bake): the arena was enlarged
  (Plane scale 4, centre (10.5,0,14)), so combat movement needs a fresh bake covering the new board.
- **Deployment loop (spawn-on-place):** load a combat node *with a drafted party* → during deployment,
  tap a bench unit then a green player-zone cell → the **real unit appears at that cell** (0.3s scale-in);
  tap a placed cell to bench it (the instance is destroyed); **Clear** removes all. **Assemble** → those
  same units start combat (no duplicates); combat does NOT start before Assemble.
- **Zones / camera:** player units sit in the near zone, the enemy on the far side (rows 6–8) — clearly
  separated; the deployment view auto-frames the whole board (near-top-down), pinch/scroll to zoom in.
  Launch the editor with `-force-d3d11` to dodge the Adreno/D3D12 GPU crash.
- *Known minor:* the downed-HP carry-forward (applied at BattleStateManager.Start) won't apply to
  units spawned during deployment (they spawn after Start) — verify if it matters for your flow.
- **Customization v2 (map scene, `-force-d3d11`):**
  1. Open Customize → party picker is on top + clickable; pick a unit → the screen is in front and every
     equip button responds (selecting one does NOT block further clicks).
  2. The equip list is non-empty immediately (Starter Sword/Shield/Helm/Plate across slots).
  3. Click an item → its **cube appears** at the matching socket on the preview unit + the button
     highlights green; click again → it's removed. Try several slots at once.
  4. Close (saves) → start a battle with that unit → the item cube shows on the **deployed/combat** unit too.
  5. *Polish:* `EquipVisual_Cube` is a placeholder — swap each item's `visualPrefab`/`visualMesh` for real
     art; the root-relative sockets can be re-parented under hand/head bones for animated attachment.
- Inspection panel click-to-show still pending Play QA.
- Battle-end UI shows placeholder "New Text" labels (pre-existing) — cosmetic cleanup.

## Known issues / blockers
- CoplayDev MCP bridge drops on every domain reload (recompile / entering Play) and cycles ports — reconnect.
- Game-view MCP screenshots are blank; use `manage_camera capture_source=scene_view` or computer-use on the Game/Simulator window.
- MCP object/component ref fields must use object form (`{"instanceID":N}` / `{"path":"..."}`); bare ints fail.
- Deployment uses the full `Unit_Sample_Prefab` for spawned units (NavMeshAgent etc.) — fine in battle; a
  stripped preview prefab is still a backlog item for the customization screen.

## Key paths
- Deployment model + phase: `Assets/Scripts/Combat/Deployment/` (GridCoord, CellState, DeploymentGridConfig,
  DeploymentGrid, DeploymentManager, DeploymentPhaseController, DeploymentGizmos).
- Deployment UI: `Assets/Scripts/UI/Deployment/` (DeploymentTray, DeploymentGridRenderer, DeploymentView) +
  `Assets/Scripts/UI/Camera/DeploymentCameraController.cs`.
- Spawn/persist: `Assets/Scripts/Run/BattlePartySpawner.cs`, `RunState.Placements`, `Assets/Scripts/Persistence/`.
- Stats (NO StatCalculator — see ADR-007): `Assets/Scripts/Units/Controllers/UnitStatusController.cs`.
- UnitFactory: `Assets/Scripts/Persistence/UnitFactory.cs`.
- Scenes: `Assets/Scenes/Test_M3_Battle.unity` (combat + deployment), `Test_M7_Map.unity` (map + between-rounds).
- Customization: `Assets/Scripts/UI/Customization/` (CustomizationScreen, CustomizationLauncher) +
  `Assets/Scripts/Units/Customization/` (UnitCustomization, UnitEquipmentVisuals) + `Assets/Data/Equipment/`
  (Equipment_SO, EquipmentCatalog.asset, Equip_Starter*.asset).
- Run map: `Assets/Scripts/Run/Map/` (MapNode, RunMap, MapGenConfig, MapGenerator, BattleNodeReturn) +
  `RunController`/`RunState`/`RunSession`; UI: `Assets/Scripts/UI/Map/` (MapView, MapNodeView) +
  `Assets/Prefabs/Map/` (MapNode_View, Edge_Line). DTOs: `Assets/Scripts/Persistence/Dto/RunStateDTO.cs` (v2).
- Tests: `Assets/Scripts/Tests/EditMode/` (162 green).
- Branch: `claude/deployment-grid` (stacked off `claude/unit-factory`; none pushed).
