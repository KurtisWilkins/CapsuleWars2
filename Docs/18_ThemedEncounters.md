# 18 — Themed, obstacle-bearing encounters (3-slice design)

> Design note for the themed-encounter system: a per-cell terrain layer, a visual biome skin, and a
> runtime encounter builder that places enemies around the terrain. **Only Slice A is built** (ADR-024);
> B and C are specified here and tracked in `TASKS.md`. This doc defines the data contracts now so the
> later slices drop in without reshaping the foundation.

## The system, end to end
A map node (Combat/Elite/Boss) resolves to an **encounter**: a themed board with obstacles + hazards, an
enemy roster, and an enemy placement that accounts for the terrain. Three slices, layered so each is useful
on its own:

| Slice | Owns | Layer | Status |
|------|------|-------|--------|
| **A. Terrain/obstacle data** | per-cell `TerrainType`, placement validation, authored layout | `Combat.Deployment` (pure model) | **built (ADR-024)** |
| **B. Scenery / biome theming** | runtime block arena: checkerboard floor + obstacle blocks, themed, runtime NavMesh bake | `UI.Arena` + `Data.Arena` | **built (ADR-025)** |
| **C. Encounter builder** | `EncounterDefinition`, enemy roster generation, obstacle-aware placement ("the strategy") | Run / Combat | **C1 terrain + C2 roster + C3 placement built (ADR-026/027)**; visual variety + smarter strategy iterate next |

The hard rule that makes this composable: **the terrain layer (A) knows nothing about visuals (B) or
enemies (C)**. B and C *read* A. A never references UI or Run (assembly layering Core/Combat → … → UI/Run).

## Slice A — terrain model (built)
`Combat.Deployment`, no scene deps, EditMode-tested. See ADR-024 for the decisions.

- **`TerrainType`** — `Passable` (normal), `Impassable` (blocks placement **and** pathing — rock/river/wall),
  `Hazard` (placeable but harmful/avoid — lava/trap). Generalizes the old binary `blocked` flag
  (`blocked == Impassable`).
- **`DeploymentGrid`** — sparse `Dictionary<GridCoord,TerrainType>` (absent = Passable) +
  `SetTerrain/GetTerrain/IsImpassable/IsHazard`, read-only `TerrainCells` (non-Passable only).
  `SetBlocked/IsBlocked` remain as Impassable wrappers (compat).
  - `IsDeployable(c) = InPlayerZone(c) && !IsImpassable(c) && (config.allowPlaceOnHazard || !IsHazard(c))`.
  - `GetState`: `OutOfBounds → Blocked(Impassable) → Occupied → Hazard → OutsideZone → Empty`. Blocked and
    Hazard read anywhere (not just in-zone), so the board can show obstacles + hazards on both sides.
- **`DeploymentGridConfig.allowPlaceOnHazard`** (default `true`) — may the player deploy on a Hazard?
- **`TerrainLayout`** — serializable `List<TerrainCell{coord,type}>` + `ApplyTo(grid)`. Authored inline on
  `DeploymentManager` (`Terrain` getter), stamped onto the grid in `DeploymentManager.Awake` before the
  deployment UI builds. Slice C generates one of these per encounter.
- **Renderer** — `DeploymentGridRenderer` got a minimal `hazardColor` case (`CellState.Hazard`). Blocked
  already rendered. No props/theming.

### Data contracts the later slices consume (defined now)
- **Read terrain:** `DeploymentGrid.GetTerrain(c)`, `IsImpassable(c)`, `IsHazard(c)`, and
  `IReadOnlyDictionary<GridCoord,TerrainType> TerrainCells` (obstacles + hazards only).
- **Authored/generated layout:** `TerrainLayout` (serializable) → `ApplyTo(DeploymentGrid)`. Exposed via
  `DeploymentManager.Terrain`.
- **World position of a cell:** `DeploymentGridConfig.CellToWorld(coord)` + `cellSize` (for prop placement
  and NavMesh-obstacle boxes).
- **Zones:** `config.InPlayerZone(c)` / `config.InEnemyZone(c)` (enemy placement domain for Slice C).

## Slice B — runtime modular-block arena (BUILT, ADR-025)
A **visual skin** over the terrain data; changes nothing in the model. Realized as a runtime block builder rather
than just prop-scatter, so the floor itself reads as the deployment grid.

- **`ThemeBlockSet` SO** (`Data.Arena`): role {FloorA, FloorB, Obstacle, HazardMarker} → `{prefab, material, height}`.
  Null prefab → scaled **primitive-cube fallback** (works with zero assets; Kubikos/Meshy drop in by assigning
  prefabs — no code change). **`EncounterTheme` SO** selects a block set per floor (grass + volcanic placeholders
  authored under `Assets/Settings/Arena/`).
