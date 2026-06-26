# Tasks — CapsuleWars2

> The **top unblocked item** is what the next session starts with. Keep it
> specific and self-contained. Move finished items to "Done (recent)".

## Next up (work top-down)
- [ ] **START HERE — Play-verify + tune the battle camera** (`Test_M3_Battle`, ADR-022; this just landed). Re-bake
      the NavMesh FIRST (item below). Then: deployment shows all 3 player rows above the HUD + every cell clickable;
      pan/zoom work in deployment; Assemble → ~45° board view; battle → pan + zoom (zoom dollies toward the board) +
      stays in bounds. Dial the feel live — nudge a field on Main Camera's `DeploymentCameraController`, press F5
      (deploy frame) / F6 (battle frame) or right-click → Re-apply. Knobs-per-symptom table in PROJECT_STATE.
- [ ] **Play-mode verification pass for the rest of the gameplay** (D3D11 default; no `-force-d3d11`). Walk the
      full per-item checklist in PROJECT_STATE.
- [ ] **Finish the paper-doll checks + tune its layout** (`Test_M7_Map`; built + Play-verified 2026-06-23 —
      tap-equip/unequip + drag-drop all confirmed). Re-run `Tools/Paper-Doll/Build In Open Scene` after tuning
      anchors in `PaperDollBuilder` if needed. Still to confirm by hand: visual layout polish; wrong-slot drag
      rejection; **Stats** button → `UnitInspectionPanel`; equip a **body part** from the Body bag tab; and the
      **persistence round-trip** (gear + body edits survive Close + reopen + show on the combat unit).
- [ ] **Re-bake the NavMesh for the enlarged arena.** `Test_M3_Battle` → `Plane` → `NavMeshSurface` → **Bake**
      (Plane scaled to 4, centred (10.5,0,14)). Gates combat movement on the bigger board.
- [ ] **Play-test deployment v2** (`Test_M3_Battle`, drafted run). Tap a bench unit → green player-zone cell ⇒
      the real unit appears (scale-in); tap a placed cell to bench it; **Clear**; **Assemble** ⇒ those exact units
      start combat (no dupes; NOT before Assemble); enemy on the far side; camera auto-frames. (Placement/enemy
      inspection/Assemble were Play-verified 2026-06-21 — re-confirm after the NavMesh bake.)
- [ ] **Play-test the branching map** (`Test_M7_Map`). Start a run → map renders → pick a start → encounter →
      return → only edge-connected nodes clickable → climb + clear Boss → new segment stitches on; lose → ends.
      Tune MapView spacing if cramped; delete any leftover old Map Panel content if it peeks through.
- [ ] **Equipment rolled-item + mirror equip (visual):** roll an item (`EquipmentRoller.Roll(def, config, tier,
      seed)`), equip → inspection shows stats + generated name while the mesh attaches; equip a mirrored part →
      shows on the correct side; a starter/old item keeps its stats after load.
- [ ] **Tune the shared style + check generated-mesh scale:** review the `StyleProfile` + 8 `PartTemplate`s;
      generate two different parts and confirm one coherent grayscale/isolated style; edit `StyleProfile.basePrompt`,
      regen, confirm it carries. Then check the generated Meshy mesh's scale/orientation at the socket.
- [ ] **Swap placeholder item visuals:** `EquipVisual_Cube` is a stand-in. Assign real `visualPrefab`/`visualMesh`
      per item; optionally re-parent `Socket_*` empties under hand/head bones for animated attachment.

### Themed encounters — continue the system (Slices A + B landed; see `Docs/18_ThemedEncounters.md` + ADR-024/025)
- [ ] **Play-verify the runtime arena (ADR-025)** — PROJECT_STATE item 1b: blocks tile with no gaps, checkerboard
      reads, obstacles/hazard on the right cells, NavMesh bakes + agents path around obstacles, placement raycasts
      still land, rebuilds cleanly between encounters.
- [ ] **Drop in real block art (no code change).** Assign Kubikos / Meshy-generated block prefabs (+ materials) to
      the `ThemeBlockSet` assets in `Assets/Settings/Arena/` (FloorA/FloorB/Obstacle/HazardMarker per theme). Tune
      floor A/B material contrast for cell readability; add more `EncounterTheme`s (forest/cave/ship/sewer/…). The
      `ArenaBuilder` scales prefabs to `cellSize` — check chunky-kit prefabs aren't distorted (may want a per-theme
      scale or to skip the auto-scale for authored prefabs).
