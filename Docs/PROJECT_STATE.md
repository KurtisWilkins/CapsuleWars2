# Project State — CapsuleWars2

> **This is a SNAPSHOT, not a log.** Overwrite stale lines every handoff so it
> always describes the project *right now*. Keep it short enough to read in 30s.

_Last updated: 2026-06-21, after the equipment runtime-instance refactor (ADR-019) — branch `claude/deployment-grid`_

## One-line status
**Equipment stats → runtime instances (NEW, ADR-019):** `Equipment_SO` is now the **Definition** (identity only;
its `statBuffs`/`rarity` are legacy migration-source). A new `EquipmentInstance` (definition ref + rolled
`modifiers` + generated name + tier/seed) carries the stats and is what's saved; `UnitStatusController` equips an
instance and its modifiers flow through the same stat math (combat unchanged). `EquipmentRoller` +
`EquipmentRollConfig` build instances explicitly or by a seeded, data-driven roll ("Helmet of Health"). A compat
`Equip(Equipment_SO)` overload + default-instance DTO migration keep old items/saves' stats. **166/166 EditMode
green** — incl. a test proving two instances of ONE helmet give different `MaxHp`/`Atk`. Customization/asset
pipeline untouched.
**Archive/Reject lifecycle:** AssetRequests gain a `Lifecycle` (Active/Archived/Rejected) separate from
Stage. The pipeline window has a **view bar** (`Active (N) · Archived (N) · Rejected (N)`, default Active) and
per-request **Archive / Reject / Restore** (+ Complete & Archive on Done) with a reason + timestamp; Restore
returns to the preserved stage; **Delete** stays the only destructive (confirm) action. Archiving never deletes
the produced game asset (no `DeleteAsset` in the lifecycle path). **Self-tested via computer-use:** archived a
Categorized request → left Active, showed under Archived with timestamp, produced model/prefab intact → restored
to its original stage. 162/162 green.
**Mirror/flip for paired parts:** sided requests (R/L hand or foot) get a **"Mirror to opposite side"**
button that horizontally flips the approved image and creates a linked opposite-side `AssetRequest`
(`mirrorOf`, flipped image, opposite slot, Meshy prompt ready) — verified via the bridge (clean horizontal
mirror, correct left-hand request). Built on the **shared Grok art-style system:** consistency is now structural. A single `StyleProfile` SO + per-part
`PartTemplate` SOs (`Assets/Editor/AssetPipeline/Style/`) compose every Grok prompt as base + part criteria +
concept + finish + avoid (`StyleComposer`), so editing the StyleProfile restyles all future generations. The
Grok call sends fixed `aspect_ratio`/`resolution` (+ opt-in reference-image edit path), auto-sets the Meshy
prompt, and a batch button generates all pending images. **Verified live this session via computer-use:** Grok
+ Meshy + Create/Wire all run end-to-end; the composed prompt is correct; 162/162 EditMode green. **The whole
asset pipeline (queue + Grok/Meshy/Create-Wire) is working with real keys** (Anthropic description needs account
credits). Prior passes (Branching Map, Customization v2) still await their Play-mode checks.

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
  gizmo colour the enemy zone. All committed; gate/zone/grid logic unit-tested (166 green).
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
- **Asset Creation Pipeline (NEW, editor-only):** `Assets/Scripts/Editor/AssetPipeline/` — `AssetRequest` SO
  (per-asset queue record, persisted under `Assets/Editor/AssetPipeline/Requests/`), **Asset Pipeline**
  `EditorWindow` (queue grouped by `PipelineStage`; add/advance/rollback, copy Grok/Meshy prompts, paste
  image+model, category/slot/socket, **Create / Wire item**, edit description), `PromptTemplates`
  (Rayman/AssetHunts-locked), `AssetPipelineImporter` (prefab under `Assets/Generated/Meshy/{slot}/` +
  `Equipment_SO`/`BodyPart_SO` + catalog add via `SerializedObject`), and an `IGenerationService`/`SecretsConfig`
  seam (no keys → assisted-manual). Compiles clean; window opens cleanly. End-to-end run = human-verify (above).

## In progress
- Nothing mid-edit. The deployment feature is code-complete + scene-wired; only the Play-mode
  verification + arena tuning remain (below).

## Needs human verification (Claude can't see Play Mode)
- **Equipment runtime instances (ADR-019) — core VERIFIED by EditMode test; Play visual optional.** The headline
  (two `EquipmentInstance`s from one `Equipment_SO` → different `MaxHp`/`Atk` via `UnitStatusController`) is an
  automated test (166/166 green), and the same definition still drives the mesh (the `.item` getter is unchanged
  for `UnitEquipmentVisuals`). Optional human check: in customization/Play, equip a rolled item and confirm the
  inspection panel shows its stats + generated name while the helmet **mesh still attaches**; and that a
  pre-existing/starter item keeps its stats after load (default-instance migration). To roll an item in code:
  `EquipmentRoller.Roll(definition, rollConfig, tier, seed)` (seed = reproducible) or
  `EquipmentRoller.Explicit(definition, modifiers)`; then `status.Equip(slot, instance)`. Author an
  `EquipmentRollConfig` via **Create ▸ CapsuleWars ▸ Equipment ▸ Roll Config** to define the stat pool.
