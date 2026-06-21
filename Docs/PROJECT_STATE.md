# Project State — CapsuleWars2

> **This is a SNAPSHOT, not a log.** Overwrite stale lines every handoff so it
> always describes the project *right now*. Keep it short enough to read in 30s.

_Last updated: 2026-06-21, after the Deployment v2 pass (spawn-on-place + split-zone board) — branch `claude/deployment-grid`_

## One-line status
Pre-combat **Deployment Phase v2**: placing a unit now **spawns the real unit at the cell** (visible
preview that carries into combat — no double-spawn), on a **larger board** (cellSize 3.5) split into a
near **player zone** (rows 0–2) and far **enemy zone** (rows 6–8); the deployment camera **auto-frames**
the board from the grid. **160/160 EditMode tests green.** Needs a Play-mode pass with a drafted run
(and a NavMesh re-bake for the enlarged arena).

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
- Unit inspection panel + between-rounds customization screen + launcher (built earlier; reachable).

## In progress
- Nothing mid-edit. The deployment feature is code-complete + scene-wired; only the Play-mode
  verification + arena tuning remain (below).

## Needs human verification (Claude can't see Play Mode)
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
- Earlier-built UI still pending Play QA: inspection click-to-show, customization equip→live stats→persist.
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
- Tests: `Assets/Scripts/Tests/EditMode/` (158 green).
- Branch: `claude/deployment-grid` (stacked off `claude/unit-factory`; none pushed).
