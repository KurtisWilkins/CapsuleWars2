# Tasks — CapsuleWars2

> The **top unblocked item** is what the next session starts with. Keep it
> specific and self-contained. Move finished items to "Done (recent)".

## Next up (work top-down)
- [ ] **Wire the between-rounds trigger for the customization screen.** Nothing
      calls `CustomizationScreen.Show(unitId)` yet, so it's unreachable in-game.
      Add an entry point in the map scene (`Test_M7_Map`): e.g. a button per party
      unit in the map HUD, or a `RunController` hook shown after a battle node
      completes. `CustomizationScreen` lives on the `CustomizationScreen` GameObject
      under `Canvas`; `RunSession.Current.Party` holds the unit ids.
- [ ] **Play-mode verify deployment** (`Test_M3_Battle`): in PreBattle, tap a
      unit's cell → inspection panel shows that unit's stats; tap an empty deploy
      cell → the selected unit moves there; confirm tiles colour by `CellState`.
- [ ] **Tune `DeploymentGridConfig`** (origin / cellSize / playerRowMin–Max) to the
      actual arena so the deploy zone sits under the player units. Keep the copy on
      `BattlePartySpawner.deploymentGrid` in sync (it maps saved placements → world).
- [ ] **Camera pass (Slice A):** tune `DeploymentCameraController` bounds/feel on the
      battle Main Camera; swap `DeploymentCell.prefab`'s material to a transparent one
      so the green/blue/red alpha tints read correctly.

## Backlog (not yet scheduled)
- [ ] Persist body-part / palette edits made in the customization screen.
      `UnitFactory.FromUnit` currently captures **equipment only**, not `Parts`/`PaletteId`,
      so cosmetic edits there won't save until that's extended.
- [ ] Real equipment source for customization. Only `EquipmentCatalog.asset`
      (Eq_IronSword, Eq_LeatherChest) exists — there is no loot/inventory system.
- [ ] Stripped "preview" unit prefab for the customization screen (the full
      `Unit_Sample_Prefab` drags in NavMeshAgent/movement → harmless warnings in the
      map scene with no NavMesh).
- [ ] Land `claude/unit-factory` → `main`, then merge the stacked feature branches in
      order (equipment-persistence → deploy-camera → unit-inspection → deployment-grid).
- [ ] Clean up battle-end UI placeholder "New Text" labels in `Test_M3_Battle`.
- [ ] Remaining M10 polish: real audio clips + event wiring, settings screen UI +
      input rebinds, tutorial, multi-arena, balance pass. See `Docs/17_BuildOrder.md`.

## Done (recent — prune periodically)
- [x] Cross-session continuity system (this file + CLAUDE.md + Docs/ + skills).
- [x] Customization screen built in `Test_M7_Map` (panel + inspection prefab instance
      + EquipButton prefab + PreviewAnchor; all refs wired).
- [x] Unit inspection panel built in `Test_M3_Battle` + extracted to a reusable prefab;
      renders + hides-on-Play confirmed.
- [x] Deployment grid: model + `DeploymentManager` (AutoArrange, cell-based selection)
      + view + renderer; tiles render and units auto-arrange in Play.
- [x] Deployment camera controller (pan/zoom/clamp, PreBattle-gated) wired to Main Camera.
- [x] Run-scoped equipment + run-state + placement persistence (UnitFactory equipment,
      RunStore, RunStateDTO). EditMode-tested.
- [x] `UnitStatusController.OnStatsChanged` event for live UI refresh.
- [x] Fixed the pre-existing EditMode baseline regression. **155/155 EditMode green.**

## Notes
- New ideas discovered mid-task go in Backlog, not into the current task.
- Run the EditMode suite (`run_tests`) after C# changes; it should stay green (155).
