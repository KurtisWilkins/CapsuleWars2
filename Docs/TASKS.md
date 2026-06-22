# Tasks — CapsuleWars2

> The **top unblocked item** is what the next session starts with. Keep it
> specific and self-contained. Move finished items to "Done (recent)".

## Next up (work top-down)
- [ ] **Run one sample asset through the Asset Pipeline** (`Tools ▸ CapsuleWars ▸ Asset Pipeline`). + New
      request → ask Claude for concepts → pick → Copy Grok prompt → run Grok → drop image → Copy Meshy prompt
      → run Meshy → import FBX to `Assets/Generated/Meshy/{slot}/` → drop model → set category/slot/socket →
      **Create / Wire item** → equip/spawn on a unit and confirm the mesh shows at the socket. (Full checklist
      in PROJECT_STATE "Needs human verification".)
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
- [ ] `BattleStartButton` is now redundant with Assemble (gated) — hide it during deployment or remove.
- [ ] Replace deprecated `FindObjectsByType(FindObjectsSortMode)` calls (CS0618 warnings) in
      DeploymentView/LegacyPromoteButton/RunBattleSetup.
- [ ] Land `claude/unit-factory` → `main`, then merge the stacked feature branches in order.
- [ ] Clean up battle-end UI placeholder "New Text" labels. Remaining M10 polish (see `Docs/17_BuildOrder.md`).

## Done (recent — prune periodically)
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
- Run the EditMode suite (`run_tests`) after C# changes; keep it green (currently 162).
