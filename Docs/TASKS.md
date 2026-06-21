# Tasks — CapsuleWars2

> The **top unblocked item** is what the next session starts with. Keep it
> specific and self-contained. Move finished items to "Done (recent)".

## Next up (work top-down)
- [ ] **Play-mode test the full deployment loop** (needs a drafted run). In `Test_M3_Battle`,
      start with `RunSession.Current.Party` non-empty (draft via `Test_M7_Map`, or temporarily
      seed a party): the bottom **DeploymentHUD** bench should list the party → tap a unit then a
      green deploy-zone cell to place it → tap a placed unit to send it back → **Assemble** →
      units spawn at the placed cells and combat starts → **Clear** empties the board and combat
      must NOT start before Assemble. Report what renders/works.
- [ ] **Tune the deployment camera + grid to the arena.** On `Main Camera`'s
      `DeploymentCameraController`, set deploymentPosition/euler/FOV so it frames the whole 7×9 board;
      on the `Deployment` object's `DeploymentManager.config` (and the matching `BattlePartySpawner.deploymentGrid`)
      set origin/cellSize so the grid sits on the arena. Check the gizmo in the Scene view.
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
