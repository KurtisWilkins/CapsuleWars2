# Tasks — CapsuleWars2

> The **top unblocked item** is what the next session starts with. Keep it
> specific and self-contained. Move finished items to "Done (recent)".

## Next up (work top-down)
- [ ] **Re-bake the NavMesh for the enlarged arena.** In `Test_M3_Battle`, select `Plane` →
      `NavMeshSurface` → **Bake** (the Plane was scaled to 4, centred at (10.5,0,14)). Without this,
      combat movement on the bigger board will be broken/off-mesh.
- [ ] **Play-mode test deployment v2** (needs a drafted run; launch editor with `-force-d3d11`). In
      `Test_M3_Battle` with `RunSession.Current.Party` non-empty: tap a bench unit → tap a green
      player-zone cell ⇒ the **real unit appears** at the cell (scale-in); tap a placed cell to bench it
      (instance destroyed); **Clear** removes all. **Assemble** ⇒ those exact units start combat (no
      duplicates); combat must NOT start before Assemble. Confirm the enemy sits on the far side and the
      camera auto-frames the board. Report what renders/works.
- [ ] **Play-mode verify the customization loop** (`Test_M7_Map`): Customize → pick a unit → equip →
      live stats update → persists across Close/restart/battle.

## Backlog (not yet scheduled)
- [ ] Bench-item prefab polish: deployment + customization reuse `EquipButton.prefab`; make a dedicated
      unit-card prefab (icon + name) if desired.
- [ ] Stripped "preview" unit prefab (no NavMeshAgent) for the customization screen.
- [ ] Persist body-part/palette edits (`UnitFactory.FromUnit` captures equipment only).
- [ ] Real equipment source / loot-inventory (only `EquipmentCatalog` with 2 items exists).
- [ ] `BattleStartButton` is now redundant with Assemble (gated) — hide it during deployment or remove.
- [ ] Replace deprecated `FindObjectsByType(FindObjectsSortMode)` calls (CS0618 warnings) in
      DeploymentView/LegacyPromoteButton/RunBattleSetup.
- [ ] Land `claude/unit-factory` → `main`, then merge the stacked feature branches in order.
- [ ] Clean up battle-end UI placeholder "New Text" labels. Remaining M10 polish (see `Docs/17_BuildOrder.md`).

## Done (recent — prune periodically)
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
- Run the EditMode suite (`run_tests`) after C# changes; keep it green (currently 158).