- [ ] **Slice C — encounter builder (iterative).** Turn a map node into a fight. Build: an `EncounterDefinition`
      SO (roster spec by `NodeType` + depth, an obstacle `TerrainLayout` or seeded generator, a placement strategy)
      + a builder that stamps the layout (`TerrainLayout.ApplyTo`), generates the roster (extends
      `RandomUnitGenerator` — note stats/class/abilities are still TODO there), and places enemies on **Passable**
      cells within `config.InEnemyZone` avoiding `IsImpassable` (Hazard per strategy); + a new `EnemyEncounterSpawner`
      (mirror of player-only `BattlePartySpawner` — enemies are scene-placed static today). **Contract consumed:**
      `grid.GetTerrain/IsImpassable/IsHazard`, `config.InEnemyZone`, `TerrainLayout`. Generate from
      `RunState.Seed + nodeId` for reproducibility (no extra save unless terrain becomes non-deterministic). Attach
      at `MapNode.Type` (Combat/Elite/Boss) via `RunController`/`RunBattleSetup`. Likely wants an enemy-zone analogue
      of `DeploymentManager.FirstFreeCell`.

## Backlog (not yet scheduled)
- [ ] Bench-item prefab polish: deployment + customization reuse `EquipButton.prefab`; make a dedicated
      unit-card prefab (icon + name) if desired.
- [ ] Stripped "preview" unit prefab (no NavMeshAgent) for the customization screen.
- [ ] Persistent run-scoped inventory (owned-item ids in `RunStateDTO`) seeded on new game; the screen
      currently shows the `EquipmentCatalog` (now 6 items incl. 4 starters) ∪ serialized `starterItems`.
