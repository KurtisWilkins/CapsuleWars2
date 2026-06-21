# Project State — CapsuleWars2

> **This is a SNAPSHOT, not a log.** Overwrite stale lines every handoff so it
> always describes the project *right now*. Keep it short enough to read in 30s.

_Last updated: 2026-06-21, continuity-setup session — branch `claude/deployment-grid`_

## One-line status
The battle + customization UI feature set (deployment grid, deploy camera, unit
inspection, between-rounds customization) is **code-complete and committed**, with
the UI screens built in-scene; **155/155 EditMode tests green**. The end-to-end
gameplay flows still need a Play-mode pass.

## What currently works
- **Milestone base through ~M9** (see `Docs/00`–`Docs/17`): draft → battle → win →
  recruit → roster cap; combat (BattleStateManager, abilities, elements, class
  synergies, status effects, stats aggregation); roguelike run/map; legacy persistence;
  customization unlocks + parts/palette pipeline.
- **Equipment + run-state + placement persistence** — `UnitFactory` applies/captures
  equipment; `RunStore` saves `run.json`; deployment placements persist battle-to-battle.
  EditMode round-trip tested.
- **Stat pipeline** — `UnitStatusController` computes final stats (base + equipment×rarity
  + status + synergy) and raises `OnStatsChanged`. Tested.
- **Deployment grid** — `Combat/Deployment` model + `DeploymentManager` (cell-based
  placement, `AutoArrange`) tested; in Play the green tiles render and units auto-arrange
  into the deploy zone (visually confirmed).
- **Unit inspection panel** — built in `Test_M3_Battle`, reusable prefab; renders all
  fields and correctly hides on Play (confirmed on screen).
- **Deployment camera** — pan/zoom/clamp, gated to PreBattle; compiles, wired to Main Camera.

## In progress
- **Customization screen** — built in `Test_M7_Map` (left-edge panel + inspection-prefab
  instance + EquipButton prefab + PreviewAnchor; all refs wired). Not yet reachable in-game
  (no caller for `Show(unitId)`) and not play-tested.

## Needs human verification (Claude can't see Play Mode)
- Deployment interaction: tap a unit's cell → inspection panel shows live stats; tap an
  empty deploy cell → the selected unit moves there.
- Customization screen end-to-end: needs a wired trigger + a run session to populate;
  verify equip → live stats update → persists across restart.
- Deployment camera feel/bounds tuning to the arena.
- `DeploymentGridConfig` origin/cellSize/zone alignment to the arena (keep
  `BattlePartySpawner.deploymentGrid` in sync).
- Battle-end UI shows placeholder "New Text" labels (pre-existing) — cosmetic cleanup.

## Known issues / blockers
- CoplayDev MCP bridge **drops on every domain reload** (recompile / entering Play) and
  cycles ports (6400→6401→…). Just reconnect (`refresh_unity` / `manage_editor`).
- **Game-view MCP screenshots return blank.** Use `manage_camera capture_source=scene_view`
  for 3D/world, or computer-use on the Unity Game/Simulator window for overlay UI.
- Setting object/component reference fields via MCP must use object form
  (`{"instanceID": N}` / `{"path": "Assets/..."}`); a bare integer silently fails to bind.

## Key paths
- ScriptableObject data layer (code): `Assets/Scripts/Data/` (Units, Equipment, Elements,
  Classes, StatusEffects, Weapons). Authored assets: `Assets/Data/`.
- UnitFactory: `Assets/Scripts/Persistence/UnitFactory.cs`
- Stat computation (NO `StatCalculator` class — see ADR-007):
  `Assets/Scripts/Units/Controllers/UnitStatusController.cs`
- Deployment — model: `Assets/Scripts/Combat/Deployment/`; view: `Assets/Scripts/UI/Deployment/`
- Inspection: `Assets/Scripts/UI/Inspection/UnitInspectionPanel.cs` +
  `Assets/Prefabs/UnitInspectionPanel.prefab`
- Customization: `Assets/Scripts/UI/Customization/CustomizationScreen.cs`
- Persistence: `Assets/Scripts/Persistence/` (`RunStore`, `LegacyStore`, `Dto/`)
- Scenes: `Assets/Scenes/Test_M3_Battle.unity` (combat + deployment),
  `Test_M7_Map.unity` (map + between-rounds), `Test_M3_Idle`, `Test_M1_Idle`
- Tests: `Assets/Scripts/Tests/EditMode/` (155 green); PlayMode asmdef exists.
- Branch: `claude/deployment-grid` (stacked: equipment-persistence → deploy-camera →
  unit-inspection → deployment-grid; none pushed; `claude/unit-factory` not yet on `main`).
