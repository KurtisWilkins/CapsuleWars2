# Project State — CapsuleWars2

> **This is a SNAPSHOT, not a log.** Overwrite stale lines every handoff so it
> always describes the project *right now*. Keep it short enough to read in 30s.

_Last updated: 2026-06-25 — build-to-spec: Slice 1 + BTS-A (event triggers) + BTS-B1 (status stat getters + resistance fixes); 215 green._

## Content completeness vs design docs (build-to-spec, ADR-028)
| Category | Spec | Built | Status |
|---|---|---|---|
| **Elements** (Docs/08) | 15 types + 5×5 chart + dual rule | 15 + chart + multipliers + **dual-element rule (new)** | ✅ **complete to spec** |
| **Abilities** (Docs/05) | ~30 strategy classes | 8 prior + 9 (targeting/filters/effects) + **8 triggers (BTS-A)** = 25; event-bus wiring done | ⏳ remaining: GetAttacker (BTS-A pt2), HighestThreat (needs dmg stat), BuffStat/KnockBack/Teleport/VFX/Evolve |
| **Status** (Docs/10) | 24 effects | 1 authored; **stat getters + resistance bugs fixed (BTS-B1)** | ⏳ author 24 assets (BTS-D); 7 behavioral need the `StatusEffectBehavior` damage hook (BTS-B2) |
| **Classes** (Docs/09 + roster) | 16 + tiers | **16 `WeaponClass_SO` (BTS-C) + 16 `UnitClass_SO` w/ [content] tiers + `globalBuffs` (BTS-E1)** | ⏳ BTS-E2: [code] behavioral tiers wire once status/effect/hook slices land; assign classes→units (BTS-F); retire `Class_Warrior` placeholder. No class on a live unit yet (Play-gated) |
| **Equipment** (Docs/07) | 8 slots × 5 rarities | code ✅; 6 items (all Common) | ⏳ 4 empty slots, none above Common, no roll-config/loot assets, rarity ×s to align to doc |
| **Evolution** (new, in scope) | XP/floor stat growth | — | ⏳ not started (gates EvolveEffect + evolution-indexed strategies) |

## One-line status
**Themed-encounter system: Slices A + B + C built (ADR-024/025/026/027).** Combat nodes now **generate a complete
encounter**: a seeded obstacle/hazard `TerrainLayout` (player deploy zone kept clear, scales with depth) AND a
generated **obstacle-aware enemy roster** — `EncounterGenerator` (`Run.Encounters`) decides terrain + how many
enemies + which Passable enemy-zone cells (avoiding obstacles); `EncounterBuilder` (−100) stamps terrain before the
arena renders + NavMesh-bakes it, and `EnemyEncounterSpawner` (−75, run-gated) spawns the roster from
`Unit_Enemy.prefab` (mirrors `BattlePartySpawner`). All deterministic from `RunState.Seed ^ CurrentNodeId` (no save
needed); wired into `Test_M3_Battle`. **v1 enemies are base clones** (visual variety + smarter strategy iterate
next). The battle board is **built at runtime from themed blocks** by `ArenaBuilder` — a checkerboard floor (one tile per grid cell, 1:1 with the deployment grid),
raised obstacle blocks on Impassable cells + markers on Hazard cells, driven by a `ThemeBlockSet`/`EncounterTheme`
SO (primitive-cube fallback → works with no assets; grass + volcanic placeholders authored). After building it
re-bakes the `NavMeshSurface` (PhysicsColliders; obstacles = NotWalkable) so agents path around obstacles. The
checkerboard is the BASE layer; the existing CellState tints overlay the same cells. Wired into `Test_M3_Battle`
+ editor preview (`Tools/Arena/*`); **190/190 EditMode green** (+9 arena tests). Editor-preview self-verified
(tiling/alignment/obstacles/hazard/checkerboard); NavMesh + pathing + look are Play-gated below. Slice **C**
(encounter builder: enemy roster + obstacle layout + obstacle-aware placement) is next, specified in
`Docs/18_ThemedEncounters.md` + TASKS. Repo trunk-based on `main` (ADR-020); rollback tag `pre-trunk-main`.
Earlier features (paper-doll ADR-021, battle camera ADR-022) still await a human Play pass (below).

