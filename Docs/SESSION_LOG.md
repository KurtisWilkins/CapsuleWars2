# Session Log — CapsuleWars2

> **Append-only.** Newest entry at the TOP. Read only the last 1–2 entries when
> resuming — do not load the whole file into context.
> Copy the template at the bottom for each new entry.

<!-- NEW ENTRIES GO HERE (top = newest) -->

## 2026-06-23 — Deployment layout auto-restores between combats (ADR-023)
**Ask:** the player shouldn't have to re-deploy units every combat — auto-deploy the last layout, still editable.
**Found:** the save side was already done — `RunState.Placements` is written by the deployment UI and round-trips
to disk; nothing clears it between combats (only the user's bench-tap / Clear do). The gap was purely RESTORE: in
spawn-on-place mode nothing replayed the saved layout, so each combat started with an empty board.
**Did (commit `2a0bede`, 172/172 green):** added `DeploymentTray.RestoreSavedPlacements()` (called in `Start`):
re-places each saved placement for a current party member via the normal `PlaceToken` + `SpawnOrMoveAt` path,
leaves them off the bench, drops stale (not-in-party) placements; benched units unchanged. Additive, reuses tested
primitives, no combat logic touched. **Play-verify (human):** deploy in one combat → fight → next combat the same
units are pre-deployed; bench/Clear still change them; survives app restart (saved to disk).

## 2026-06-23 — Battle/deployment camera fix (ADR-022)
**Goal:** focused fix of `DeploymentCameraController` in `Test_M3_Battle` — three problems: (1) only the front
player row was reachable (other two hidden behind the HUD), (2) Assemble snapped to a bad angle with no control,
(3) no pan/zoom to watch the fight.

**Measured first (scene YAML, since the "Click to Do" overlay blocked computer-use clicks):** the HUD
`DeploymentHUD` is a bottom-anchored 230px band with an **opaque raycast-target Image** over the whole strip, in a
**720×1280 / match-0.5** CanvasScaler → ~0.18 of height in the reference but **~0.32 on a wide/landscape view**,
while `bottomViewportInset` was a fixed 0.22 → near rows fall under the band. Battle pose was the authored Awake
pose; `restrictToDeployment` locked all input once combat started.

**Changed (commit `aab0146`, 172/172 EditMode green):**
- Deployment: `deploymentTiltDegrees` 78→84 + `bottomViewportInset` 0.22→0.30 (script defaults + scene overrides).
- `TryComputeBoardFraming` generalized to `(tilt, inset, fov)`; `FrameBattle` computes a `battleTiltDegrees`
  (45) frame with the authored pose as fallback (`computeBattleFromGrid`).
- `allowControlDuringBattle` (default true) overrides `restrictToDeployment` for combat (still locked during the
  transition lerp). Zoom now moves along `transform.forward` (clamped by height) instead of only `p.y`. Bounds
  widened (−10,−30 / 35,40) so the 45° pose + panning fit.
- Editor-only tuning aid (`#if UNITY_EDITOR`, never ships): ContextMenu "Re-apply deployment/battle frame" + F5/F6.
- Added `DeploymentCameraTests` (Clamp XZ+height + forward-zoom floor); added `CapsuleWars.UI` to the test asmdef.

**Not done (human-gated — it's a feel task I can't see):** Play-verify the three behaviors + dial the knobs.
Knobs-per-symptom table is in PROJECT_STATE / the START-HERE task. No combat logic touched.

## 2026-06-23 — Paper-doll panel BUILT in `Test_M7_Map` + Play-verified (ADR-021)
**Goal:** actually assemble the paper-doll scene (the prior session left it as a checklist because the bridge
couldn't read refs / the computer-use capture was blank — turned out to be remote/RDP).

**Got computer-use working:** the user was physically at the machine; capture worked once the editor was the
front window. Surfaced the running editor via the Hub (the editor process has no Start-menu entry, so
`open_application "Unity"` only ever opens the Hub; double-clicking the project there focused/loaded the editor).

**Built deterministically, not by blind clicks:** wrote `Assets/Scripts/Editor/PaperDollBuilder.cs` (editor-only,
`Tools/Paper-Doll/Build In Open Scene`). It finds the `CustomizationScreen`, treats `panelRoot` (= the panel
itself) as the root, removes the old list UI (VerticalLayoutGroup), full-stretches + transparents the panel,
generates the containers (left/right gear columns, body row, Gear/Body bag ScrollRect+grid, HP/DAMAGE/ARMOR
footer, Stats/Close/tab buttons + UIThemeApplier), and wires all 13 serialized refs via `SerializedObject`.
Deleted the leftover old children (`TitleText`/`EquipmentListRoot`/old `CloseButton`). Idempotent + re-runnable.

**Play-verified end-to-end (computer-use, drafted run):** a `Tools/Paper-Doll/TEST - Open First Party Unit`
helper calls `Show()` directly (bypasses the tangled in-scene button nav). The panel opens for a live unit; gear
slots + cosmetic body slots (green = filled with the unit's parts) + the Gear bag (starter items) all generate;
HP/DAMAGE/ARMOR show live; **tap a bag item → equips to its slot (slot fills + bag highlights); tap a filled slot
→ unequips; drag-and-drop → equips with a drag ghost + background auto-route.** Layout works but wants visual
tuning (re-run the builder after tweaking anchors). 169/169 EditMode green. Commits `c2678ef` (+ docs).
Still to confirm by hand: layout polish, wrong-slot drag reject, Stats button, body-part bag equip, persistence
round-trip. (Starter items carry no stat modifiers, so equipping them doesn't move the numbers — content, not a bug.)

## 2026-06-22 — Feature: paper-doll customization (ADR-021), code-complete + green
**Goal:** rework the customization screen into a paper-doll equip layout — centered preview, flanking gear slots,
cosmetic body-slot row, HP/DAMAGE/ARMOR footer + Stats button, scrollable Gear/Body bag; tap-to-route +
drag-and-drop (wrong slot rejects) + tap-to-unequip; reuse the equip backend; mobile + desktop.

**Built (committed `79ba7fe`, 169/169 EditMode green):**
- `UnitCustomization` now records `AppliedParts`/`AppliedPalette` + exposes `MountedSlots` (was write-only into
  mounts) so the screen can read the live body loadout back, edit it incrementally, and capture it. +3 EditMode
  tests (`UnitCustomizationTests`).
- New self-building uGUI widgets: `PaperDollSlot` (gear or body; tap-unequip + `IDropHandler` validate/reject),
  `BagItemWidget` (gear or part; tap-route + drag), `PaperDollDropZone` (background auto-route). They build their
  own icon/label visuals, so slots + bag generate at runtime with no authored prefab.
- `CustomizationScreen` rewritten: keeps the proven backend (Show/Close/SpawnPreview/EnsureForeground/Capture),
  swaps the view for the paper-doll. Gear routes via `UnitStatusController.Equip`; body parts via
  `UnitCustomization.ApplyParts`; one bag with Gear/Body tabs; HP/DAMAGE/ARMOR footer live via `OnStatsChanged`;
  Stats button reuses `UnitInspectionPanel`. Auto-creates the drop zone, drag ghost, foreground.
- **Body-part persistence** (closes the old backlog item): `Capture()` writes `dto.Parts`, guarded by
  `partsDirty` so gear-only edits don't freeze a definition unit's parts. Decision per ADR-021 (user chose to
  include cosmetic slots + build persistence).

**NOT done — scene assembly is a manual checklist (`Docs/CHECKLIST_PaperDoll.md`):** the MCP bridge this session
could not read component refs (`manage_components` has no `get`), page the hierarchy (`get_hierarchy` returns only
roots regardless of scope/target), or run editor code (`execute_code` overflows Windows' arg limit; `batch_execute`
drops params). `create`/`set_property` work, but without read-back I couldn't identify the existing `panelRoot` to
disable the old list UI, verify wiring, or see the 3D-preview-vs-overlay compositing — so building blind into the
live map scene was unsafe. Left `Test_M7_Map` pristine (deleted the one stray test object, did not save).
**Next session:** assemble per the checklist, then run its Play verification (tap-route, drag-drop + reject,
unequip, live stats, gear + body-part round-trip).


## 2026-06-22 — Cleanup / consolidation: trunk-based on `main` (ADR-020) + repo hygiene
**Goal this session:** no features — get git state, docs, and loose code hygiene into a clean, truthful, sustainable shape.

**Step 1 — found (docs had drifted):** `deployment-grid` was actually **pushed** (docs said "none pushed");
the per-slice stack had **collapsed** — all 5 other local branches contained in `deployment-grid`, zero unique
commits; `claude/unit-factory` existed locally but never on the remote; `main` was 134 behind. Also found **6
committed `.cs` with untracked `.meta`** (GUID risk), stale test counts (162 vs actual 166), a `-force-d3d11`
self-contradiction, and a garbled deployment/map bullet.

**Step 2/3 — done (committed + pushed on `main`):**
- **Branch consolidation (ADR-020, supersedes ADR-009):** FF `main` → `deployment-grid` (clean, no work lost),
  pushed `main`, tagged `pre-trunk-main` (852a520) as rollback, deleted the 5 contained local branches. Now
  trunk-based on `main`. `deployment-grid` (local+remote) kept as a synced pointer (prunable).
- **Meta hygiene:** committed the 10 orphaned `.meta` (`7e5b35d`).
- **Dead code:** removed `DeploymentView` — pulled its inert (disabled/unwired) component from `Test_M3_Battle`,
  then deleted script+meta; no missing-script left (`6806bde`). 166/166 green.
- **Verified two backlog items were themselves drift:** `BattleStartButton` already disabled (`fdab6a5`); the
  "deprecated `FindObjectsByType` CS0618" item is a **false alarm** (that's the current API; zero CS0618).
- **Doc-sync** of TASKS + PROJECT_STATE to git reality (`6aa82ef`), plus this handoff's full PROJECT_STATE rewrite.
- Reverted test-polluted catalogs (`EquipmentCatalog`/`PartCatalog` — Create/Wire test items referencing
  uncommitted `Generated/`); left those test artifacts untracked + local.

**Not done (flagged):** battle-end "New Text" placeholder labels — left for the real copy (didn't guess);
pruning `deployment-grid` (local+remote) — left to an explicit call.

**Next session starts with:** the gameplay **Play-mode verification pass** (NavMesh re-bake first, then the
deployment loop / branching map / customization v2 / mirror equip / rolled-item visual) — see PROJECT_STATE
"Needs human verification." D3D11 is the project default now (no `-force-d3d11` flag needed).

## 2026-06-21 — Equipment stats → runtime instances (Definition + Instance, ADR-019)
**Goal this session:** move equipment stats off the ScriptableObject onto a runtime, saved DTO so one asset
(e.g. a helmet) can be "of Health", "of Attack", etc. — reusable + roguelite-friendly.

**Done (committed + pushed on `claude/deployment-grid`; 166/166 EditMode green):**
- `Data/Equipment/EquipmentInstance.cs` (NEW) — `definition` (Equipment_SO) + `modifiers (List<StatBuff>)` +
  `displayName` + `tier` + `seed`; `FromDefinitionDefault` for migration. In Data so Units + Persistence both see it.
- `Equipment_SO.cs` — reframed as the **Definition** (identity); `statBuffs`/`rarity` now LEGACY default stats +
  `BuildDefaultModifiers()` (StatBuffs × rarity).
- `UnitStatusController.cs` — `EquippedItem { slot, instance }` + `item` getter (`=> instance.definition`, keeps
  visual/label readers unchanged); `Equip(slot, EquipmentInstance)` + compat `Equip(slot, Equipment_SO)` (builds a
  default instance); `ComputeMods` sums `instance.modifiers` (same SumBuffs path → combat unchanged).
- `Persistence/Dto/UnitDTO.cs` — `UnitEquipmentDTO` +`modifiers`/`displayName`/`tier`/`seed` + `From(slot,instance)`
  + `ToInstance(db)` (empty modifiers → default from the definition's legacy stats; additive, SaveVersion 1).
- `Persistence/UnitFactory.cs` — round-trips instances (FromUnit captures, ApplyEquipment rebuilds + migrates).
- `CustomizationScreen.cs` — capture writes the full instance.
- `Data/Equipment/EquipmentRollConfig.cs` + `EquipmentRoller.cs` (NEW) — data-driven pool (stat ranges/weights/
  name suffixes + per-tier rules); `Explicit(...)` and seeded `Roll(def, config, tier, seed)` (deterministic) +
  dominant-stat name gen.
- Tests: `EquipmentRollerTests.cs` (NEW) — two instances of one definition → different MaxHp/Atk; roll determinism;
  tier scaling; name gen. Existing 6 `EquipmentStatTests` stay green via the compat overload.

**Verified myself (EditMode, no Play needed):** 166/166 green, incl. the headline two-instances-one-helmet test;
clean compile. The mesh still attaches by construction (`UnitEquipmentVisuals` reads `eq.item` = the definition,
unchanged).

**Needs human verification:** optional Play/customization visual — equip a rolled item, confirm the inspection
panel shows its stats + generated name while the mesh attaches; and a starter/old item keeps stats after load.

**Next session starts with:** the open Play-mode checks (this refactor's visual, mirror equip-on-opposite-side,
style consistency, branching map, customization v2). See TASKS.

## 2026-06-21 — Archive / Reject (lifecycle) for the pipeline queue
**Goal this session:** move done/dead AssetRequests out of the active queue (Archive = completed/wired, Reject =
abandoned) without deleting anything, keeping them recoverable.

**Done (committed + pushed on `claude/deployment-grid`; 162/162 EditMode green):**
- `AssetRequest.cs` — `enum Lifecycle { Active, Archived, Rejected }` + `lifecycle` / `lifecycleReason` /
  `lifecycleDate`. **Separate from `PipelineStage`** (Stage is preserved, so Restore returns to it).
- `AssetPipelineWindow.cs` — a **view bar** (`Active (N) · Archived (N) · Rejected (N)`, default Active) filters
  the listing by lifecycle, then groups by Stage as before; per-request **Archive / Reject / Restore to Active**
  (+ **Complete & Archive** on Done items), an editable **Reason** + stamped date on archived/rejected items,
  and the existing **Delete** reworded as the only destructive (confirm) action. `SetLifecycle`/`Restore` just
  flip the field + `SaveAssets` — **no `DeleteAsset`, no touch to `createdItem`/prefab/catalog**.
- Mirror pairs are NOT auto-paired (archiving one side leaves the other; link stays visible) — flagged in the plan.

**Self-tested via computer-use (clicks worked this session — the OS overlays didn't interfere):** archived the
wired "Test Helmet" (Categorized) → it left Active (4→3), appeared under **Archived (1)** with an
"Archived — 2026-06-21 20:55" stamp + Reason field, and its `Generated/Items/Equipment` + `Meshy/Helmet` assets
stayed intact; **Restore to Active** returned it to Categorized (Active back to 4). Live counts + the
"No Archived requests" empty message both correct. Window opens with no exceptions; 162/162 EditMode green.

**Needs human verification:** optional Play sanity check that an archived item's produced asset still equips on a
unit (it's untouched by archiving). Reject + Complete-&-Archive share the verified `SetLifecycle` path.

**Next session starts with:** the still-open Play-mode checks (mirror equip-on-opposite-side, style consistency
tuning, branching map, customization v2). See TASKS.

## 2026-06-21 — Image mirror/flip for paired parts
**Goal this session:** one-click horizontal mirror of an approved sided-part image → a linked opposite-side
AssetRequest, so I don't regenerate identical R/L pairs (flip the 2D image, not the mesh, to keep normals).

**Done (committed + pushed on `claude/deployment-grid`; 162/162 EditMode green):**
- `MirrorUtil.cs` — sidedness via **slot** (both feet map to PartType.Foot, so slot not PartType):
  `TryGetOpposite(category, slot)` → opposite slot + side words. EquipmentSlot RightHand↔LeftHand;
  PartSlot hand/foot L↔R.
- `MirrorAction.cs` — `MirrorRequest(src, interactive)`: flip PNG (`Texture2D.LoadImage` → reverse each row →
  `EncodeToPNG`; pure horizontal, preserves resolution/grayscale/bg) → save `Assets/Generated/Images/{id}_{Side}.png`
  (never the original) → find-or-create `{id}_{Side}.asset` (opposite slot, side word swapped via whole-word
  regex, flipped image, `mirrorOf` link, stage=ImageChosen, `meshyPrompt` via StyleComposer, grokImagePrompt
  cleared). Idempotent (deterministic id → re-runs update, no duplicates). Symmetry warning (modal on the window
  button; non-modal console warning + refusal on the `asymmetric` flag for the MenuItem).
- `AssetRequest.cs` — `+mirrorOf` (AssetRequest link) `+asymmetric` (bool).
- `AssetPipelineWindow.cs` — "Mirror to opposite side (Side → Side)" button on sided requests (enabled when a
  Chosen image exists) + a "mirror of" link. Plus `[MenuItem]` "Tools ▸ CapsuleWars ▸ Mirror Selected Request"
  (non-modal; operates on selection else the single eligible request) for bridge/automation triggering.

**Verified myself (via the MCP bridge — computer-use is unreliable here; OS touch-keyboard / "Click to Do"
overlays keep stealing editor focus):** triggered the mirror on the RightHand "Mikey mouse hands" via
`execute_menu_item`; console logged the symmetry warning + "Created mirror 'MikeyMouseHands_Left'". `Read` both
PNGs — confirmed a clean **horizontal** mirror (fist flipped L↔R, vertical unchanged), same resolution, still
grayscale, plain white bg. `Read` the new asset — `targetSlot`=LeftHand(1), `mirrorOf` set, flipped image
attached, stage=ImageChosen, `meshyPrompt` populated, requestText side-swapped. 162/162 EditMode green.

**Needs human verification:** equip/show the mirrored part on the *opposite* side in Play/customization (Play
visual — I can't see Play mode, and the OS overlays block editor clicks here). The in-editor window-button flow
is the same logic I drove via the menu item.

**Next session starts with:** the Play-mode equip check for the mirror, plus the still-open style consistency
tuning + earlier Play-mode checks (branching map, customization v2). See TASKS.

## 2026-06-21 — Shared Grok art-style system + live API verification
**Goal this session:** (a) finish wiring the Grok/Meshy/Claude APIs and verify them live; (b) build a shared
art-style system so every generated part keeps one cartoony look.

**Done (committed on `claude/deployment-grid`; 162/162 EditMode green):**
- **Live APIs wired + verified via computer-use.** Fixed the Grok model (`grok-2-image` → **`grok-imagine-image-quality`**,
  confirmed from xAI docs) and hardened Meshy (image-to-3d doesn't take `ai_model` → omit by default; request
  `target_formats:[fbx,glb]`). Ran the "Mikey mouse hands" sample end-to-end: **Grok** image saved, **Meshy** FBX
  imported, **Create/Wire** made a `BodyPart_SO` + added it to `PartCatalog`. **Anthropic** description: code is
  correct but the account returned HTTP 400 "credit balance too low" — needs credits (or let Claude write it).
- **Shared style system (ADR-016).** New `Assets/Scripts/Editor/AssetPipeline/Style/`:
  - `StyleProfile.cs` (SO) — single source of truth: base spine, finish rules, avoid list, `aspect_ratio`/
    `resolution`, opt-in `referenceImage`. `PartTemplate.cs` (SO + `PartType` enum) — per-part criteria + limb cut.
  - `StyleComposer.cs` — resolves the active profile + the template (from category+slot) and composes
    `base + criteria + limbCut + concept + finish + "Avoid: " + avoid`. Falls back to `PromptTemplates` if no profile.
  - `StyleSetupTool.cs` — **Tools ▸ CapsuleWars ▸ Create Default Style + Templates** seeds 1 profile + 8 templates
    (Helmet/RightHand/LeftHand/Foot/Torso/Weapon/Armor/Generic); re-run safe (won't overwrite tuned assets).
  - `GrokImageService` — added `aspect_ratio`/`resolution`; added `EditAsync` for `/v1/images/edits` (opt-in ref image).
  - `GenerationActions.GenerateImage` — composes via `StyleComposer`, passes framing, optional ref-image edit,
    sets `r.meshyPrompt` after the image saves; added `GenerateImagesBatch` (sequential, pumped from Done/Fail).
  - `AssetPipelineWindow` — Copy-prompt uses the composer; toolbar **Style…** + **Generate images (N)** batch.
  - Seeded `StyleProfile` + 8 `PartTemplate` `.asset`s committed under `Assets/Editor/AssetPipeline/Style/`.

**Key facts:** xAI image API = `grok-imagine-image-quality` @ `/v1/images/generations` (params model/prompt/n/
response_format/aspect_ratio/resolution); **NO seed**; reference image only via `/v1/images/edits` (best-effort).
Keys live in git-ignored `Tools/Editor/SecretsConfig.json` (edited via the keys window).

**Verified this session:** composed Grok prompt is correct (read the `MikeyMouseHands.asset`: base + RightHand
criteria + limb cut + concept + finish + avoid), a live Grok generate with the new params saved an image, and
`meshyPrompt` auto-populated. Compiles clean; 162/162 EditMode.

**Needs human verification:** the *visual* sameness across two different parts (Helmet vs Right Hand) + tuning
the StyleProfile/templates; and checking the generated Meshy mesh's scale at the socket. (Details in PROJECT_STATE.)

**Next session starts with:** tuning the shared style + the cross-part consistency check (top of TASKS).

## 2026-06-21 — Asset Creation Pipeline + queue (editor tool) [+ Assemble-click fix verified]
**Goal this session:** build a repeatable, visible queue to design body parts / weapons / armor for the
capsule soldiers (request → concepts → Grok image prompt → Meshy 3D prompt → import → categorize + wire →
AI description → Done). Also: finish verifying the deployment **Assemble** button from the prior session.

**Done (committed on `claude/deployment-grid`; 162/162 EditMode green):**
- **Assemble-click bug fixed + Play-verified** (`165cf1f`). The buttons weren't responding because the
  `EnemyInspectionPanel` (added last session for enemy stat inspection) has a **root background Image with
  raycastTarget on**, whose rect overlapped the HUD's right-side Clear/Assemble buttons and ate their clicks
  even while the panel content was hidden (left-side bench buttons worked — that was the tell). Fix: disabled
  `raycastTarget` on that root Image. Verified in Play (D3D11): placement, enemy inspection, and Assemble →
  combat all work.
- **Asset Pipeline (ADR-015)** (`2b1a706`), editor-only under `Assets/Scripts/Editor/AssetPipeline/`
  (`CapsuleWars.Editor` asmdef):
  - `AssetRequest.cs` — ScriptableObject, one per asset; `AssetCategory` {Undecided,Weapon,EquipmentArmor,
    BodyPart} + `PipelineStage` {Requested…Done} + `ConceptOption`; holds every stage's artifact. Persisted
    under the `Assets/Editor/AssetPipeline/Requests/` Editor folder (stripped from builds; created on first use).
  - `AssetPipelineWindow.cs` — **Tools ▸ CapsuleWars ▸ Asset Pipeline**. Queue grouped by stage; + New,
    advance/rollback, category/slot/socket, **Copy Grok prompt** / **Copy Meshy prompt** (clipboard), paste
    Chosen image + Imported model, edit Description, **Create / Wire item**, Ping/Open/Delete. Generate buttons
    show only if a key is configured.
  - `PromptTemplates.cs` — concept brief + Grok image prompt (single clean asset, plain bg, for image→3D) +
    Meshy image-to-3D prompt, all with the Rayman/AssetHunts low-poly style locked in.
  - `AssetPipelineImporter.cs` — makes a prefab under `Assets/Generated/Meshy/{slot}/`, then creates an
    `Equipment_SO` (Weapon = hand slot + WeaponClass; Armor = other slots) or `BodyPart_SO` (cosmetic) with
    mesh/visualPrefab set via `SerializedObject`, and adds it to `EquipmentCatalog_SO` / `PartCatalog_SO`.
  - `IGenerationService.cs` + `SecretsConfig.cs` — API seam. No keys configured → assisted-manual; keys at
    `Tools/Editor/SecretsConfig.json` (git-ignored) or env vars light up Generate buttons later.
  - Docs updated: `16_AssetGeneration.md` (status → implemented assisted-manual), PROJECT_STATE, TASKS, DECISIONS.

**Discovery facts (for next time):** items = `Equipment_SO` (private fields + getters; set via SerializedObject;
slot=EquipmentSlot, weaponClass, attachSocketName, visualPrefab/visualMesh) and `BodyPart_SO` (PartSlot, mesh,
cosmetic — ADR-005). Equipped visuals render via `UnitEquipmentVisuals` named sockets; catalogs resolve ids at
spawn. Baseline art = AssetHunts "Capsule" kit fbx (`Assets/AssetHunts!/GameDev Essential Kit - Capsule/Source
File/`), Rayman low-poly. No Grok/Meshy/Anthropic keys present.

**Compiled?** Yes — clean (no CS errors), 162/162 EditMode pass, window opens with no exceptions (verified via
`execute_menu_item`). The interactive create/copy/wire flow needs a human (Claude can't see editor UI).

**Needs human verification:** run one sample asset through the whole pipeline (full checklist in PROJECT_STATE
"Needs human verification") — including **Create / Wire item** on a real imported model and confirming the
created item equips + shows on a unit. Note: `AssetPipelineImporter`'s SerializedObject field names + catalog
wiring are unverified at runtime (matched against source, not executed).

**Next session starts with:** the Asset Pipeline sample run above; remaining older play-tests (branching map
`Test_M7_Map`; customization v2; re-bake NavMesh for the enlarged arena) are still open in TASKS.

## 2026-06-21 — GPU crash fixed: force Direct3D11 on Windows
**Goal this session:** the editor kept crashing in Play with "d3d12: Unrecoverable GPU device error!" —
find the cause and fix it.

**Done (committed on `claude/deployment-grid`):**
- **Diagnosis (from crash logs):** `%LOCALAPPDATA%\Temp\Unity\Editor\Crashes\` had 5 dumps today; the
  current Editor.log shows the GPU is a **Qualcomm Adreno X1-85 (Snapdragon X ARM64), driver 31.0.133.1**,
  running **Direct3D 12**. The crash modules are the Qualcomm D3D12 driver DLLs + `D3D12Core.dll` → an
  unrecoverable D3D12 device-removed. It even crashed on the *draft screen* (no game code) → environmental,
  not our code.
- **Fix:** Project Settings → Player → Other Settings → **unchecked Auto Graphics API for Windows** and
  **removed Direct3D12** (Direct3D11 only). Restarted the Editor → title bar now `<DX11>`. Re-ran the
  playthrough: Play no longer crashes at the point it previously died. Committed `ProjectSettings.asset`.
  `-force-d3d11` is no longer needed.

**Verified via computer-use Play mode:** editor on DX11, Play runs without the crash.

**Next session starts with:** the (now-unblocked) deployment placement playthrough — draft a party → battle →
drop a unit on a player cell; plus the branching-map play-test.

## 2026-06-21 — deployment placement fix + enemy stat inspection
**Goal this session:** placing a unit on a cell didn't work (HUD covered the player-zone cells); also add
clicking an enemy to see its stats.

**Done (committed on `claude/deployment-grid`; 162/162 EditMode green):**
- **Root cause:** `DeploymentTray.Update` drops taps where `EventSystem.IsPointerOverGameObject()` is true;
  the `DeploymentHUD` is a full-width 230px bottom bar and the camera framed the whole board near-top-down,
  so the player zone (near rows) sat at the bottom under the HUD → taps dropped.
- **FIX 1 (camera/UI move):** `DeploymentCameraController.bottomViewportInset` (default 0.22) frames the
  board into the upper screen above a clear bottom band; added `framingOffset` nudge. Disabled the legacy
  `DeploymentView` (redundant 2nd click handler, no-ops under spawn-on-place).
- **FIX 2 (enemy inspection):** `DeploymentTray` — tapping an enemy-zone cell (`InEnemyZone`) finds the
  `Team.Enemy` `UnitRoot` there and shows the shared `UnitInspectionPanel` (real `UnitStatusController`
  stats), read-only, returns without placing. Instantiated `UnitInspectionPanel.prefab` as
  `EnemyInspectionPanel` (top-right, clear of the player zone) in `Test_M3_Battle` + wired the ref.

**Compiled clean:** yes. **EditMode 162/162.**

**Needs human verification (Play Mode, `-force-d3d11`):** see PROJECT_STATE — place a unit on a player cell;
click an enemy to see stats without blocking placement; tune `bottomViewportInset`/`framingOffset` + panel
RectTransform if needed.

**Decisions:** ADR-014 (frame board above HUD; collider-free enemy-zone-cell inspection; DeploymentView retired).

**Next session starts with:** play-test the deployment placement fix + enemy inspection (TASKS top item).

## 2026-06-21 — branching run map (Slay-the-Spire: seeded, infinite, choice-based)
**Goal this session:** replace the linear "advance" route with a visual branching node map — pick a
start, follow edges upward, infinite segments stitch on, run ends only on loss.

**Done (committed on `claude/deployment-grid`; 162/162 EditMode green):**
- **Model/gen** (`Run/Map/`): `MapNode` +Row/Column/Edges; `RunMap` graph helpers; new `MapGenConfig`
  (rows/segment, nodesPerRow, pathCount, type weights, rules); `MapGenerator.GenerateInitial`/`AppendSegment`
  — seeded bottom-to-top path-walk edges + reachability/outgoing repair + rule/weight types + segment
  stitching. 8 invariant tests (reachability, outgoing, bottom=Combat, top=Boss, no adjacent Rests, range,
  determinism, stitch). Legacy linear `Generate` kept only to ease the migration.
- **Run state/flow:** `RunState` graph-based (`CurrentNodeId`/-1, depth=`CurrentFloor`, `Seed`,
  `SegmentIndex`, `TravelTo`/`ReachableNodeIds`/`MarkCurrentCleared`/`AppendNextSegment`/`DifficultyMultiplier`);
  DTOs v2 (edges/row/col + seed + node id; pre-v2 saves discarded). `RunController` builds a seeded first
  segment, `TravelToNode` for the map UI, stitches on top-row clear, ends only on loss (points by depth).
  `BattleNodeReturn` marks cleared (no auto-advance); `RunBattleSetup` scales enemies by depth; `RunHud`
  shows depth/segment; `RunEndPanel` shows depth. Rewrote `RunStateTests`/`RunStatePersistenceTests`.
- **UI:** `UI/Map/MapView` (ScrollRect of colour/state-coded nodes + edge lines, click→TravelToNode,
  auto-scroll, foreground) + `MapNodeView`; procedural node/edge prefabs (`Assets/Prefabs/Map/`).

**Compiled clean:** yes. **EditMode 162/162.**

**Mid-session blocker:** Unity Editor closed/crashed during planning; I kept edits additive + paused the
coupled migration until it was relaunched, then did it with test feedback.

**Needs human verification (Play Mode):** scene assembly + test — see PROJECT_STATE (add a Scroll View to
the map panel, wire `MapView` + the 2 prefabs, assign a Label font, set `RunController` MapGenConfig/seed,
then run a branching climb). Launch `-force-d3d11`.

**Decisions:** ADR-013 (branching/infinite seeded map; graph run-state; loss-only end; MapView UI).

**Next session starts with:** assemble + play-test the branching map (TASKS top item).

## 2026-06-21 — customization v2 (front/clickable + starter items + item meshes on sockets)
**Goal this session:** fix three customization-screen gaps — (1) not in front / hard to click,
(2) no items to test with, (3) equipping changes stats but shows nothing on the unit.

**Done (all committed on `claude/deployment-grid`; 160/160 EditMode green):**
- **Front + clickable (CHANGE 1, code):** `CustomizationScreen.EnsureForeground` + `CustomizationLauncher.EnsureForeground`
  add an overriding high-sort `Canvas` (100 / 90) + `GraphicRaycaster` + `CanvasGroup` to the panel/picker on
  open — guarantees foreground + raycasts above other map UI regardless of scene wiring (both map canvases were
  Screen Space–Overlay at sort 0). Equip became a **toggle** with a green selected highlight.
- **Starter items (CHANGE 2):** 4 `Equipment_SO` (sword/shield/helm/plate across slots) added to
  `EquipmentCatalog.asset` (now 6) + a serialized `starterItems` union on the screen.
- **Item meshes (CHANGE 3):** `Equipment_SO` gained `attachSocketName` + `visualPrefab`; new
  `UnitEquipmentVisuals` (on `Unit_Sample_Prefab`) holds named sockets (RightHand/LeftHand/Helmet/Chest as
  root empties) and diff-rebuilds attached meshes on `OnStatsChanged` — live in preview AND on combat units
  (UnitFactory.Equip fires the event). Placeholder `EquipVisual_Cube` prefab as the visual.

**Compiled clean:** yes. **EditMode 160/160.**

**Needs human verification (Play Mode, `-force-d3d11`):** see PROJECT_STATE — open Customize → in front +
clickable → 4 items listed → click ⇒ cube on socket + highlight → Close → combat unit shows the cube. Swap
the placeholder cube for real meshes.

**Decisions:** ADR-012 (item visuals via named sockets + UnitEquipmentVisuals; customization foreground in code).

**Next session starts with:** re-bake NavMesh (deployment v2) + Play-mode test both v2 passes (TASKS top items).

## 2026-06-21 — deployment v2 (spawn-on-place + split-zone board + grid-fit camera)
**Goal this session:** fix two deployment problems — (1) you couldn't *see* units as you placed them
(placement was data-only), and (2) the board was tiny with no enemy zone (sides overlapped).

**Done (all committed on `claude/deployment-grid`; 160/160 EditMode green):**
- Camera framing fix (prior turn): deployment pose was pitched forward past the deploy zone; re-aimed
  near-top-down. Then generalised into auto-framing (below).
- **Spawn-on-place:** `BattlePartySpawner` gained `SpawnOrMoveAt/Despawn/DespawnAll` (instance dict +
  `DeployedUnits` container, cached DB); `DeploymentTray` calls them on place/bench/clear. Dropped the
  deferred OnConfirmed spawn — placed instances (idle in PreBattle) become the combat units on Assemble,
  no double-spawn. No-deployment fallback unchanged.
- **Bigger split-zone board:** `DeploymentGridConfig` cellSize 1.5→3.5; added `enemyRowMin/Max` (6–8) +
  `InEnemyZone`. Renderer + gizmo colour the enemy zone. Scene: cellSize 3.5 on both grid configs, enemy
  moved to (10.5,0,24.5), Plane enlarged (scale 4 @ (10.5,0,14)), wider camera bounds/zoom.
- **Camera auto-frame:** `DeploymentCameraController` computes the framing pose from the grid (fits
  board width/depth for the aspect, near-top-down tilt); falls back to the manual pose if no grid.
- Tests: zone-disjoint + cellSize tests; fixed the default-cellSize assertion in DeploymentManagerTests.

**Compiled clean:** yes. **EditMode 160/160.**

**Needs human verification (Play Mode):** see PROJECT_STATE. **Re-bake the NavMesh** (Plane enlarged)
first; then with a drafted run, place units (they should appear at the cell), Assemble → those units
fight; enemy on the far side; camera auto-frames. Launch with `-force-d3d11`.

**Decisions:** ADR-011 (deployment spawn-on-place + split-zone board + grid-fit camera; supersedes ADR-010 tokens).

**Next session starts with:** re-bake the NavMesh, then Play-mode test deployment v2 (TASKS top items).

## 2026-06-21 — deployment phase (7×9 grid, gate, tray HUD, camera)
**Goal this session:** add a Deployment Phase before combat — a 7×9 grid with a visible tray HUD
to place units, camera pulled back to frame the board, confirm-to-start.

**Done (all committed on `claude/deployment-grid`; 158/158 EditMode green):**
- Grid → 7×9 (columns = X/width, rows = Z/depth); updated the scene's DeploymentManager +
  BattlePartySpawner configs. New `DeploymentGizmos` draws the grid + player zone in the Scene view.
- `DeploymentPhaseController` gates `BattleStateManager.StartBattle` (DeploymentRequired/Confirmed);
  `Confirm()` (Assemble) spawns + starts. `BattlePartySpawner` defers spawn to `OnConfirmed`
  (place-then-spawn); falls back to immediate spawn when no deployment phase. Late-spawned units
  still register via `UnitRoot.OnEnable → registry.OnUnitRegistered` (verified).
- `DeploymentManager` token placement (`PlaceToken/RemoveToken/ClearAll`). `DeploymentTray` HUD:
  bench from `RunSession.Party`, tap-select → tap-cell to place, tap a placed cell to bench it,
  Assemble/Clear; mirrors `RunState.Placements`; refreshes the grid renderer for valid/invalid colours.
- `DeploymentCameraController` auto-frames the board on deploy, restores the battle pose on Assemble.
- Wired in `Test_M3_Battle`: `DeploymentPhase` + `DeploymentHUD` (bottom bench bar + Assemble/Clear +
  selection label) + the gizmo component. HUD bar + buttons render (confirmed in the Simulator).

**Compiled clean:** yes. **EditMode 158/158** (+gate, +token, +7×9 fix tests).

**Needs human verification (Play Mode):** see PROJECT_STATE. Core loop: load a combat node with a
drafted party → HUD bench populates → place units → Assemble → units spawn at placed cells → combat
runs. Camera deployment pose + grid origin/zone need tuning to the arena.

**Decisions:** ADR-010 (deployment phase: place-then-spawn + confirm gate + 7×9 + camera auto-frame).

**Next session starts with:** Play-mode test of the full deployment loop with a drafted run (TASKS top item).

## 2026-06-21 — continuity setup + battle/customization UI
**Goal this session:** ship the battle/customization UI feature set, then stand up a
cross-session continuity system (this doc system).

**Done:**
- Persistence foundation — `UnitDTO.Equipment` + `IEquipmentDatabase`/`EquipmentDatabase`/
  `EquipmentCatalog_SO`; `UnitFactory` applies/captures equipment; `RunStateDTO`+`MapNodeDTO`+
  `UnitPlacementDTO`, `RunStore` (run.json), `RunState.ToDTO/FromDTO`, `RunSession` save/load.
- `UnitStatusController.OnStatsChanged` event (fires on equip/unequip/synergy).
- `DeploymentCameraController` (UI/Camera) — pan/zoom/clamp, PreBattle-gated; on battle Main Camera.
- `UnitInspectionPanel` (UI/Inspection) built in Test_M3_Battle, extracted to
  `Assets/Prefabs/UnitInspectionPanel.prefab`.
- Deployment grid: `Combat/Deployment/` model (GridCoord/Config/Grid/CellState) +
  `DeploymentManager` (AutoArrange, cell-based) + `UI/Deployment/` view + `DeploymentGridRenderer`
  + `DeploymentCell.prefab`; `BattlePartySpawner` spawn-then-arrange.
- `CustomizationScreen` (UI/Customization) built in Test_M7_Map + `EquipButton.prefab` +
  `EquipmentCatalog.asset`.
- Fixed the pre-existing EditMode baseline regression (UnitHealthController lazy init;
  BattleStatsAggregator id fallback).

**Compiled clean:** yes. **EditMode tests: 155/155 green.**

**Needs human verification in editor (Play Mode):**
- Deployment: click unit cell → inspection shows; click empty cell → unit moves.
- Customization screen: not yet triggered (no `Show` caller) and needs a run session to
  populate; verify equip → live stats → persist once wired.
- Camera feel/bounds; grid config alignment to arena; battle-end "New Text" placeholders.

**Decisions:** ADR-003…009 added (uGUI, mobile+desktop, armor=stats, run-scoped persistence,
no StatCalculator, spawn-then-arrange/cell-based deployment, stacked feature branches).

**Next session starts with:** wire the between-rounds trigger that calls
`CustomizationScreen.Show(unitId)` (TASKS.md top item).

---
---

<!-- ============ ENTRY TEMPLATE — copy this block ============ -->
<!--
## YYYY-MM-DD — session <n>
**Goal this session:** (one line)

**Done:**
- (file touched) — (what changed, why)

**Compiled clean:** yes / no — (notes if no)

**Needs human verification in editor:**
- (visual/gameplay things Claude couldn't confirm)

**Decisions:** (link ADR id if added) / none

**Next session starts with:** (the exact next step — must match TASKS.md top item)
-->
