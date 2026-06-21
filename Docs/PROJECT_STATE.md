# Project State — CapsuleWars2

> **This is a SNAPSHOT, not a log.** Overwrite stale lines every handoff so it
> always describes the project *right now*. Keep it short enough to read in 30s.

_Last updated: 2026-06-21, after the Deployment Phase feature — branch `claude/deployment-grid`_

## One-line status
A full **pre-combat Deployment Phase** is implemented and wired into the battle scene
(7×9 grid, visible bench HUD, place units, confirm-to-start, camera framing, place-then-spawn).
**158/158 EditMode tests green.** The end-to-end loop needs a Play-mode pass with a drafted run.

## What currently works
- Milestone base through ~M9 (draft → battle → recruit; combat, abilities, elements, synergies,
  status, stats; run/map; legacy + customization-unlock pipelines).
- Equipment + run-state + placement persistence (`UnitFactory` equipment, `RunStore`). EditMode-tested.
- Stat pipeline in `UnitStatusController` (+ `OnStatsChanged`). Tested.
- **Deployment phase (NEW):** 7×9 grid model + `DeploymentManager` (placement, tokens, AutoArrange);
  `DeploymentPhaseController` gate (combat blocked until Assemble); `DeploymentTray` bench HUD;
  `DeploymentGizmos` Scene-view grid; `DeploymentCameraController` auto-frame; `BattlePartySpawner`
  place-then-spawn. All committed; HUD renders; gate/token/grid logic unit-tested.
- Unit inspection panel + between-rounds customization screen + launcher (built earlier; reachable).

## In progress
- Nothing mid-edit. The deployment feature is code-complete + scene-wired; only the Play-mode
  verification + arena tuning remain (below).

## Needs human verification (Claude can't see Play Mode)
- **Deployment loop:** load a combat node *with a drafted party* (RunSession.Party non-empty) →
  the bottom HUD bench lists units → tap a unit then a green cell to place (only the player zone;
  tap a placed unit to bench it) → **Assemble** → units spawn at the placed cells → combat starts.
  Confirm **Clear** empties the board and combat does NOT start before Assemble.
- **Camera:** tune `DeploymentCameraController` deploymentPosition/euler/FOV so it frames the whole
  7×9 board; confirm the transition in/out feels right.
- **Grid placement:** tune `DeploymentGridConfig` origin/cellSize on the `Deployment` object (and
  the matching copy on `BattlePartySpawner`) so the board sits on the arena; verify the gizmo in the Scene view.
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
