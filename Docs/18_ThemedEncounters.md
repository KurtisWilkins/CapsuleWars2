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
| **B. Scenery / biome theming** | `TerrainType → prop prefab/material`, NavMesh carve, skybox/biome skin | UI / scene | later |
| **C. Encounter builder** | `EncounterDefinition`, enemy roster generation, obstacle-aware placement ("the strategy") | Run / Combat | later, iterative |

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

## Slice B — scenery / biome theming (later)
A **visual skin** over the terrain data; changes nothing in the model.

- **`BiomeTheme` SO**: `TerrainType → { prop prefab, material/decal }` + ground material + skybox/lighting.
- **`TerrainView` scene component**: on deployment build, iterate `manager.Terrain.Cells` (or
  `grid.TerrainCells`), instantiate the themed prop for each non-Passable cell at `config.CellToWorld(coord)`.
  Mirrors how `DeploymentGridRenderer` builds tiles.
- **NavMesh carve** (same cell set): drop a `NavMeshObstacle` (carving) box ~`cellSize` on every `Impassable`
  cell so the baked arena NavMesh is cut at runtime — **no re-bake needed**. Best lives next to `TerrainView`
  since both consume `TerrainCells`. (The arena `Plane` already has a `NavMeshSurface`; see PROJECT_STATE.)
- **Contract used:** `TerrainCells` + `CellToWorld` + `cellSize`. Read-only; B never writes the model.

## Slice C — encounter builder (later, iterative)
Turns a map node into a concrete fight: roster + obstacle layout + obstacle-aware placement.

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