## Repo / branch state
- **Trunk: `main`** (= `origin/main`) — the only working branch now. Work on `main` or short-lived feature
  branches (ADR-020). `claude/deployment-grid` was pruned (local + remote). The remote still keeps
  `claude/capsule-wars-setup-pBoDq` (an old setup branch, fully contained in `main` — prunable).
- Rollback point: tag **`pre-trunk-main`** (`852a520`, pushed) = `main` before the consolidation fast-forward.
- Tests: `Assets/Scripts/Tests/EditMode/` — **215 green**. Run `run_tests` after any C# change.

## What currently works
- Milestone base through ~M9 (draft → battle → recruit; combat, abilities, elements, synergies, status, stats;
  run/map; legacy + customization-unlock pipelines).
- **Equipment = Definition + runtime Instance (ADR-019):** `Equipment_SO` is identity-only; an `EquipmentInstance`
  (def ref + rolled `modifiers` + generated name + tier/seed) carries stats and is what's saved.
  `EquipmentRoller`/`EquipmentRollConfig` build explicit or seeded rolls. A compat `Equip(Equipment_SO)` overload +
  DTO default-instance migration preserve old/starter items' stats. Stats flow through `UnitStatusController`
  (the stat math; ADR-007). EditMode-tested (two instances of one SO → different stats).
- **Deployment phase (v2):** 7×9 grid (cellSize 3.5), near player zone (rows 0–2) + far enemy zone (rows 6–8);
  `DeploymentTray` HUD; `DeploymentPhaseController` gate (combat blocked until Assemble); spawn-on-place via
  `BattlePartySpawner`; enemy-zone cell tap → shared `UnitInspectionPanel`. **Placements persist + auto-restore
  on re-entry (ADR-023):** your last layout re-deploys each combat (still editable via bench-tap / Clear).
- **Board terrain/obstacles (ADR-024, Slice A of themed encounters):** each cell has a `TerrainType` —
  `Passable` / `Impassable` (blocks placement + pathing) / `Hazard` (placeable-but-harmful, per
  `DeploymentGridConfig.allowPlaceOnHazard`, default on). Pure `Combat.Deployment` model: `DeploymentGrid`
  `SetTerrain/GetTerrain/IsImpassable/IsHazard/TerrainCells` (legacy `SetBlocked/IsBlocked` = Impassable wrappers);
  `IsDeployable`/`GetState` are terrain-aware (`CellState.Hazard`). A serializable `TerrainLayout` is authored inline
  on `DeploymentManager` and stamped on `Awake`. EditMode-tested (9 tests). Theming + NavMesh carve (Slice B) and the
  encounter builder (Slice C) are specified in `Docs/18_ThemedEncounters.md`, not built.
- **Runtime modular-block arena (ADR-025, Slice B):** `ArenaBuilder` (`UI.Arena`, on `Test_M3_Battle`) builds the
  board at runtime from themed blocks — checkerboard floor (1 tile/cell, 1:1 with the grid), raised obstacles on
  Impassable cells, hazard markers on Hazard cells, sized from `cellSize`, driven by `ThemeBlockSet`/`EncounterTheme`
  SOs (`Data.Arena`; primitive-cube fallback when no prefab; grass + volcanic placeholders in `Assets/Settings/Arena/`).
  After building, re-bakes the `NavMeshSurface` (PhysicsColliders; obstacles `NavMeshModifier`=NotWalkable) so agents
  path around obstacles; the legacy Plane is hidden but its collider stays as the placement-raycast + walkable ground.
  Editor preview via `Tools/Arena/*` + component ContextMenus. Demo terrain authored (Impassable (2,4)(4,4)(3,5) +
  Hazard (3,3), neutral rows). Real kits (Kubikos/Meshy) drop in by assigning prefabs to a block set — no code change.
- **Seeded encounter terrain (ADR-026, Slice C1):** `EncounterGenerator` (pure, `Run.Encounters`) + `EncounterDefinition`
  SO + `EncounterBuilder` (battle-scene, exec order −100). In a run, each combat node generates a reproducible
  obstacle/hazard `TerrainLayout` from `RunState.Seed ^ CurrentNodeId` (+ floor scaling; player deploy zone kept
  clear), applied via `DeploymentManager.SetTerrain` before `ArenaBuilder` renders + bakes it. Wired into
  `Test_M3_Battle` with `EncounterDefinition_Default`; no run → keeps the authored demo terrain. Editor preview:
  `Tools/Arena/Preview Generated Encounter`.