- **Archive/Reject lifecycle — VERIFIED via computer-use (2026-06-21).** Archived the wired "Test Helmet"
  (Categorized) → it left Active (4→3), appeared under **Archived** with an "Archived — <time>" stamp + Reason
  field, its `Generated/Items/Equipment` + `Meshy/Helmet` assets stayed intact; **Restore to Active** returned it
  to Categorized (4 again); live counts + empty-view message correct. Reject + Complete-&-Archive use the same
  `SetLifecycle` path. Nothing left to verify here, but if you want: confirm an Archived item's `createdItem`
  still equips on a unit in Play (Play visual — the asset is untouched by archiving, so this is just a sanity check).
- **Mirror/flip — Play-mode equip check (the rest is verified).** I verified via the bridge: flipping the
  RightHand "Mikey mouse hands" produced a clean **horizontal** mirror (same resolution, grayscale, plain bg)
  and a linked **LeftHand** request (`mirrorOf`, flipped image, `targetSlot`=LeftHand, Meshy prompt ready).
  Manual checklist to confirm in-editor: open **Tools ▸ CapsuleWars ▸ Asset Pipeline**, expand an approved
  right-hand request (has a Chosen image) → **Mirror to opposite side** → accept the symmetry warning → a NEW
  left-hand request appears linked as "mirror of", flipped image attached, Meshy prompt ready, at ImageChosen;
  open both images and confirm a clean horizontal mirror (same resolution, still grayscale, plain background, no
  rotation). **Still human-only:** equip/show the mirrored part on the *opposite* side in Play/customization,
  then exit Play. (Re-running the mirror updates the same `{id}_Left/Right` asset — no duplicates.)
- **Shared style consistency (visual check + tuning).** Run **Tools ▸ CapsuleWars ▸ Create Default Style +
  Templates** (done — 1 `StyleProfile` + 8 `PartTemplate`s under `Assets/Editor/AssetPipeline/Style/`). Review
  + tune the `StyleProfile` (cartoony spine / finish / avoid / `aspect_ratio` / `resolution`) and each
  `PartTemplate`'s criteria. Then generate **two different parts** (e.g. a Helmet and a Right Hand) and confirm
  both come out in the same grayscale/isolated/flat-bg cartoony style; edit `StyleProfile.basePrompt`, regen
  one, and confirm the change carries. Note: xAI has **no seed** (consistency = shared prompt + fixed framing);
  the reference-image toggle is opt-in/best-effort. **Composition + a live Grok generate with the new params
  were verified this session** (the RightHand prompt composed correctly + saved an image); the *visual* sameness
  across parts is the human check.
- **Asset Pipeline live APIs — VERIFIED end-to-end (2026-06-21, via computer-use in the editor).** Ran the
  "Mikey mouse hands" (BodyPart / RightHand) sample: **Grok Generate** → image saved + set as Chosen image
  (`grok-imagine-image-quality`); **Meshy Generate** → polled to 100%, FBX imported + assigned
  (`Assets/Generated/Meshy/RightHand/MikeyMouseHands.fbx`); **Create / Wire item** → created
  `BodyPart_SO 'MikeyMouseHands'` and added it to `PartCatalog`. **Anthropic description**: integration is
  correct (auth + request OK) but returned HTTP 400 *"credit balance is too low"* — **needs Anthropic account
  credits**, not a code fix (or just let Claude write the description directly). Still worth a human check:
  equip/spawn the created `BodyPart_SO` on a unit and confirm the Meshy mesh renders at the RightHand socket
  (scale/orientation may need tuning), and confirm nothing pipeline-related ships in a player build (all under
  Editor folders / `CapsuleWars.Editor`).
- **Deployment placement + enemy inspection + Assemble — VERIFIED in Play (2026-06-21, D3D11).** Played the
  full run (draft → branching map → combat → deployment): placed both units on player-zone cells, clicked an
  enemy → stats panel showed (Tester Ted), **Assemble** confirmed (HUD hides, camera reframes, combat starts).
  Bug found + fixed in the process: the `EnemyInspectionPanel` root Image (raycastTarget on) overlapped the
  right-side Clear/Assemble buttons and ate their clicks while hidden → fixed by disabling that raycastTarget
  (committed). Remaining tuning is optional (`bottomViewportInset`/`framingOffset`, panel rect).
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
- **GPU crash FIXED (2026-06-21):** the Editor/Standalone **Graphics API for Windows is now Direct3D11 only**
  (Auto off, D3D12 removed) in `ProjectSettings.asset`. Root cause was Unity's **D3D12 on the Qualcomm Adreno
  X1-85 (Snapdragon X ARM64) driver 31.0.133.1** → unrecoverable device-removed crashes (5 in one session).
  Verified: editor title shows `<DX11>`, Play no longer crashes. **`-force-d3d11` is no longer needed** — the
  older verification notes below that mention that flag are obsolete (D3D11 is the project default now).
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
- Tests: `Assets/Scripts/Tests/EditMode/` (**166** green).
- Branch: `claude/deployment-grid` — **pushed** and up to date with `origin/claude/deployment-grid`. `main`
  (= `origin/main`) is **134 commits behind** it. The old per-slice stack (ADR-009) has **collapsed into this
  branch**: all 5 local feature branches (incl. `claude/unit-factory`, never pushed) are fully contained — zero
  unique commits. Remote has only `main`, `claude/deployment-grid`, `claude/capsule-wars-setup-pBoDq`. **Branch
  consolidation is pending a decision** (recommend FF `main` → `deployment-grid` + go trunk-based — see TASKS).
