# Project State — CapsuleWars2

> **This is a SNAPSHOT, not a log.** Overwrite stale lines every handoff so it
> always describes the project *right now*. Keep it short enough to read in 30s.

_Last updated: 2026-06-23 — battle-camera fix (ADR-022) in `Test_M3_Battle`: clear-HUD deploy frame, 45° battle frame, free pan/zoom in combat; 172 green._

## One-line status
**Customization is now a paper-doll** (ADR-021): **code complete, 169/169 EditMode green, committed (`79ba7fe`)**
on `main` — centered live preview flanked by gear slots + a cosmetic body-slot row, HP/DAMAGE/ARMOR footer +
Stats button, scrollable **Gear/Body** bag; **tap** auto-equips to the item's own slot, **drag-and-drop** equips
(wrong slot rejects), **tap a filled slot** unequips. Reuses the equip backend (no new stat math); **body-part
edits now persist**. The panel is now **assembled in `Test_M7_Map` and Play-verified** (built via the
`PaperDollBuilder` editor tool — `Tools/Paper-Doll/Build In Open Scene`; opens for a live unit, slots + body
slots + bag generate, live stats, and **tap-equip, tap-unequip, and drag-and-drop all confirmed in Play**). Layout
still wants visual tuning (the builder is re-runnable). Repo is trunk-based on `main` (ADR-020); rollback tag
`pre-trunk-main`. Other gameplay still needs a Play pass (see "Needs human verification").

## Repo / branch state
- **Trunk: `main`** (= `origin/main`) — the only working branch now. Work on `main` or short-lived feature
  branches (ADR-020). `claude/deployment-grid` was pruned (local + remote). The remote still keeps
  `claude/capsule-wars-setup-pBoDq` (an old setup branch, fully contained in `main` — prunable).
- Rollback point: tag **`pre-trunk-main`** (`852a520`, pushed) = `main` before the consolidation fast-forward.
- Tests: `Assets/Scripts/Tests/EditMode/` — **169 green**. Run `run_tests` after any C# change.

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
  `BattlePartySpawner`; enemy-zone cell tap → shared `UnitInspectionPanel`.
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
   Plane scale 4, centre (10.5,0,14)); combat movement needs it.
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