- **Generated obstacle-aware enemy roster (ADR-027, Slice C2/C3):** `EncounterGenerator.RosterSize` (count by
  boss/floor) + `.EnemyCells` (Passable enemy-zone cells avoiding Impassable) + `EnemyEncounterSpawner`
  (`Run.Encounters`, exec −75, run-gated): in a run, retires the scene enemy and spawns the roster from
  `Unit_Enemy.prefab` (clones; mirrors `BattlePartySpawner`). Deterministic from `Seed ^ CurrentNodeId`; difficulty
  rides `RunBattleSetup`'s depth boost. Wired into `Test_M3_Battle`. **v1 = base clones; visual variety + smarter
  placement + per-unit stats iterate next.** Spawn is Awake-only (Play-verify) — watch NavMesh-bake-vs-spawn order.
- **Battle/deployment camera (ADR-022):** `DeploymentCameraController` auto-frames the board clear of the HUD for
  deployment (tilt 84, inset 0.30), eases to a computed **~45° TFT-style** view on Assemble (not the authored
  pose), and allows **free pan/zoom during combat** (`allowControlDuringBattle`; zoom moves along the view
  direction). Editor F5/F6 + ContextMenu reframe live for tuning. Feel is human-gated (knobs below).
- **Branching run map (code-complete):** seeded branching+infinite graph (`MapNode`/`RunMap`/`MapGenConfig`/
  `MapGenerator`), graph `RunState`, `RunController.TravelToNode` + stitch-on-clear + loss-only end,
  `MapView`/`MapNodeView`. `Test_M7_Map` assembled.
- **Customization (paper-doll, ADR-021) — code complete; scene assembly pending checklist:** centered live preview
  flanked by gear slots + a cosmetic body-slot row, HP/DAMAGE/ARMOR footer + Stats (reuses `UnitInspectionPanel`),
  scrollable Gear/Body bag. Tap auto-equips to the item's own slot; drag-and-drop equips (wrong slot rejects); tap
  a filled slot unequips. Gear → `UnitStatusController.Equip`; body parts → `UnitCustomization.ApplyParts`; both
  persist (`dto.Equipment` + `dto.Parts`, the latter only when a part was edited). `UnitEquipmentVisuals` still
  shows equipped meshes on sockets, live + on combat units. **Assemble + wire per `Docs/CHECKLIST_PaperDoll.md`.**
- **Asset Creation Pipeline (editor-only, `Assets/Scripts/Editor/AssetPipeline/`):** `AssetRequest` queue with a
  Lifecycle (Active/Archived/Rejected); the Asset Pipeline window (stage groups; Grok/Meshy prompt copy + live
  Generate; paste image/model; Create/Wire item; archive/reject/restore; mirror sided parts); `StyleProfile` +
  `PartTemplate` + `StyleComposer` (shared art style); `GrokImageService`/`MeshyModelService`/
  `AnthropicDescriptionService` (keys in git-ignored `Tools/Editor/SecretsConfig.json`). Live Grok + Meshy +
  Create/Wire verified; Anthropic description needs account credits.

## Needs human verification (Play mode — Claude can't see it; D3D11 is the project default, no flag needed)
1. **Re-bake the NavMesh first** — `Test_M3_Battle` → `Plane` → `NavMeshSurface` → **Bake** (arena enlarged:
   Plane scale 4, centre (10.5,0,14)); combat movement needs it. (Note: the new `ArenaBuilder` ALSO re-bakes at
   runtime on build — this manual bake is the editor baseline; verify the runtime bake in item 1b.)
1b. **Runtime modular-block arena (ADR-025, `Test_M3_Battle`) — the focus of this session.** Enter Play (in a run
   so deployment loads). Verify: blocks tile with **no gaps/overlap** and match unit sizing; the **checkerboard**
   reads as discrete cells from the deployment camera (player-zone tint still distinguishable on top); the **3
   obstacle blocks** sit on cells (2,4)(4,4)(3,5) and the **hazard** marker on (3,3); the legacy Plane is hidden
   (board is the visible floor); **placement raycasts still land** (tap a player-zone cell → unit appears on the
   right tile); after Assemble the **NavMesh bakes** and **agents path AROUND the obstacle cells** (don't walk
   through); leaving + re-entering the combat **rebuilds cleanly** (no leftover/duplicate blocks). Eyeball without
   Play via `Tools/Arena/Build Preview (open scene)` then `Clear Preview`. Tune look by assigning real
   prefabs/materials to the `ThemeBlockSet` assets in `Assets/Settings/Arena/` (no code change), or bump
   floor A/B material contrast for clearer cells.