- [ ] *(Optional)* clear the remaining default "New Text" labels in `Test_M3_Idle` + `Test_M7_Map` (the battle
      scene's 4 were cleared; these are likely runtime-driven HUD/node labels — clear only if they show through).

## Done (recent — prune periodically)
- [x] **Themed encounters — Slice B runtime block arena (ADR-025, 2026-06-25):** `ArenaBuilder` (`UI.Arena`) builds
      the board at runtime from `ThemeBlockSet`/`EncounterTheme` SOs (`Data.Arena`, primitive fallback) — checkerboard
      floor 1:1 with the grid, raised obstacles on Impassable cells, hazard markers, sized from cellSize; runtime
      `NavMeshSurface` re-bake (PhysicsColliders + obstacle NotWalkable). Wired into `Test_M3_Battle` via
      `ArenaSetupTool` (grass + volcanic placeholders, demo terrain) + editor preview. Pure `ArenaLayout` math.
      **190/190 green** (+9 arena tests); editor-preview self-verified. Play-verify + real art are the next items above.
- [x] **Themed encounters — design + Slice A terrain model (ADR-024, 2026-06-25):** designed the 3-slice system
      (`Docs/18_ThemedEncounters.md`); built Slice A — `TerrainType` (Passable/Impassable/Hazard) generalizing the
      binary `blocked` flag, `DeploymentGrid` terrain map (`SetTerrain/GetTerrain/IsImpassable/IsHazard/TerrainCells`
      + `SetBlocked/IsBlocked` compat wrappers), `IsDeployable`/`GetState` extension (`CellState.Hazard`),
      `DeploymentGridConfig.allowPlaceOnHazard`, `TerrainLayout` (+ `DeploymentManager` Awake stamp), renderer
      `hazardColor`. Pure logic, self-verified: **181/181 green** (+9 terrain tests). B + C captured above with their
      contracts. No Play needed (carve visual is Slice B).
- [x] **Deployment layout persists + auto-restores (ADR-023, 2026-06-23):** `DeploymentTray.RestoreSavedPlacements()`
      replays `RunState.Placements` (already saved) on combat entry — your last layout re-deploys; stale/not-in-party
      placements dropped; bench/Clear still edit. Reuses tested primitives; 172/172 green (`2a0bede`). Play-verify:
      deploy → fight → next combat is pre-deployed.
- [x] **Battle-camera fix (ADR-022, 2026-06-23):** `DeploymentCameraController` — deployment frame clears the HUD
      (tilt 78→84, inset 0.22→0.30), `FrameBattle` computes a ~45° TFT board view (was: snap to authored pose),
      `allowControlDuringBattle` enables free pan/zoom in combat, zoom moves along the view direction (clamped),
      bounds widened, editor-only F5/F6 + ContextMenu tuning aid. Scene values updated + saved; added
      `DeploymentCameraTests` (Clamp + forward-zoom). 172/172 green. Commit `aab0146`. Play feel = human-gated.
- [x] **Paper-doll panel assembled + Play-verified (ADR-021, 2026-06-23):** built `Test_M7_Map`'s customization
      panel via the re-runnable `PaperDollBuilder` editor tool (containers + footer + Gear/Body bag + buttons,
      old list UI removed, all 13 refs wired by `SerializedObject`). Play-verified end-to-end via computer-use:
      opens for a live unit; slots + body slots + bag generate; live HP/DAMAGE/ARMOR; **tap-equip, tap-unequip,
      drag-and-drop (ghost + auto-route) all work.** 169 green. Commit `c2678ef`.
- [x] **Paper-doll customization — code (ADR-021):** reworked the customization screen into a paper-doll (gear
      slots + cosmetic body slots, tap-route + drag-drop with wrong-slot reject + tap-unequip, HP/DAMAGE/ARMOR
      footer, Gear/Body bag, Stats reuses `UnitInspectionPanel`). `UnitCustomization` exposes
      `AppliedParts`/`AppliedPalette`/`MountedSlots`; new self-building widgets `PaperDollSlot`/`BagItemWidget`/
      `PaperDollDropZone`; `CustomizationScreen` rewritten. **Body-part edits now persist** (`Capture` writes
      `dto.Parts` when edited) — closes the old "persist body-part edits" backlog item. 169/169 green (`79ba7fe`).
      In-editor scene assembly is the START-HERE checklist (bridge couldn't build it blind).
- [x] **Final cleanup touches:** pruned `claude/deployment-grid` (local + remote; was identical to `main`, an
      ancestor — no commit loss); cleared the battle scene's 4 placeholder "New Text" labels. `main` is now the
      only branch.
- [x] **Trunk consolidation (ADR-020, supersedes ADR-009):** fast-forwarded `main` → `deployment-grid` (clean FF,
      no work lost), pushed `main`, tagged `pre-trunk-main` (852a520) as rollback, deleted the 5 contained local
      branches. Now trunk-based on `main`; `deployment-grid` kept as a synced pointer (prunable).
- [x] **Repo hygiene (cleanup session 2026-06-22):** committed 10 orphaned `.meta` (committed scripts/folders
      whose metas were never staged → GUID-regeneration risk on clone); removed dead `DeploymentView`
      (inert scene component + script + meta — superseded by `DeploymentTray`, ADR-011/014). Verified two
      backlog items were themselves stale: `BattleStartButton` is **already disabled** (commit `fdab6a5`), and
      the "deprecated `FindObjectsByType` CS0618" item is a **false alarm** — `FindObjectsByType(FindObjectsSortMode)`
      is the *current* API (zero CS0618; the deprecated `FindObjectsOfType` is unused). 166/166 EditMode green.
- [x] **Equipment stats → runtime instances** (ADR-019): `Equipment_SO` = identity Definition (legacy stats kept
      for migration); new `EquipmentInstance` (def ref + modifiers + name + tier/seed) carries stats + is saved;
      `UnitStatusController` equips an instance (modifiers via the same stat math); `EquipmentRoller` +
      `EquipmentRollConfig` build explicit/seeded rolls with generated names. Compat overload + default-instance
      DTO migration keep old items/saves. 166/166 EditMode green (incl. two-instances-one-helmet test).
- [x] **Archive/Reject lifecycle for the pipeline queue** (ADR-018): `AssetRequest.Lifecycle`
      (Active/Archived/Rejected) + reason/date, separate from Stage; window view bar (counts) + lifecycle filter;
      per-request Archive/Reject/Restore (+ Complete & Archive on Done); reason field; Delete reworded as the only
      destructive action. Archiving never deletes the produced item (no `DeleteAsset` in the path). Self-tested via
      computer-use (archive → Archived view → restore to prior stage; item intact). 162 green.
- [x] **Image mirror/flip for paired parts** (ADR-017): "Mirror to opposite side" on sided requests (R/L hand
      or foot) — `MirrorUtil` (sidedness via slot) + `MirrorAction` (horizontal PNG flip → linked opposite-side
      `AssetRequest` with `mirrorOf`, opposite slot, flipped image, Meshy prompt; idempotent deterministic id;
      original never overwritten; symmetry warning). Window button + non-modal MenuItem. `AssetRequest` +
      `mirrorOf`/`asymmetric`. Verified via bridge (clean horizontal mirror + correct left-hand request); 162 green.
- [x] **Shared Grok art-style system + live API** (ADR-016): `StyleProfile` (single source of truth) +
      `PartTemplate`s + `StyleComposer` (base + part criteria + concept + finish + avoid); `StyleSetupTool`
      seeder (1 profile + 8 templates); `GrokImageService` aspect/resolution + opt-in `/v1/images/edits`;
      `GenerationActions` composes + sets meshyPrompt + sequential batch; window Style + batch buttons.
      Verified live (composed prompt correct, Grok generate with new params OK, meshyPrompt auto-set). 162 green.
- [x] **Live generation APIs verified end-to-end** (computer-use): Grok image + Meshy 3D + Create/Wire all work
      with real keys; Anthropic description integration correct but account out of credits. Grok model fixed to
      `grok-imagine-image-quality`; Meshy hardened (omit ai_model, request fbx/glb).
- [x] **Asset Creation Pipeline + queue** (ADR-015): editor-only `Assets/Scripts/Editor/AssetPipeline/` —
      `AssetRequest` SO (stage-by-stage queue, persisted), **Asset Pipeline** EditorWindow (grouped by stage;
      copy Grok/Meshy prompts, paste image+model, Create/Wire item, edit description), `PromptTemplates`
      (style-locked), `AssetPipelineImporter` (prefab + `Equipment_SO`/`BodyPart_SO` + catalog), and an
      `IGenerationService`/`SecretsConfig` API seam (assisted-manual; no keys). 162/162 green; window opens clean.
- [x] **Assemble-click bug + deployment Play-verification** (D3D11): the `EnemyInspectionPanel` root Image
      (raycastTarget on) overlapped the right-side Clear/Assemble buttons and ate their clicks while hidden →
      disabled that raycastTarget. Verified in Play: placement, enemy inspection, and Assemble→combat all work.
- [x] **Deployment placement fix + enemy inspection** (ADR-014): camera `bottomViewportInset`/`framingOffset`
      frame the board above the bottom HUD (player-zone clicks were dropped as "over UI"); legacy
      `DeploymentView` disabled; enemy-zone cell tap → shared `UnitInspectionPanel` (read-only, top-right),
      wired in `Test_M3_Battle`. 162/162 green.
- [x] **Branching run map** (ADR-013): seeded branching+infinite graph model/generator (`MapNode` edges,
      `RunMap`, `MapGenConfig`, `MapGenerator` GenerateInitial/AppendSegment); graph `RunState` (CurrentNodeId,
      depth, Seed, TravelTo, DifficultyMultiplier, AppendNextSegment); `RunController.TravelToNode` + stitch +
      loss-only end; DTOs v2; `MapView`/`MapNodeView` + node/edge prefabs. 162/162 green. Scene wiring = checklist.
- [x] **Customization v2** (ADR-012): screen/launcher self-promote to front + clickable (`EnsureForeground`);
      equip toggle + selected highlight; 4 starter items in the catalog; `UnitEquipmentVisuals` renders
      equipped items as meshes on named sockets (RightHand/LeftHand/Helmet/Chest) live + in combat. 160 green.
- [x] **Deployment v2** (ADR-011): spawn-on-place (`BattlePartySpawner.SpawnOrMoveAt/Despawn/DespawnAll`
      driven by `DeploymentTray`; placed instances become combat units, no double-spawn); bigger board
      (cellSize 3.5) + far enemy zone (rows 6–8, `InEnemyZone`) coloured in renderer + gizmo; camera
      auto-frames from the grid; enemy repositioned + Plane enlarged. 160/160 green.
- [x] **Deployment Phase** (7×9 grid + gizmos; DeploymentPhaseController confirm gate; place-then-spawn
      `BattlePartySpawner`; DeploymentManager token placement; DeploymentTray bench HUD with Assemble/Clear;
      camera auto-frame; wired in `Test_M3_Battle`). 158/158 green; HUD renders.
- [x] Between-rounds customization launcher/trigger (`CustomizationLauncher`).
- [x] Cross-session continuity system (CLAUDE.md + Docs/ + skills).
- [x] Customization screen, inspection panel (+prefab), deployment grid core, camera controller,
      run-scoped equipment/run-state/placement persistence, `OnStatsChanged`, baseline test fix.

## Notes
- New ideas discovered mid-task go in Backlog, not into the current task.
- Run the EditMode suite (`run_tests`) after C# changes; keep it green (currently **172**).
