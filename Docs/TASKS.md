# Tasks — CapsuleWars2

> The **top unblocked item** is what the next session starts with. Keep it
> specific and self-contained. Move finished items to "Done (recent)".

## Next up (work top-down)
- [ ] **Tune the shared style + confirm cross-part consistency.** Review/tune `StyleProfile` + the 8
      `PartTemplate`s under `Assets/Editor/AssetPipeline/Style/`, then generate **two different parts** (e.g.
      Helmet + Right Hand) and confirm they come out in the same grayscale/isolated cartoony style; edit
      `StyleProfile.basePrompt`, regen one, confirm it carries. (Pipeline Grok+Meshy+Create/Wire already verified
      live; Anthropic description needs account credits. Full checklist in PROJECT_STATE.)
- [ ] **Check the generated Meshy mesh on a unit** — equip/spawn the created `BodyPart_SO` and confirm the
      model's scale/orientation at the socket (generated models sometimes need a scale tweak).
- [ ] **Play-test the branching map** (`Test_M7_Map`, `-force-d3d11`). Scene is assembled (ScrollRect +
      `MapView` wired, node label font set). Start a run → branching map renders → pick a start → encounter →
      return → only connected nodes clickable → climb → clear Boss → new segment stitches on; lose → run ends.
      Tune `RunController` `MapGenConfig`/`fixedSeed`/`difficultyPerDepth` + MapView spacing as desired; delete
      any leftover old Map Panel content if it shows through. (Full notes in PROJECT_STATE.)
- [ ] **Re-bake the NavMesh for the enlarged arena.** In `Test_M3_Battle`, select `Plane` →
      `NavMeshSurface` → **Bake** (the Plane was scaled to 4, centred at (10.5,0,14)). Without this,
      combat movement on the bigger board will be broken/off-mesh.
- [ ] **Play-mode test deployment v2** (needs a drafted run; launch editor with `-force-d3d11`). In
      `Test_M3_Battle` with `RunSession.Current.Party` non-empty: tap a bench unit → tap a green
      player-zone cell ⇒ the **real unit appears** at the cell (scale-in); tap a placed cell to bench it
      (instance destroyed); **Clear** removes all. **Assemble** ⇒ those exact units start combat (no
      duplicates); combat must NOT start before Assemble. Confirm the enemy sits on the far side and the
      camera auto-frames the board. Report what renders/works.
- [ ] **Play-mode test customization v2** (`Test_M7_Map`, `-force-d3d11`): Customize → picker on top +
      clickable → pick a unit → screen in front, all equip buttons respond. List has the 4 starters →
      click an item ⇒ its cube appears at the matching socket on the preview + button highlights; click
      again ⇒ removed. Close (saves) → battle with that unit ⇒ cube shows on the combat unit too. Verify
      stats still update live.
- [ ] **Swap placeholder item visuals:** `EquipVisual_Cube` is a stand-in. Assign real `visualPrefab`/
      `visualMesh` per `Equip_Starter*` asset; optionally re-parent the unit's `Socket_*` empties under
      hand/head bones so attachments follow animation.

## Backlog (not yet scheduled)
- [ ] Bench-item prefab polish: deployment + customization reuse `EquipButton.prefab`; make a dedicated
      unit-card prefab (icon + name) if desired.
- [ ] Stripped "preview" unit prefab (no NavMeshAgent) for the customization screen.
- [ ] Persist body-part/palette edits (`UnitFactory.FromUnit` captures equipment only).
- [ ] Persistent run-scoped inventory (owned-item ids in `RunStateDTO`) seeded on new game; the screen
      currently shows the `EquipmentCatalog` (now 6 items incl. 4 starters) ∪ serialized `starterItems`.
- [ ] Clean up battle-end UI placeholder "New Text" labels (cosmetic; pre-existing). Remaining M10 polish.
- [ ] **Branch consolidation — PENDING your decision (cleanup session).** `main` is 134 commits behind
      `claude/deployment-grid`, which **is pushed** and **fully contains all 5 stale local branches** (incl.
      `claude/unit-factory`, which was never pushed). Recommended: fast-forward `main` → `deployment-grid`,
      push, delete the contained branches, work trunk-based — would supersede ADR-009. (Obsolete prior plan
      "land unit-factory → main then merge the stack" is moot: nothing has unique commits.)

## Done (recent — prune periodically)
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
- Run the EditMode suite (`run_tests`) after C# changes; keep it green (currently **166**).