1c. **Seeded encounter terrain (ADR-026, Slice C1)** — in a RUN (start from `Test_M7_Map` → a Combat node), verify
   each combat node shows a generated obstacle/hazard field (not the fixed demo layout); the **player deploy rows
   stay clear** (every player-zone cell still placeable); re-entering the SAME node regenerates the SAME board
   (deterministic); deeper floors get more obstacles; agents path around the generated obstacles after Assemble.
   Eyeball without Play via `Tools/Arena/Preview Generated Encounter (open scene)` → `Clear Preview` (don't save).
1d. **Generated enemy roster (ADR-027, Slice C2/C3) — Awake-only, needs Play in a RUN.** Enter a Combat node in a
   run: generated enemies appear in the **enemy zone, off the obstacle cells** (not the single authored
   `Unit_Enemy`); count varies by floor (boss = fixed); re-entering the SAME node spawns the SAME roster; and they
   **fight normally**. **Main thing to watch:** the spawner runs in Awake before `ArenaBuilder` re-bakes the NavMesh
   in Start — confirm the spawned `NavMeshAgent`s are on the mesh and MOVE; if they don't, reorder the spawn to
   after `ArenaBuilder.Bake()` (see ADR-027). If something's off, enemies only break inside a run (standalone scene
   keeps the authored enemy). **→ now addressed by ADR-031 (item 1e) — agents self-heal onto the baked mesh.**
1e. **Battle-start polish fixes (ADR-031) — the focus of this fix; resolves the 1d NavMesh caveat.** Enter a Combat
   node in a run and press **Battle Start**. Verify all three: (a) **deployment highlight** — the green player-zone
   tiles now read ON TOP of the checkerboard floor (they were buried under it); (b) units spawn at **full scale,
   planted on the tiles** (not compressed/floating); (c) after Battle Start, units **move + attack + animate** (no
   longer frozen in Idle — off-mesh NavMeshAgents self-heal via `SamplePosition`+`Warp` in `UnitMovementController`).
   If a unit still won't move, confirm a baked navmesh exists within ~2m of its cell. Button now reads **Battle
   Start** (was "Assemble").
2. **Deployment loop** — load a combat node with a drafted party → tap a bench unit → tap a green player-zone
   cell ⇒ the real unit appears (scale-in); tap a placed cell to bench it; **Clear**; **Assemble** ⇒ those exact
   units start combat (no dupes; not before Assemble). (Placement + enemy inspection + Assemble were Play-verified
   2026-06-21; re-confirm after the NavMesh bake.)
2b. **Battle/deployment camera (ADR-022, `Test_M3_Battle`) — the focus of this fix.** Verify in Play:
   - **Deployment:** all THREE player rows (0,1,2) sit ABOVE the HUD and every cell is clickable/placeable; no taps
     dropped over the bottom band. Pan (WASD/drag) + zoom work during deployment.
   - **Assemble:** the camera eases to a ~45° board view (not the old bad angle).
   - **Battle:** pan (WASD/drag/touch) + zoom (scroll/pinch) both work; zoom dollies toward the board; the camera
     stays in bounds and can frame the fight.
   - **Tune by eye (editor):** nudge a field on Main Camera's `DeploymentCameraController`, then **F5** (re-frame
     deployment) / **F6** (re-frame battle), or right-click the component → Re-apply. Knobs per symptom: rows still
     under the HUD → ↑ `bottomViewportInset` (0.30→0.36) and/or ↑ `deploymentTiltDegrees` (84→88); too flat →
     ↓ `deploymentTiltDegrees` (84→80); battle too steep/shallow → `battleTiltDegrees` (45 ±5); can't see the whole
     board in battle → ↓ `battleTiltDegrees` / ↑ `battleFov` / widen bounds; camera snaps back when panning →
     widen `boundsMin`/`boundsMax`; zoom too fast or flies off → `scrollZoomSpeed`/`pinchZoomSpeed`,
     `minHeight`/`maxHeight`.