- **`ArenaBuilder`** (`UI.Arena`, scene component on `Test_M3_Battle`): `Build()` reads `DeploymentManager.Config`
  + `.Terrain` and instantiates, under one `ArenaRoot`, a checkerboard floor tile per cell (`(col+row)%2`) + a
  raised obstacle on each Impassable cell + a marker on each Hazard cell, sized from `cellSize`. `Teardown()`
  destroys the root (no leaks). Pure math in testable `ArenaLayout`.
- **NavMesh = runtime re-bake** (not carving — see ADR-025): `Bake()` sets `NavMeshSurface.useGeometry =
  PhysicsColliders` and calls `BuildNavMesh()` after the geometry exists; obstacles carry `NavMeshModifier(Not
  Walkable)` + a BoxCollider; collider-less units (ADR-008) are never baked in; the Plane MeshCollider is the
  walkable ground. Extends the scene's existing `NavMeshSurface`.
- **Checkerboard ↔ overlay:** the block floor is the BASE; `DeploymentGridRenderer`'s CellState tints are a
  translucent OVERLAY on the same cells (shared grid). Floor lifted `floorSurfaceY≈0.05` so it reads over the
  legacy Plane (which the builder hides) with no z-fighting.
- **Editor preview:** `Tools/Arena/*` menu items + component ContextMenus build/bake/clear without Play.
- **Contract used:** `DeploymentManager.Config` + `.Terrain` (the `TerrainLayout`), `config.CellToWorld`/`cellSize`,
  `grid.TerrainCells`. Read-only; B never writes the model.

## Slice C — encounter builder (iterative; C1 built)
Turns a map node into a concrete fight: roster + obstacle layout + obstacle-aware placement.

**C1 — seeded terrain generation (BUILT, ADR-026):** `EncounterGenerator` (pure, `Run.Encounters`) +
`EncounterDefinition` SO + `EncounterBuilder` (battle-scene, `[DefaultExecutionOrder(-100)]`). On Awake it reads
the run (`Seed ^ CurrentNodeId`, `CurrentFloor`), generates a `TerrainLayout` of Impassable+Hazard cells (player
deploy zone kept clear, obstacles scale with floor), and applies it via `DeploymentManager.SetTerrain` before
ArenaBuilder renders + NavMesh-bakes it. Deterministic → no save needed. Wired into `Test_M3_Battle`; editor
preview via `Tools/Arena/Preview Generated Encounter`.

**C2+C3 — generated obstacle-aware enemies (BUILT, ADR-027):** `EncounterGenerator.RosterSize` (count by
boss/floor) + `EncounterGenerator.EnemyCells` (Passable enemy-zone cells avoiding Impassable) + an
`EnemyEncounterSpawner` (`[DefaultExecutionOrder(-75)]`, run-gated, retires the scene enemy, spawns
`Unit_Enemy.prefab` clones via `UnitFactory.Spawn` — mirrors `BattlePartySpawner`). v1 = base clones (visual
variety later). **Next iterations:** enemy visual generation (needs a non-unlock-gated parts pool), smarter
placement strategies, real per-unit stats/class. Watch NavMesh-bake-vs-spawn ordering in Play (ADR-027).

- **`EncounterDefinition` SO** (or seeded generator config): enemy roster spec (count/tiers/archetypes by
  `NodeType` + depth), an obstacle **`TerrainLayout`** (hand-authored or a generator config), and a
  **placement strategy** (e.g. spread / cluster / behind-cover).
- **Builder**: stamp the `TerrainLayout` onto the grid (`ApplyTo`), generate the roster (extends
  `RandomUnitGenerator` — which today does **visual identity only**; stats/class/abilities are still TODO and
  are **out of scope** until then), then place each enemy on a `Passable` cell within `config.InEnemyZone`
  avoiding `IsImpassable` (and `IsHazard` per strategy). Will want an enemy-zone analogue of
  `DeploymentManager.FirstFreeCell` (which scans the player zone today).
- **`EnemyEncounterSpawner`** — new, a mirror of `BattlePartySpawner` (which is player-only). **Today enemies
  are scene-placed static** (`Unit_Enemy` in `Test_M3_Battle`); `RunBattleSetup` only *boosts* `Team.Enemy`
  stats per depth/boss. So this is a greenfield seam — nothing to refactor, just add the run-driven spawn.
- **Persistence/determinism**: generate the layout + roster deterministically from `RunState.Seed + nodeId`
  → reproducible, so nothing extra needs saving. Only serialize a `TerrainLayout` into `RunState` if a later
  design makes an encounter non-deterministic (e.g. player-modified terrain mid-fight).
- **Attach point**: `MapNode.Type` (Combat/Elite/Boss) + the combat-scene setup (`RunController` /
  `RunBattleSetup`) is where the builder is invoked.

## Scope guardrails (this session built only A)
- No biome/scenery visuals, materials, props, skybox (Slice B).
- No `EncounterDefinition`, no enemy roster generation, no placement AI (Slice C).
- No new enemy stat/class/ability generation — `RandomUnitGenerator` stays as-is.
- B and C are captured in `TASKS.md` with the contracts above.
