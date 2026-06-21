# Architecture — CapsuleWars2

> Stable structural reference. Update when the *shape* of the project changes,
> not for day-to-day progress (that's PROJECT_STATE / SESSION_LOG).
> (Verified against the repo on 2026-06-21.)

## Genre / loop
Auto-battler / roguelite: the player drafts/arranges a team, units fight
automatically on a NavMesh arena, and roguelite progression (recruit, customize,
unlocks) happens between rounds. Detailed design lives in `Docs/00`–`Docs/17`
(milestone/build docs).

## Assemblies (enforced layering)
Code is split into `CapsuleWars.*` asmdefs; lower layers must **not** reference UI/Run:

`Core` → `Data` → `Units` → `Abilities` → `Combat` → `Run` / `Legacy` → `Persistence` → `UI`
(plus `Audio`, `Generation`, `Editor`, `Tests.EditMode`, `Tests.PlayMode`).

Rule of thumb: **Combat must not depend on UI or Run.** UI is the top integration
layer and may reference everything. Cross-layer data moves as DTOs (Persistence)
and via the static `CombatServices` locator (Core).

## Core systems
- **UnitFactory** (`Assets/Scripts/Persistence/UnitFactory.cs`) — the single entry
  point for turning a `UnitDTO` into a configured runtime unit and back:
  `FromDTO(dto, unit, defDb, partDb, equipDb)`, `FromUnit(unit)`, and the
  instantiating `Spawn(...)`. Resolves ids through `IUnitDefinitionDatabase`,
  `IPartDatabase`, `IEquipmentDatabase`. Applies identity + visuals (definition or
  explicit parts/palette) + equipment; captures the same back out.
- **Stat computation** (`Assets/Scripts/Units/Controllers/UnitStatusController.cs`) —
  **there is no `StatCalculator` class** (see ADR-007). Final stats are read-only
  getters (`MaxHp/Atk/Def/Speed`) that fold base values + equipped items (× rarity)
  + active status effects + class-synergy buffs pushed by `SynergyResolver`. Raises
  `OnStatsChanged` for live UI; status effects raise `OnStatusApplied/Expired`.
- **ScriptableObject data layer** (`Assets/Scripts/Data/`, assets in `Assets/Data/`) —
  `UnitDefinition_SO`, `Equipment_SO` (+ rarity), `ElementType_SO` + `ElementChart`,
  `UnitClass_SO` (synergy tiers), `StatusEffect_SO`, `BodyPart_SO`/`Palette_SO`,
  `Ability_SO`, and catalogs (`UnitDefinitionCatalog_SO`, `PartCatalog_SO`,
  `EquipmentCatalog_SO`) that build id→asset databases.
- **Combat** (`Assets/Scripts/Combat/`) — `BattleStateManager` (PreBattle → Active →
  Resolved), `BattleContext` + `CombatServices` (Core locator: `Registry`, `Phase`,
  `ElementChart`), `BattleEventBus`, `BattleStatsAggregator`, `Stats/SynergyResolver`.
- **Abilities** (`Assets/Scripts/Abilities/`) — composable `Ability_SO` =
  Trigger + Targeting + Filters + Effects; `AbilityController` ticks `AbilityRuntime`s.
- **Run / Legacy / Persistence** — `RunController`/`RunState`/`RunSession` drive the
  roguelike map; `RunStore` (`run.json`) + `LegacyStore` (`legacy.json`) persist via
  Newtonsoft JSON; DTOs in `Persistence/Dto/` (`UnitDTO`, `RunStateDTO`, `MapNodeDTO`,
  `UnitPlacementDTO`, `LegacyUnitDTO`, …). DTOs use primitive ints for enum/coords so
  Persistence stays free of Run/Combat refs.

## Battle/customization UI (this feature set)
- **Deployment grid** — model in Combat (`Combat/Deployment/`: `GridCoord`,
  `DeploymentGridConfig` grid↔world + player zone, `DeploymentGrid` occupancy/`CellState`,
  `DeploymentManager` placement authority with `AutoArrange`). View in UI
  (`UI/Deployment/`: `DeploymentView` cell-based tap input via Input System,
  `DeploymentGridRenderer` colours tiles by state). Spawn-then-arrange via
  `BattlePartySpawner`; placements persist in `RunState`. The grid is placement-only —
  combat movement is NavMesh.
- **Unit inspection** — `UI/Inspection/UnitInspectionPanel.cs` (+ reusable prefab) reads
  `UnitRoot.Status` (stats/element/class), `AbilityController.Runtimes`, equipment, and
  `UnitCustomization`; live-refreshes on `OnStatsChanged`. Used by deployment and customization.
- **Customization** — `UI/Customization/CustomizationScreen.cs` (map scene) spawns a live
  *preview* unit from the selected party `UnitDTO`, equips armor from `EquipmentCatalog`
  (→ `OnStatsChanged` → inspection updates live), and writes equipment back into the party
  DTO + saves the run.
- **Deployment camera** — `UI/Camera/DeploymentCameraController.cs`, pan/zoom/clamp,
  gated to `BattlePhase.PreBattle`.

## Data flow
SO definitions → `UnitFactory.Spawn` → runtime `UnitRoot` (+ `UnitStatusController`,
`AbilityController`, controllers) → combat resolution via `CombatServices`/event bus →
UI reads `UnitStatusController` directly (inspection/customization). Persistence:
runtime ↔ `UnitDTO`/`RunStateDTO` via `UnitFactory`/`RunState`, stored by `RunStore`.

## UI surfaces
- Deployment grid + roster (battle scene, PreBattle) — built.
- Unit inspection panel (shared) — built.
- Between-rounds customization screen (map scene) — built, not yet triggered.
- Existing run UI: RunHud, map/shop/event/draft/recruit/run-end panels, legacy roster.

## Tooling
- Engine: Unity 6 (6000.4.7f1), URP, C#. Input System package (new) active.
- Live editing: Claude Code via **CoplayDev Unity MCP bridge** (~v9.7.1). Bridge drops on
  domain reload; Game-view screenshots blank (use scene_view / computer-use). Assets,
  scenes, prefabs are **Git LFS-tracked** (diffs show as small oid bumps).
- Tests: NUnit EditMode (155 green) under `Assets/Scripts/Tests/EditMode`.

## Conventions
- Thin MonoBehaviours; logic in plain C# systems where practical (e.g. the deployment
  grid model is plain C# in Combat, the MonoBehaviour view lives in UI).
- One responsibility per ScriptableObject type; ids resolve via catalog/database interfaces.
- MonoBehaviour init that EditMode tests rely on must be lazy (Awake doesn't run on
  `AddComponent` in edit mode) — see `UnitHealthController`/`DeploymentManager`.