3. **Branching map** (`Test_M7_Map`) — start a run → branching map renders → pick a start → encounter → return →
   only edge-connected nodes clickable → climb + clear the Boss → a new segment stitches on; lose → run ends.
   Tune MapView spacing if cramped; delete any leftover old Map Panel content if it peeks through.
4. **Paper-doll customization** (`Test_M7_Map`) — assembled + **Play-verified 2026-06-23**: opens for a live
   unit; gear + body slots + Gear/Body bag generate; live HP/DAMAGE/ARMOR; **tap-equip, tap-unequip, and
   drag-and-drop (ghost + background auto-route) all confirmed.** STILL to verify by a human: visual layout
   tuning (the builder is re-runnable); wrong-slot drag rejection (red flash); the **Stats** button →
   `UnitInspectionPanel`; equipping a **body part from the Body bag tab**; and the **persistence round-trip**
   (gear + body-part edits survive Close + reopen + show on the combat unit). Note: starter items currently carry
   no stat modifiers, so equipping them doesn't move the numbers (content, not a UI bug).
5. **Equipment rolled item** — roll one (`EquipmentRoller.Roll(def, rollConfig, tier, seed)`), equip it →
   inspection shows its stats + generated name while the mesh attaches; a starter/old item keeps stats after load.
6. **Mirror equip** — equip a mirrored (opposite-side) part and confirm it shows on the correct side.
7. **Generated Meshy mesh** — check scale/orientation at the socket (generated models sometimes need a tweak).

## Known issues / notes
- **GPU crash FIXED:** Graphics API for Windows is **Direct3D11 only** (Auto off, D3D12 removed) in
  `ProjectSettings.asset` — Unity D3D12 on the Qualcomm Adreno X1-85 driver caused device-removed crashes. Editor
  title shows `<DX11>`; **`-force-d3d11` is NOT needed** (D3D11 is the default).
- CoplayDev MCP bridge drops on every domain reload (recompile / entering Play) and cycles ports — reconnect.
- Game-view MCP screenshots are blank; use `manage_camera capture_source=scene_view` or computer-use on the
  Game/Simulator window. (Computer-use clicks are sometimes blocked by Windows touch-keyboard / "Click to Do".)
- MCP object/component ref fields need object form (`{"instanceID":N}` / `{"path":"..."}`); bare ints fail.
- Working tree carries untracked **test artifacts** under `Assets/Generated/` + `Assets/Editor/AssetPipeline/Requests/`
  (Mikey/TestHelmet pipeline tests) — throwaway, intentionally not committed.
- Cosmetic: the battle scene's placeholder "New Text" labels were cleared; `Test_M3_Idle` + `Test_M7_Map`
  still have default "New Text" labels (likely runtime-driven HUD/node text) — clear if any show through.

## Key paths
- Stats (NO StatCalculator — ADR-007): `Assets/Scripts/Units/Controllers/UnitStatusController.cs`. Equipment:
  `Assets/Scripts/Data/Equipment/` (`Equipment_SO`, `EquipmentInstance`, `EquipmentRoller`, `EquipmentRollConfig`,
  `EquipmentCatalog`). UnitFactory (DTO↔runtime): `Assets/Scripts/Persistence/UnitFactory.cs`; DTOs in `Persistence/Dto/`.
- Deployment: `Assets/Scripts/Combat/Deployment/` + UI `Assets/Scripts/UI/Deployment/` (DeploymentTray,
  DeploymentGridRenderer) + `UI/Camera/DeploymentCameraController.cs`. Spawn/persist: `Run/BattlePartySpawner.cs`,
  `RunState.Placements`, `Persistence/`.
- Run map: `Assets/Scripts/Run/Map/` + `RunController`/`RunState`/`RunSession`; UI `Assets/Scripts/UI/Map/`;
  prefabs `Assets/Prefabs/Map/`.
- Customization: `Assets/Scripts/UI/Customization/` + `Assets/Scripts/Units/Customization/`.
- Asset pipeline (editor-only): `Assets/Scripts/Editor/AssetPipeline/` (+ `/Style`); requests/style assets under
  `Assets/Editor/AssetPipeline/`; API keys at project-root `Tools/Editor/SecretsConfig.json` (git-ignored).
- Scenes: `Assets/Scenes/Test_M3_Battle.unity` (combat + deployment), `Test_M7_Map.unity` (map + between-rounds).
