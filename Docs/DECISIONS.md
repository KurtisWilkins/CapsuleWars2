# Decisions — CapsuleWars2

> Short, append-only record of choices that shape the project, so a future
> session doesn't re-litigate them. One entry per decision.

### ADR-001 — Art style: consistent AI-friendly pixel art
**Decided:** adopt a single coherent pixel-art style rather than style-matching
across many character race heads. (Shovel Knight-style pixel art was evaluated
as a strong reference.)
**Why:** consistency + far cheaper to generate/maintain solo than per-race
bespoke matching.

### ADR-002 — Live editing via CoplayDev Unity MCP bridge
**Decided:** Claude Code edits the Unity project through the CoplayDev MCP bridge
(~v9.7.1) connected to the editor.
**Why:** lets Claude read/modify scripts and assets in-place instead of
copy-pasting.
**Implication:** Claude can't see Play Mode results — gameplay/visual changes
must be human-verified (tracked in PROJECT_STATE.md). The bridge also drops on
every domain reload and cycles ports; reconnect and continue.

### ADR-003 — UI is uGUI (not UI Toolkit)
**Decided:** build all new UI (deployment grid, inspection, customization) with
legacy uGUI + the existing theme system (`UIThemePalette`/`UIThemeApplier`).
**Why:** matches all existing UI; lowest friction. `manage_ui` (UXML/USS) is for
UI Toolkit and is deliberately not used.

### ADR-004 — Target mobile + desktop
**Decided:** design input/layout for both. New input uses the Input System
(`activeInputHandler = 1`; legacy `UnityEngine.Input` is off) with mouse +
keyboard + touch (drag/tap, pinch). Build targets include Android/iOS.

### ADR-005 — Armor carries stats; body parts are cosmetic
**Decided:** `Equipment_SO` (armor/weapons) carries `StatBuffs`; `BodyPart_SO`
parts stay visual-only via `UnitCustomization`.
**Why:** keeps the stat/balancing surface small; matches the existing model.

### ADR-006 — Run-scoped persistence (EquipmentDTO + RunStateDTO)
**Decided:** equipment + the in-run party + deployment placements persist at the
run level via `RunStore` (`run.json`), so a mid-run loadout survives an app
restart. Equipment ids resolve through an `IEquipmentDatabase` at spawn time.
**Why:** the realistic "persist through JSON" requirement for between-rounds
customization. `RunStateDTO` stores `NodeType`/coords as ints so the Persistence
assembly never references the Run/Combat assemblies.

### ADR-007 — No StatCalculator class; stats live in UnitStatusController
**Decided:** there is **no** separate `StatCalculator`. Final stats are computed
by `UnitStatusController` read-only getters (`MaxHp/Atk/Def/Speed`) that fold
base + equipment(×rarity) + status effects + class-synergy buffs.
**Why:** the computation was already cleanly encapsulated and read-only, so the
inspection/customization UI consumes it directly — no extraction needed. (Docs
or tasks that mention "StatCalculator" mean this controller.) A lightweight
`OnStatsChanged` event was added so UI can refresh live.

### ADR-008 — Deployment: spawn-then-arrange + cell-based selection
**Decided:** units spawn first (`BattlePartySpawner`), then the player arranges
them on the grid during `PreBattle`; placements persist battle-to-battle
(`RunState.Placements`). Selection is **cell-based** (raycast ground/tiles → look
up the cell's occupant) because the unit prefabs have **no colliders**.
`DeploymentManager.AutoArrange()` seeds every player unit onto a free deploy-zone
cell so it's always selectable. The 9×7 grid is a placement tool only — combat
movement stays NavMesh-based.

### ADR-009 — Feature work on stacked per-slice branches
**Decided:** the battle/customization features were built as separate branches
stacked off `claude/unit-factory`: equipment-persistence → deploy-camera →
unit-inspection → deployment-grid. The pre-existing EditMode baseline fix is its
own commit (`450d3f3`).
**Why:** keeps each slice independently reviewable; `unit-factory` isn't on
`main` yet, so stacking avoids an unprompted merge.

### ADR-010 — Deployment Phase: place-then-spawn, confirm-gated, 7×9
**Decided:** a pre-combat Deployment Phase on a **7×9** grid (columns = X/width, rows = Z/depth;
player zone = near rows). Model = **place-then-spawn**: during deployment the field has no player
units — the player places party **tokens** from a bench (`DeploymentTray`) onto cells, and on
**Assemble** the party spawns at the placed cells (`BattlePartySpawner` deferred to
`DeploymentPhaseController.OnConfirmed`). Combat is **gated**: `BattleStateManager.StartBattle`
won't reach Active while `DeploymentRequired && !DeploymentConfirmed`. The camera auto-frames the
board on entry and restores the battle pose on Assemble.
**Why:** matches the "tray of available units → units spawn where I placed them" intent, and reuses
the existing `RunState.Placements` + spawner. Late-spawned units register fine via
`UnitRoot.OnEnable → registry.OnUnitRegistered`, so deferring spawn is safe.
**Implication / fallback:** if no `DeploymentPhaseController` is in the scene (or no run party),
`BattlePartySpawner` spawns immediately as before — existing combat is unaffected. Gate flags
default off. Selection is cell-based (units have no colliders).

### ADR-011 — Deployment: spawn-on-place + bigger split-zone board (supersedes ADR-010 token model)
**Decided:** placing a unit now **spawns the real unit instance at the cell** (visible during
setup), instead of placing a data-only token. `DeploymentTray` drives
`BattlePartySpawner.SpawnOrMoveAt/Despawn/DespawnAll`; the placed instances ARE the combat units
(spawned during PreBattle they stay idle — all combat controllers gate on `Phase == Active` — and
simply join combat on Assemble, **no deferred double-spawn**). The board is enlarged (`cellSize`
1.5→**3.5**) and split into a near **player zone** (rows 0–2), a neutral middle (3–5), and a far
**enemy zone** (rows 6–8, new `enemyRowMin/Max` + `InEnemyZone`). The deployment camera **auto-computes**
its framing from the grid (fits board width/depth for the current aspect), so it always frames the
board and follows cellSize/row changes.
**Why:** the player asked to *see* units as they deploy and for clearly separated sides; the token
model showed neither. Spawning during PreBattle gives a live preview for free.
**Implication:** units left on the bench aren't deployed (don't enter combat). The enlarged arena
needs the ground Plane enlarged (done) and the **NavMesh re-baked** for combat movement. The
no-deployment-phase fallback (immediate `SpawnParty`) is unchanged. Enemies are still scene-placed
(now repositioned into the enemy zone); a runtime enemy-zone placer is a follow-up if they become dynamic.

### ADR-012 — Equipment visuals via named sockets + UnitEquipmentVisuals; customization brought to front in code
**Decided:** equipped items render as child objects on **named sockets**. `Equipment_SO` gains
`attachSocketName` + optional `visualPrefab` (falls back to the existing `visualMesh`/`visualMaterials`).
New `UnitEquipmentVisuals` (Units layer, on the unit prefab) holds a serialized `name→Transform` socket
list and **diff-rebuilds** attached visuals on `UnitStatusController.OnStatsChanged`. Sockets are empties
**under the unit root** (not animated bones) for now. The customization screen + launcher **guarantee
foreground in code** (`EnsureForeground`: add a nested `Canvas` with `overrideSorting` + high
`sortingOrder` + `GraphicRaycaster` + `CanvasGroup` on open) rather than relying on scene wiring.
**Why:** the player wanted to *see* gear on the unit live and in combat, and the panel reliably on top
and clickable. Because `UnitFactory.ApplyEquipment` calls `Status.Equip` (→ `OnStatsChanged`), the same
component makes visuals appear automatically on deployment/combat units — no spawner changes.
**Implication:** root-relative sockets don't follow limb animation (re-parent under bones later if needed);
item meshes are placeholder cubes (`EquipVisual_Cube`) to swap for real art. Starter items live in the
`EquipmentCatalog` (the screen also unions a serialized `starterItems`); a persistent run-scoped inventory
is still a follow-up.

### ADR-013 — Branching, seeded, infinite run map (Slay-the-Spire style); runs end only on loss
**Decided:** the linear `RunMap`/`MapGenerator` is replaced by a **branching graph** generated one
**segment** at a time, seeded per run for reproducibility. `MapNode` gains Row/Column/Edges;
`MapGenerator.GenerateInitial`/`AppendSegment` walk bottom-to-top paths (adjacency-biased) + repair
reachability/outgoing edges + assign types by rules (bottom=Combat, top=single Boss gate, a Rest near
the top, no adjacent Rests) and weights (`MapGenConfig`). `RunState` tracks graph position
(`CurrentNodeId`), depth (`CurrentFloor`), `Seed`, `SegmentIndex`, and a `DifficultyMultiplier`;
`TravelTo`/`ReachableNodeIds` enforce edge-following choice. The player picks any bottom node to start,
then any node connected by an outgoing edge. Clearing the top Boss **stitches a new segment** on
(`AppendNextSegment`), so the climb is **infinite** — the run ends **only on loss** (the old
boss-win/recruit completion path is dormant; unlock points are awarded by depth on loss). The visual
map is `UI/Map/MapView` (+`MapNodeView`): a ScrollRect of colour-coded clickable nodes + edge lines,
data-driven off `RunState.Map`, reusing the existing encounter entry point (`RunController.EnterCurrentNode`).
**Why:** the requested player-driven branching progression; segment-at-a-time generation makes "infinite"
natural and keeps generation pure/testable (8 invariant tests). Persistence bumped to SaveVersion 2
(pre-v2 linear saves are discarded on load).
**Implication:** node/edge prefabs + the ScrollRect container are assembled/wired in `Test_M7_Map` per the
editor checklist (Claude can't see Play mode). Node visuals are procedural placeholders (coloured squares
+ a letter). A persistent run-scoped item inventory + a win/recruit path for infinite runs remain follow-ups.

### ADR-014 — Deployment click path: frame board above the HUD; enemy inspection via enemy-zone cell taps
**Decided:** the deployment placement bug (HUD bar covering the player-zone cells, so taps were dropped by
`DeploymentTray`'s `IsPointerOverGameObject` gate) is fixed by **moving the view, not making UI
click-through**: `DeploymentCameraController.bottomViewportInset` frames the board into the upper screen
above a clear bottom band for the HUD (+ a `framingOffset` nudge). The legacy `DeploymentView` (a second,
now-redundant click handler that no-ops under spawn-on-place) is **disabled**. Enemy stat inspection is
**cell-based, collider-free**: tapping an enemy-zone cell (`InEnemyZone`) finds the `Team.Enemy` `UnitRoot`
there and shows the shared `UnitInspectionPanel` (real `UnitStatusController` stats), anchored top-right so
it never overlaps the player zone — read-only, no placement effect.
**Why:** the user preferred a camera/UI move over click-through; cell-based enemy selection reuses the
existing ground raycast + `InEnemyZone` and respects ADR-008 (units have no colliders). Keeps a single
active click handler (`DeploymentTray`).
**Implication:** `bottomViewportInset`/`framingOffset` + the `EnemyInspectionPanel` RectTransform are
serialized/scene-tunable; verify in Play mode.

### ADR-015 — Asset creation pipeline + queue (editor-only, assisted-manual with an API seam)
**Decided:** a repeatable workflow to design body parts / weapons / armor for the capsule soldiers, managed as
a **visible queue with explicit stages** (`Requested → ConceptsReady → ConceptChosen → ImagePrompt →
ImageChosen → MeshyPrompt → ModelImported → Reviewed → Categorized → Described → Done`). Built **editor-only**
(`Assets/Scripts/Editor/AssetPipeline/`, `CapsuleWars.Editor` asmdef): an `AssetRequest` ScriptableObject (one
per asset, holds every stage's artifact; persisted under the `Assets/Editor/AssetPipeline/Requests/` Editor
folder so the queue survives sessions and never ships in a build); an **Asset Pipeline** `EditorWindow`
(Tools ▸ CapsuleWars ▸ Asset Pipeline) listing requests grouped by stage with add/advance/rollback,
copy-prompt buttons, paste-result object fields, category/slot/socket, **Create / Wire item**, and an editable
description; `PromptTemplates` (Rayman/AssetHunts style locked into the concept/Grok/Meshy prompts); and
`AssetPipelineImporter` which makes a prefab under `Assets/Generated/Meshy/{slot}/` and creates an
`Equipment_SO` (Weapon = hand slot + WeaponClass; Armor = other slots) or `BodyPart_SO` (cosmetic) — private
fields set via `SerializedObject` — then adds it to `EquipmentCatalog_SO` / `PartCatalog_SO`.
**Why:** the player wanted a managed, stage-by-stage way to spin up unique art that **drops straight into the
existing item + attach-point system** (ADR-005/012). No Grok/Meshy/Anthropic keys are configured, so the
pipeline is **assisted-manual** (Claude writes the prompts + description into the `AssetRequest` over the MCP
bridge; the user runs Grok/Meshy and pastes results back). An `IGenerationService` + git-ignored
`SecretsConfig.json` (`Tools/Editor/`, env-var fallback) seam makes "Generate" buttons light up automatically
if a key is added later — realizing Docs/16_AssetGeneration.md's design.
**Implication:** the AI **description lives on the `AssetRequest`** (runtime `Equipment_SO`/`BodyPart_SO` keep
their I2 `descTermKey` pattern — zero runtime change); weapons still need a `WeaponClass_SO` + stat buffs
assigned on the created asset; generated models live in `Assets/Generated/` (LFS). End-to-end run on a real
imported model is human-verified (Claude can't see the editor UI / Play mode).

### ADR-016 — Shared Grok art-style system (StyleProfile + PartTemplates), live Grok API
**Decided:** make art-style consistency **structural, not manual**. A single `StyleProfile` SO
(`Assets/Scripts/Editor/AssetPipeline/Style/`) is the source of truth for the cartoony look (base spine,
finish rules, avoid list, fixed `aspect_ratio`/`resolution`, opt-in reference image). `PartTemplate` SOs hold
ONLY each part type's criteria + floating-limb cut (Helmet/RightHand/LeftHand/Foot/Torso/Weapon/Armor/Generic).
`StyleComposer` always builds the Grok prompt as **base + part criteria + concept + finish + avoid**, resolving
the template from the request's category+slot — so nothing re-types the style and **editing the StyleProfile
restyles every future generation**. `StyleSetupTool` seeds 1 profile + 8 templates (re-run safe). The Grok call
(`GrokImageService`) now sends `aspect_ratio`/`resolution` and has an opt-in `/v1/images/edits` reference-image
path; `GenerationActions` composes via `StyleComposer`, passes the framing, sets the Meshy prompt after the
image saves, and runs a **sequential batch** ("Generate all pending images"). Default image model
`grok-imagine-image-quality`.
**Why:** every generated part must read as one game; a shared, referenced style + grayscale/isolated/flat-bg
rules + fixed framing make parts Meshy-ready and coherent. **xAI has no `seed` param (June 2026)**, so
seed-reproducibility isn't possible — consistency comes from the shared prompt + framing. The reference-image
edit path is best-effort (edits modify the source image) and off by default.
**Implication:** the StyleProfile + PartTemplate `.asset`s live under `Assets/Editor/AssetPipeline/Style/`
(editor-only, not shipped) and are the things the designer tunes; re-running the seeder won't overwrite tuned
assets. Verified live: composed prompt = base+template+concept+finish+avoid, Grok generated with the new params,
Meshy prompt auto-populated. Keys stay in the git-ignored `SecretsConfig.json`.

### ADR-017 — Image mirror/flip for paired parts (flip the 2D image, not the mesh)
**Decided:** sided parts (right/left hand, right/left foot) get a one-click **"Mirror to opposite side"** in the
Asset Pipeline window. It horizontally flips the approved PNG and finds-or-creates a **linked opposite-side
`AssetRequest`** (`mirrorOf`), ready for Meshy. Flip is done at the **2D image stage on purpose** — mirroring the
image gives Meshy a clean opposite-side 3D model, whereas negative-scaling the mesh in Unity flips normals and
breaks lighting. New `MirrorUtil` (sidedness via **slot**, since both feet map to one `PartType`) +
`MirrorAction` (flip = `Texture2D.LoadImage` → reverse each row's columns → `EncodeToPNG`; pure horizontal,
preserves resolution/grayscale/background). The mirror request: opposite slot, side word swapped (whole-word
regex), flipped image attached, `stage = ImageChosen`, `meshyPrompt` regenerated via `StyleComposer`,
`grokImagePrompt` cleared (no Grok regen). `AssetRequest` gains `mirrorOf` + `asymmetric`.
**Why:** avoids regenerating an identical opposite part + keeps the pair visually identical. Idempotent
(deterministic `{id}_{Side}` id → re-runs update the same asset/PNG, never pile up) and the original is never
overwritten. A symmetry **warning** fires before flipping (modal on the window button; non-modal console warning
+ refusal on the `asymmetric` flag for the MenuItem) so nothing flips silently.
**Implication:** the window button is the interactive path; a non-modal `[MenuItem]`
"Tools ▸ CapsuleWars ▸ Mirror Selected Request" exists for automation/bridge testing. Editor-only; no
runtime/build impact. Verified via the bridge: clean horizontal mirror + correct linked left-hand request.

### ADR-018 — Archive / Reject (lifecycle) for the asset pipeline queue
**Decided:** add a `Lifecycle` enum (Active/Archived/Rejected) to `AssetRequest`, **separate from `PipelineStage`**
(don't overload Stage). The pipeline window filters by a `_view` (default Active) shown as a view bar with live
counts, then groups by Stage as before. Per-request **Archive**, **Reject** (any stage), **Restore to Active**,
and a **Complete & Archive** shortcut on Done items; archived/rejected items show an editable reason + a stamped
`lifecycleDate`. Stage is never changed by these, so **Restore returns the request to its prior stage**. Real
**Delete** stays the only destructive action (confirm dialog).
**Why:** done/dead requests clutter the working queue; the user wants them out of the active view but recoverable,
without deleting anything. A separate Lifecycle field keeps Stage meaningful + Restore trivial.
**Implication:** lifecycle changes are pure field flips on the editor-only `AssetRequest` — **no `AssetDatabase.DeleteAsset`
and no touch to `createdItem`/prefab/catalog**, so the produced game asset is never deleted/unwired by archiving.
Mirror pairs are **not** auto-paired (archiving one side leaves the other; the `mirrorOf` link stays visible) —
chosen so each side can be kept/dropped independently. No folder move (the window finds requests by type). Verified
via computer-use: archive → leaves Active / shows under Archived with timestamp / item intact → restore → back at
its stage.

### ADR-019 — Equipment stats move to runtime instances (Definition + Instance)
**Decided:** stats no longer live on the equipment ScriptableObject. `Equipment_SO` becomes the **Definition**
(identity: id, slot, weapon class, element, icon, mesh/prefab + attach socket); a new `EquipmentInstance`
(`Data.Equipment`, `[Serializable]`) holds the runtime stats — `definition` ref + `modifiers (List<StatBuff>)` +
`displayName` + `tier` + `seed`. The unit equips an **instance**; `UnitStatusController.ComputeMods` sums the
instance's modifiers through the *same* `SumBuffs` path as before, so combat is unchanged. `EquipmentRoller` +
`EquipmentRollConfig` (data-driven stat pool, per-tier rules, name suffixes) build instances either explicitly or
by a **seeded, deterministic roll**, generating a name from the dominant stat ("… of Health"). One definition
now backs many stat variants.
**Why:** baking stats on the SO meant one asset per variant; moving stats to a saved instance makes equipment
reusable + roguelite-friendly (roll loot at runtime) without new assets.
**Implication / compat:** `Equipment_SO.statBuffs`/`rarity` are kept as **legacy default stats** +
`BuildDefaultModifiers()`; a compat `UnitStatusController.Equip(Equipment_SO)` overload and
`UnitEquipmentDTO.ToInstance` build a *default instance* from them when no rolled modifiers exist (old saves,
starter items) — so nothing silently loses stats. `UnitEquipmentDTO` gained `modifiers`/`displayName`/`tier`/`seed`
(additive; SaveVersion stays 1). `EquippedItem.item` is now a getter (`=> instance.definition`) so
`UnitEquipmentVisuals`/inspection/customization read the definition (mesh/labels) unchanged. The class stays named
`Equipment_SO` (it *is* the Definition) to avoid churning ~15 files + the asset pipeline + every `.asset`. The
asset pipeline is untouched (Definitions authored/imported as before; the importer no longer needs to set stats).
Verified by EditMode test: two instances of one definition → different stats; roller determinism + name + tier. 166/166 green.

### ADR-020 — Trunk-based development on `main` (supersedes ADR-009)
**Decided:** drop the stacked per-slice branch model. `main` is the trunk; work happens on `main` directly or on
short-lived feature branches that merge straight back. **Why:** ADR-009's stack only pays off with PR review,
which a solo project doesn't have. In practice the stack had collapsed anyway — all six branches
(`unit-factory`, `equipment-persistence`, `deploy-camera`, `unit-inspection`, `capsule-wars-setup-pBoDq`,
`deployment-grid`) ended up fully contained in `deployment-grid` with zero unique commits, while `main` sat 134
commits behind and untouched. The stack was pure clutter + doc drift.
**Action taken (2026-06-22 cleanup session):** fast-forwarded `main` → `deployment-grid` (a clean FF — main was a
strict ancestor, no merge commit, no work lost), pushed `main`, tagged the pre-merge tip `pre-trunk-main`
(`852a520`, also pushed) as a rollback point, and deleted the 5 contained local branches. `claude/deployment-grid`
(local + remote) is retained as a synced pointer for now and can be pruned later.
**Implication:** `main` now carries everything through ADR-019, including gameplay that is **not yet Play-verified**
(deployment loop, branching map, customization v2, mirror equip, style consistency, mesh scale, NavMesh re-bake —
tracked in PROJECT_STATE "Needs human verification"). `main` has no release semantics here; that verification list
is the gate before any build. The editor asset-pipeline tooling can't ship (Editor-only). 166/166 EditMode green.

### ADR-021 — Customization is a paper-doll; one screen drives gear (stats) AND body parts (cosmetic) (extends ADR-005/012)
**Decided:** replace the customization list-of-buttons with a **paper-doll**: a centered live preview flanked by
framed equipment slots, a cosmetic body-slot row, an HP/DAMAGE/ARMOR footer + a Stats button (reusing
`UnitInspectionPanel`), and a scrollable bag with **Gear / Body** tabs. Interaction: **tap** a bag item
auto-equips to ITS own slot (the slot is read from the item, never chosen); **drag-and-drop** equips (wrong slot
rejects with a red flash; dropping on the doll background auto-routes); **tap a filled slot** unequips. Slots and
bag items are **generated at runtime** from `EquipmentSlot` + the preview's mounted `PartSlot`s, so the scene only
provides empty layout containers. **Why:** the list UI didn't scale or communicate slots; a paper-doll is the
genre-standard, reads at a glance, and works on mobile (tap) + desktop (drag) through the uGUI EventSystem
(`InputSystemUIInputModule`). Reuses the equip backend entirely (`UnitStatusController.Equip` for gear,
`UnitCustomization.ApplyParts` for parts) — no new stat math (ADR-007).
**New persistence:** body-part edits now persist. `UnitCustomization` records `AppliedParts`/`AppliedPalette` +
exposes `MountedSlots`; the screen's `Capture()` writes `dto.Parts` — but **only when a part was actually edited**
(`partsDirty`), so a gear-only session doesn't freeze a definition-driven unit into explicit parts. Closes the
old "persist body-part edits" backlog item. Load already round-trips `dto.Parts` (`UnitFactory.FromDTO`).
**Widgets:** `PaperDollSlot`, `BagItemWidget`, `PaperDollDropZone` (uGUI `IPointerClick`/`IDrop`/`IDrag`), each
self-building its visuals so they generate with no authored prefab; theme via `UIThemeApplier` + `UIThemePalette`
(no new art style). **Scope held:** no bag economy / currency / "add slots" (backlog); no run-scoped inventory yet
(bag = `EquipmentCatalog ∪ starterItems` and `PartCatalog`; equipped = highlighted, not consumed).
**Status:** code complete + **scene assembled in `Test_M7_Map` and Play-verified (2026-06-23)**. Built via a
re-runnable editor tool `PaperDollBuilder` (`Tools/Paper-Doll/Build In Open Scene`) that generates the containers
+ footer + bag + buttons and wires all 13 refs via `SerializedObject` (deterministic, not blind clicks).
Play-verified via computer-use: opens for a live unit; slots/body-slots/bag generate; live HP/DAMAGE/ARMOR;
**tap-equip, tap-unequip, and drag-and-drop (ghost + auto-route) all confirmed.** 169/169 EditMode green. Remaining
human checks: visual layout polish, wrong-slot drag reject, Stats button, body-part bag equip, persistence
round-trip. (`Docs/CHECKLIST_PaperDoll.md` retained as the assembly/verification reference.)

### ADR-022 — Battle camera: free pan/zoom during combat + computed 45° frame (revisits ADR-014)
**Decided:** the deployment/battle camera (`DeploymentCameraController` on `Test_M3_Battle`'s Main Camera) now
(1) **frames the board clear of the deployment HUD**, (2) **eases to a computed ~45° board view on Assemble**
instead of snapping to the authored scene pose, and (3) **allows pan + zoom during combat** (TFT-style), not just
deployment. **Why:** ADR-014 framed the board above the HUD with `bottomViewportInset`/`framingOffset`, but at
78° tilt the three player rows foreshortened into the bottom band (a 230px HUD that reaches ~0.32 of height on a
wide view under the 720×1280 / match-0.5 CanvasScaler) and `restrictToDeployment` locked the camera the instant
combat started, dropping the player at a bad angle with no control.
**Changes:** steeper deployment tilt (default 78→**84**) so rows spread vertically; `bottomViewportInset` 0.22→
**0.30**; `TryComputeBoardFraming` generalized to `(tilt, inset, fov)` so deployment and battle share it;
`FrameBattle` computes a frame at `battleTiltDegrees` (default **45**, `battleFov`, `computeBattleFromGrid`) with
the authored pose as fallback; new **`allowControlDuringBattle`** (default true) overrides `restrictToDeployment`
for the combat phase (control still off during the transition lerp); **zoom moves along the camera's view**
(`transform.forward`), clamped by height, instead of only changing `p.y` — so it dollies toward the board at any
pitch; world bounds widened (min −10,−30 / max 35,40) so the 45° pose + panning stay in range.
**Tuning aid (editor-only, never ships):** `#if UNITY_EDITOR` ContextMenu "Re-apply deployment/battle frame" +
F5/F6 hotkeys to reframe live after nudging a serialized knob — this is a feel task dialed by eye.
**Status:** code + scene done, **172/172 EditMode green** (`Clamp` now covered). Play-mode feel is human-gated
(see PROJECT_STATE "Needs human verification" for the knobs-per-symptom table). No combat logic touched; ADR-014's
framing intent stands, this tunes it and removes the battle lock.

### ADR-023 — Deployment layout persists + auto-restores between combats (extends ADR-006/011)
**Decided:** the player's unit placements carry across combats. `RunState.Placements` was already saved (written
by the deployment UI on place/bench, round-tripped to disk via the DTO), but in spawn-on-place deployment mode
nothing replayed it — every combat started with an empty board. `DeploymentTray.Start` now calls
`RestoreSavedPlacements()`: each saved placement for a CURRENT party member is re-placed via the normal path
(`DeploymentManager.PlaceToken` + `BattlePartySpawner.SpawnOrMoveAt`) and left off the bench; units with no saved
cell stay benched; placements for units no longer in the party are dropped. **Why:** re-deploying the same layout
every fight was tedious; auto-restoring the last layout — still fully editable via bench-tap / Clear — is the
convenient default. **Status:** code done, **172/172 EditMode green** (`2a0bede`); reuses tested primitives + the
existing placement round-trip test. Play-verify: deploy → fight → next combat auto-deploys the same layout; edits stick.

### ADR-024 — Per-cell terrain model generalizes the binary "blocked" flag (Slice A of the themed-encounter system)
**Decided:** replace the deployment grid's binary `blocked` set with a per-cell `TerrainType` —
`Passable` / `Impassable` (blocks placement AND pathing: rock/river/wall) / `Hazard` (placeable but harmful/avoid:
lava/trap). The model stays a **pure, serializable Combat.Deployment model with no scene deps** (matches
`DeploymentGrid`'s style), so it's fully EditMode-testable. `DeploymentGrid` gains
`SetTerrain/GetTerrain/IsImpassable/IsHazard` + a read-only `TerrainCells` (non-Passable cells only); the old
`SetBlocked/IsBlocked` stay as **thin compat wrappers over `Impassable`** so all existing callers + tests are
untouched. `IsDeployable = InPlayerZone && !Impassable && (config.allowPlaceOnHazard || !Hazard)`. `CellState`
gains `Hazard` (appended, so existing values keep their meaning); `GetState` reports `Blocked` for Impassable and
`Hazard` for hazards (both readable anywhere, like the old Blocked). A serializable `TerrainLayout`
(`List<TerrainCell>` + `ApplyTo(grid)`) is authored **inline on `DeploymentManager`** and stamped onto the grid in
`Awake` before the deployment UI builds.
**Decisions taken (recommended set):** Hazard is **placeable by default** (the harm is applied later in
combat/Slice C, not by this data layer) behind a `DeploymentGridConfig.allowPlaceOnHazard` flag (default true);
`SetBlocked/IsBlocked` kept as Impassable wrappers; terrain authored inline (a ScriptableObject is deferred to
Slice C generation); `CellState.Hazard` added.
**Why:** the binary flag can't express "walk-through-but-harmful" vs "solid wall", and the themed-encounter system
(below) needs a richer, generated obstacle layer. Generalizing in place — with compat wrappers — adds the model
without disturbing placement, the spawner, or the 172 existing tests.
**Seams left for later slices (NOT built):** **Slice B (theming)** reads `GetTerrain`/`TerrainCells` to map
`TerrainType → themed prop prefab/material` and to drive the **NavMesh carve** (drop `NavMeshObstacle` carving
boxes on Impassable cells at `config.CellToWorld`, no re-bake). The renderer got a minimal `hazardColor` case only.
**Slice C (encounter builder)** reads `GetTerrain`/`IsImpassable` + `config.InEnemyZone` to place a generated enemy
roster on Passable enemy-zone cells around the obstacles; `EncounterDefinition` (SO) = roster spec + a
`TerrainLayout` (or seeded generator) + placement strategy, and a new `EnemyEncounterSpawner` (mirror of
`BattlePartySpawner`) spawns them. Determinism from `RunState.Seed + nodeId` → the layout needn't be saved unless a
later design makes it non-deterministic.
**Status:** Slice A code done, **181/181 EditMode green** (+9 terrain tests). Pure logic — self-verified; the only
human-Play item is the future NavMesh carve visual (Slice B). See `Docs/18_ThemedEncounters.md`.

### ADR-025 — Runtime modular-block arena builder + ThemeBlockSet model (Slice B of the themed-encounter system)
**Decided:** the battle board is built at runtime from themed blocks by `ArenaBuilder` (`CapsuleWars.UI.Arena`),
consuming the Slice A terrain layer (ADR-024). One checkerboard floor tile per grid cell — aligned **1:1 with
`DeploymentGridConfig`** so every visual tile IS a deployment cell — plus a raised obstacle block on each
Impassable cell and a marker on each Hazard cell, all sized from `cellSize`. Everything is driven by a
`ThemeBlockSet` SO (`CapsuleWars.Data.Arena`) mapping roles {FloorA, FloorB, Obstacle, HazardMarker} →
{prefab, material, height}; a **null prefab falls back to a scaled primitive cube**, so the system works with
zero authored assets and a real kit (Kubikos / Meshy blocks) drops in later by assigning prefabs — no code change.
An `EncounterTheme` SO selects the block set per floor (forest/volcanic/etc. are just different sets). Pure
placement math (checkerboard parity, terrain→role, cell centers) lives in a testable static `ArenaLayout`.
**Checkerboard = deployment readability:** the floor is the BASE layer; the existing `DeploymentGridRenderer`
CellState tints are a translucent OVERLAY on the SAME cells (one shared grid — no second visual grid). Floor tiles
sit a hair proud of the legacy ground (`floorSurfaceY`, default 0.05) so the board reads with no z-fighting and
units (spawned at ~0) still sit on it.
**NavMesh (runtime bake, not carving):** after building geometry, `Build()` calls `Bake()` →
`NavMeshSurface.useGeometry = PhysicsColliders` then `BuildNavMesh()`. Units have no colliders (ADR-008) so they're
never baked into the mesh; the legacy Plane's MeshCollider is the walkable ground; obstacle blocks carry a
`NavMeshModifier(area = Not Walkable)` BoxCollider so their footprints carve out. Order matters: **bake once after
all blocks exist, before any unit moves**. Extends the scene's existing `NavMeshSurface` (the Plane), doesn't
duplicate it. The legacy Plane renderer is hidden by the builder (`groundPlaneRenderer`) while its collider stays
as the placement-raycast + NavMesh ground.
**Lifecycle:** build at encounter start (`Start`), `Teardown()` (destroy the `ArenaRoot`) on end — each battle
reloads the scene → clean rebuild, no leaks. Editor-only `#if UNITY_EDITOR` preview (ContextMenu + `Tools/Arena/*`
menu items: Build geometry / Bake / Clear) so the board is eyeball-able without Play; nothing debug ships.
**Scope (NOT built):** no procedural terrain generation (Slice C — the layout is the hand-authored
`DeploymentManager.Terrain`, seeded here with a demo: Impassable (2,4)(4,4)(3,5) + Hazard (3,3) in neutral rows);
no multi-level/vertical pathing (single 2D plane; obstacles are visually raised only); no enemy generation; no paid
assets (primitives only).
**Status:** code done, scene wired (`Test_M3_Battle`), **190/190 EditMode green** (+9 arena tests). Editor-preview
self-verified: blocks tile 1:1 with cells, obstacles raised on the right cells, hazard marker placed, checkerboard
reads. NavMesh bake + agent pathing + final look are **Play-verified** (PROJECT_STATE). See `Docs/18_ThemedEncounters.md`.

### ADR-026 — Seeded encounter terrain generation (Slice C, iteration 1 of the encounter builder)
**Decided:** combat nodes generate their own obstacle field. A pure `EncounterGenerator` (`Run.Encounters`) turns
an `EncounterDefinition` SO + the grid config + `(RunState.Seed ^ CurrentNodeId, CurrentFloor)` into a
`TerrainLayout` (Slice A) of Impassable + Hazard cells; an `EncounterBuilder` battle-scene component
(`[DefaultExecutionOrder(-100)]`) reads the active run on Awake and applies it via the new
`DeploymentManager.SetTerrain` **before** `ArenaBuilder` renders + NavMesh-bakes it (Slice B). This closes the
A→B→C loop: data → render → generate. **Deterministic** from seed^nodeId, so a node always lays out the same
board and nothing extra needs saving. Counts scale with floor (`obstaclesPerFloor`); the **player deploy zone is
kept clear** by default (`keepPlayerZoneClear`) so deployment is never blocked, and obstacles are confined to the
enemy/neutral rows (`allowEnemyZone`).
**Scope — iteration 1 (C1) is terrain only.** Enemy roster generation (C2) and obstacle-aware enemy placement +
an `EnemyEncounterSpawner` (C3) are deferred — today enemies are still scene-placed and `RunBattleSetup` scales
their stats by depth. The `EncounterDefinition` SO is the stable home for the C2 roster spec.
**With no active run** the builder leaves the scene's authored terrain (the Slice B demo layout) in place, so the
battle scene stays playable standalone; an editor preview (`Tools/Arena/Preview Generated Encounter`) generates a
sample board without Play.
**Status:** C1 code done, wired into `Test_M3_Battle`, **196/196 EditMode green** (+6 generator tests: determinism,
count ranges, player-zone-clear, enemy-zone confinement, no double-use). Editor-preview self-verified (seeded
obstacles + hazards in the neutral/enemy rows, player rows clear). Play-verify in a run (PROJECT_STATE). See
`Docs/18_ThemedEncounters.md`.

### ADR-027 — Generated, obstacle-aware enemy roster (Slice C, iterations C2+C3)
**Decided:** combat nodes spawn a generated, obstacle-aware enemy roster instead of the static scene enemy.
`EncounterGenerator.RosterSize` (deterministic from `Seed ^ nodeId`; boss = fixed count, else min/max + per-floor
scaling) decides how many; `EncounterGenerator.EnemyCells` picks that many **Passable cells in the enemy zone,
avoiding Impassable** terrain (seeded separately so obstacle and enemy placement don't correlate). An
`EnemyEncounterSpawner` (`Run.Encounters`, `[DefaultExecutionOrder(-75)]` — after `EncounterBuilder` stamps terrain,
before the player spawner) reads the run, retires the scene's `Team.Enemy` units, and spawns the roster from a
`Unit_Enemy.prefab` (Team.Enemy base) via `UnitFactory.Spawn` — mirroring `BattlePartySpawner`'s run-gated
retire-then-spawn pattern. **v1 spawns clones of the base enemy** (visual variety via the part generator is a later
pass — `RandomUnitGenerator` is player-unlock-gated, so enemies need a non-gated pool or a fully-unlocked profile
first). Difficulty rides the existing `RunBattleSetup` depth boost; no per-unit stat/class generation. With no
active run the spawner leaves the authored scene enemy (so the scene stays playable standalone), so wiring it is
low-blast-radius.
**Open caveat for Play-verify / next session — NavMesh timing.** The spawner runs in Awake (−75); `ArenaBuilder`
re-bakes the NavMesh in `Start`. Spawned `NavMeshAgent`s rely on the editor-baked mesh existing at scene load and
the enemy cells staying walkable after the re-bake (they avoid Impassable, so they should). If agents come up
off-mesh / don't move, reorder so the roster spawns AFTER `ArenaBuilder.Bake()` but still before
`BattleStateManager`'s registration sweep. This is the main thing to watch in Play.
**Status:** C2+C3 code done, wired into `Test_M3_Battle` (run-gated), **201/201 EditMode green** (+5 tests: roster
size range/boss/floor, enemy cells in-zone + obstacle-avoiding + deterministic). The spawn itself is Awake-only →
**Play-verified** (PROJECT_STATE item 1d): generated enemies appear in the enemy zone, off the obstacles, and fight.

### ADR-028 — Build-to-spec content pass: audit + locked decisions (Docs/05/07/08/09/10 + class roster)
**Context:** a build-to-spec session audited the predetermined content (elements, abilities, status, classes,
equipment) against the docs and built the foundational slice. Findings: **elements ~complete** (15 types + 5×5
chart correct; only the dual-element rule was missing); **abilities** had 8/~30 strategy classes; **status** had
1/24 authored (data-driven, no per-effect enum — correct); **classes** had 0/16 roster classes (Class_Warrior is a
placeholder) + 2/16 weapon classes; **equipment** code complete, content sparse.
**Locked decisions (this session):**
- **Monk** = its own hybrid class (NOT multi-class); one-class-per-unit holds at MVP.
- **Complex status mechanism** = build the doc's `StatusEffectBehavior` custom-SO hook (not a fold-in shortcut)
  for the 7 behavioral statuses (Frozen-amp, Marked, Unlucky, LastStand, Madness, Protected, Shield).
- **Ability strategy naming** = the code's existing convention (`_SO` suffix, no `Movement` prefix), not the doc's
  literal names.
- **Element chart topology** = stays hardcoded in `ElementChart_SO.cs` (matches doc; the scalar multipliers stay
  data-tunable on the asset).
- **Evolution system is IN scope** (not deferred): XP after each battle, stats evolving across floors/maps. This
  is a new prioritized slice; `EvolveEffect` + evolution-indexed ability strategy arrays land there (the
  `Ability_SO` axes are spec'd as evolution-indexed arrays but are single-slot in code today).
- **Equipment rarity multipliers** = align to Docs/07 (1 / 1.25 / 1.5 / 2 / 3); current assets are 1/1.5/2/2.5/3.
**Built (Slice 1a/1b):** the element dual-element "least favorable" rule via a pure `ElementMath.Multiplier`
(both damage paths route through it) + `SecondaryElement` on `UnitStatusController`; and the no-new-infra ability
strategy classes (targeting GetAll/GetAlly/GetCurrent; filters LowestHp/Random/RaceClassElement/KeepCurrentTarget;
effects NoEffect/Revive) with a tested pure `AbilitySelect` core. **209/209 EditMode green.**
**Cross-cutting [code] prerequisites discovered (gate later content), tracked in TASKS:** a status→damage-pipeline
hook (`StatusEffectBehavior`); `ClassSynergyTier.globalBuffs` (spec'd, absent in code); Accuracy/CritRate(/DefElem)
modified getters on `UnitStatusController` (buffs computed but never read); wiring ability event-triggers to the
existing `BattleEventBus` (it has OnDamageDealt/Taken/Downed/Kill/BattleStart) + per-runtime latch state; two
status bugs (resistance roll `Random.value < 0f` always-false; `RollPerTick` unhandled).
**Doc drift noted:** Docs/07 data-model section is stale vs ADR-019 (instance model) — update with the equipment slice.

### ADR-029 — Lower layers subscribe to combat events via a Core `IBattleEvents` seam (BTS-A)
**Decided:** ability triggers (and any other below-Combat consumer) reach the `BattleEventBus` through a Core
interface `IBattleEvents` (OnDamageDealt/Taken/Downed/Kill/BattleStart) published on `CombatServices.Events` by
`BattleStateManager`, rather than referencing the Combat assembly. `BattleEventBus` implements the interface; the
event payload structs already live in Core. **Why:** the Abilities assembly is below Combat in the layering and
can't take a Combat dependency; the existing `CombatServices` service-locator (Registry/ElementChart) is the
established seam for exactly this. **Per-unit event state:** `AbilityController` subscribes once combat is active,
filters events to its own unit by GameObject identity, and stamps timestamps on its `AbilityRuntime`s; event
triggers fire when their event is newer than the runtime's last cast. Keeps trigger SOs stateless (matching
`TimeBasedTrigger` reading `runtime.LastCastTime`). **Status:** built (BTS-A), 212 green; `GetAttacker` (needs the
event's other-unit in the cast context) is part 2.

### ADR-030 — Class synergies: `globalBuffs` field + 16 `UnitClass_SO` authored, [content]/[code] tier split (BTS-E1)
**Decided:** the 16-class roster (Docs/09, LOCKED) is now authored as `UnitClass_SO` assets, and the
`ClassSynergyTier.globalBuffs` field the spec listed but the code lacked is added. **`globalBuffs`** (whole-team,
any class) sits alongside `teamBuffs` (same-class only); `SynergyResolver` gets a third pass that accumulates each
active tier's `globalBuffs` per team and applies them to every live unit on that team regardless of class
(`teamGlobals` dict → per-unit add). The 16 classes are authored by a re-runnable editor tool `ClassSetupTool`
(`Tools/Build-To-Spec/Author Unit Classes`, reflection-sets the serialized fields + `SaveAssets`, idempotent) into
`Assets/Data/Classes/Class_<Name>.asset` on the **2/4/6 threshold ladder**, numbers copied **verbatim** from the
roster (first-pass/tunable — not "improved").
**[content]/[code] split (the key call):** only the pure-`StatBuff` portions of each tier are filled now —
**Barbarian/Fighter/Archer/Spearman(T2)/Heavy/Javelin/Assassin/Monk/Crossbow(T4-6)/Paladin** get real stat tiers,
and **Heavy-T6/Monk-T6/Paladin-T4-6** get `globalBuffs`. **Paladin is fully [content]** (Def/Res + team Res/Def).
Tiers whose roster effect is **[code]** (armor-pen, DoT/splash, on-hit/kill heal, atk-speed ramps, front-row /
low-HP conditionals, ability-dmg %, healing-power, team regen, strike-first, double-shot, reposition, pierce,
backline-open) are authored as **threshold + descTermKey with EMPTY buff lists** — placeholders so the ladder/asset
exists, to be wired in **BTS-E2** once the ability-effect / status / combat-hook slices (BTS-B2, BTS-D, BTS-F) land.
Never faked in the stat layer (the roster's build note). A `-10% Speed` content fragment (HandGunner-T4) is also
deferred because it only makes sense paired with its [code] damage trade-off — shipping a lone downside would be a
silent imbalance.
**Why now:** `globalBuffs` is the one architectural gap blocking the high/support tiers; the stat tiers are pure
content authorable today; the behavioral tiers are dependency-gated. Splitting E into E1 (this) + E2 (behavior)
keeps each slice green and testable.
**Status:** code + 16 assets done, **216/216 EditMode green** (+1 test: a wizard `globalBuff` reaches a same-team
warrior while the wizard `teamBuff` does not). The Accuracy/CritRate/CritDmg/Resistance getters (BTS-B1) mean
Assassin/Archer/Spearman/Paladin tiers actually fold. **`Class_Warrior.asset` is a stale pre-roster placeholder** —
left in place (may be referenced by test units) and flagged to retire when units are assigned roster classes (BTS-F).
Behavioral tiers + assigning classes→units are **Play-gated** (no class is on a live unit yet).

### ADR-031 — Battle-start polish: NavMesh re-attach, robust spawn-reveal, floor-aware deployment overlay
**Context:** Play-testing after the themed-encounter (ArenaBuilder) work surfaced three battle-start defects, all
rooted in the same theme — **things authored for the old flat Y≈0 ground, or for a spawn-before-bake order, were
broken by the runtime arena.** A read-only diagnostic workflow (3 agents) root-caused each with high confidence.
**Decided (three fixes, all Play-verify-gated — runtime behaviors, not EditMode-testable):**
- **Animations never started (the big one) — NavMesh spawn-order inversion.** Units spawn in `Awake`
  (`EnemyEncounterSpawner` −75, `BattlePartySpawner` −50) and during deployment, but `ArenaBuilder` bakes the
  runtime NavMesh in `Start` (the scene ships with NO baked mesh). NavMeshAgents came up **off-mesh and were never
  re-attached** (Unity doesn't auto-attach when a mesh later appears), so `UnitMovementController.Update`
  short-circuited at `if (!agent.isOnNavMesh) return;` **before** driving the Animator's `Speed` param → units
  froze in Idle, never moved, never attacked. **Fix:** self-heal in the movement controller — once Active, any
  off-mesh agent does `NavMesh.SamplePosition + agent.Warp` onto the baked mesh the first frame it needs to act
  (cheap; only runs while off-mesh). Chosen over a central sweep / execution-order reshuffle because it's
  self-contained and also covers units placed interactively during deployment. **Resolves the NavMesh-timing
  caveat flagged in ADR-027.**
- **Units spawned compressed + floating — fragile spawn-reveal.** `UnitSpawnInHide` zeroed the unit scale at
  Awake and relied on a DOTween `DOScale` grow-back; a tween can silently fail (DOTween capacity exhaustion when
  many units spawn, uninitialized engine, `SetLink` kill on re-parent/destroy), leaving the unit **permanently
  squashed** — which also reads as floating (a near-zero-scale unit collapsed to its pivot). **Fix:** rewrote it
  **Update-driven** (ease-out cubic, no overshoot), guaranteed to reach the authored scale, with an `OnDisable`
  restore and a zero-scale guard. Dropped the DOTween dependency entirely. Update-driven over tween precisely
  because the failure mode was "the tween didn't complete."
- **No "place here" highlight in deployment — overlay occluded by the floor.** The green deployable highlight
  (`DeploymentGridRenderer`, `CellState.Empty`) was rendered correctly but at Y≈0.02, **buried under the opaque
  checkerboard floor** whose top is at `floorSurfaceY`=0.05. **Fix:** made the renderer floor-aware — it reads
  `ArenaBuilder.FloorSurfaceY` (new accessor; same `CapsuleWars.UI` assembly) and floats every overlay tile at
  `floorSurfaceY + yOffset`, so the floor can never re-bury it even if `floorSurfaceY` is retuned. The overlay
  raycast still maps taps by XZ, so the Y lift doesn't affect placement.
**Why grouped:** one shared cause — "units/overlays are set up before the runtime arena (bake / floor lift) is
ready, and nothing re-syncs them." Also renamed the deployment confirm button **"Assemble" → "Battle Start"** per
the same feedback.
**Status:** code done, **216/216 EditMode green**, no new tests (all three are Play behaviors). **Play-verify
needed** (see PROJECT_STATE): enter Test_M3_Battle → deploy (green player-zone tiles now read on top of the floor)
→ Battle Start → units spawn at full scale, planted (not squashed/floating), and **move + attack + animate**. Noted
cleanup for later: a dead legacy `AttackOrder` Animator param (`AttackIndex` is the live one).

### ADR-032 — Customization live preview un-occluded; icon system scoped (build-not-AI)
**Context:** Play-test feedback: the customization paper-doll's live character preview "does not show up", and
parts/weapons/armor have no icons. A read-only 2-agent diagnostic workflow root-caused the preview (high
confidence) and inventoried the icon gap.
**Preview — decided + fixed:** the preview is an **in-world unit** (same base prefab, spawned at the origin
PreviewAnchor), meant to show through a transparent Screen-Space-Overlay panel — NOT a RenderTexture rig. It was
rendered correctly but **hidden behind the panel's own opaque background**: `PaperDollBuilder` set the panel Image
transparent, but the `UIThemeApplier` on it (`colorOwnBackground=true`) repaints the background with
`palette.panelBackground` (0.06,0.07,0.10,**0.96** ≈ opaque) on every OnEnable/ExecuteAlways. (Confirmed NOT the
`UnitSpawnInHide` zero-scale bug — the Update-driven rewrite is on disk and the reveal completes.) **Fix:** set
`colorOwnBackground=false` on the panel's applier so it themes child buttons/text but leaves the root transparent —
in `PaperDollBuilder.Build` (durable, re-runnable) **and** directly in `Test_M7_Map.unity` (panel Image alpha
0.96→0 + applier `colorOwnBackground` 1→0 on GameObject 1055545173) so the current scene works without a rebuild.
**Known follow-up:** with the panel transparent the preview shows against the **map backdrop** (no dedicated
background). The clean version — a preview Camera on a PREVIEW layer → RenderTexture → RawImage, preview unit on
that layer — is deferred as polish (bigger: new layer + camera + RT + builder wiring).
**Icons — scoped, NOT yet built; decision recorded.** No icon system exists: `Equipment_SO` has an `icon` field
(all 7 assets null); `BodyPart_SO` has **no icon field at all** (6 assets); 13 items total, 0 icons; the bag/slot
widgets fall back to text. **Decision: BUILD icons by rendering the actual 3D geometry** (an editor `IconBaker`
using `PreviewRenderUtility` / a hidden RenderTexture → `EncodeToPNG` → import as **Sprite** (`textureType=Sprite`,
the step the AI path also lacks today) → assign via `SerializedObject`), **not AI-generate** — free, deterministic,
re-runnable, and matches the in-game mesh; AI (the Grok pipeline) stays a manual override for hero items. Plan:
(1) add `BodyPart_SO.icon` + getter; (2) `Assets/Scripts/Editor/IconRenderer/IconBaker.cs`
(`Tools/Icons/Bake All Item Icons`, iterate `EquipmentCatalog`+`PartCatalog`); (3) widget tweaks (`BagItemWidget`
ConfigureBody → pass `part.Icon`; `PaperDollSlot.SetBody` sprite-aware; `CustomizationScreen.RefreshSlots` body
branch). **Caveat:** default body parts use built-in primitive meshes (sphere/cubes) so they bake generic
thumbnails until real meshes exist, and **per-category camera framing/angle needs the user's eyes** (Play/scene
review) — so the baker is its own task (#90 split: preview done here; icons = follow-up).
**Status:** preview fix done, **216/216 EditMode green** (no new tests — UI/scene change). Play-verify: open the
paper-doll → the character preview is now visible (against the map until the RT rig lands).

### ADR-033 — Content icons via Grok AI (flat-emblem), reversing ADR-032's "build/render" call
**Decided:** generate all content icons (classes, moves, body parts, weapons, armor) with the **existing
editor-side Grok image pipeline (AI)** in a **flat-emblem game-icon style**, NOT render-to-thumbnail — at the
user's direction, reversing ADR-032's recommendation. **Why the reversal:** classes and moves are **abstract**
(no 3D mesh to render), so AI is the *only* way to icon them; and the placeholder meshes (default body parts are
built-in sphere/cubes) render to generic blobs, so one consistent Grok style reads better across the whole set
than mixed render-thumbnails. The render path (ADR-032) is dropped; AI is now primary.
**Built — `IconGen` (`Assets/Scripts/Editor/IconGen.cs`, `Tools/Icons/*`):** per category it composes a
flat-emblem prompt (a shared style anchor + a per-class imagery map / humanized item name) → `GrokImageService
.GenerateAsync(…, "1:1", "1k")` → writes `Assets/Generated/Icons/<cat>/<asset>.png` → **imports it as a Sprite**
(`textureType=Sprite`, the one step the AssetRequest image path lacked) → assigns to the SO's `icon` via
`SerializedObject`. Generation is **sequential** (each image is a PAID xAI call), **idempotent** (skips SOs that
already have an icon; delete one to regenerate), and reuses the pipeline's `GenerationServices`/`GenerationHttp`/
`AssetPipelineImporter`. Added **`BodyPart_SO.icon`** — the only missing field (`UnitClass_SO`/`Ability_SO`/
`Equipment_SO`/`ElementType_SO`/`StatusEffect_SO` already had one). Menu: per-category + ALL + a single-icon
**test** entry to validate the pipeline cheaply before the full sweep.
**Security:** the xAI key stays in the git-ignored editor-only `SecretsConfig`; Claude drives the tool (menu
items) but **never sees/handles the key and makes no raw API calls** — the editor makes the call.
**Scope now:** classes (16) + equipment (7) + body parts (6) ≈ 29 icons. **Move icons await BTS-F** (no
`Ability_SO` move assets exist yet; Ability also needs the editor asmdef to reference the Abilities assembly).
Curate results via the pipeline's Archive/Reject; refine prompts after seeing the first batch.
**Status:** pipeline built + compiles, **216/216 EditMode green** (additive `BodyPart_SO.icon` field + editor-only
tool). Generation itself is **paid + eye-verified** — Claude can't see the icons over the user's remote setup, so
the user runs the sweep and judges quality. Widget consumption of `BodyPart.Icon` (bag/slots) is the remaining
wiring (gear already reads `Icon`); tracked with #91.

### ADR-034 — Customization preview rig: PreviewUnit-layer camera → RenderTexture → RawImage (implements ADR-032's deferred clean version)
**Decided:** replace the fragile "in-world unit shown through a transparent overlay panel" preview with an isolated
render-to-texture rig. The paper-doll spawns its preview unit onto a dedicated **`PreviewUnit`** layer; a dedicated
**preview Camera** (culling mask = that layer only, solid dark background, framed on the unit) renders to a
**RenderTexture** (`Assets/Data/Customization/CustomizationPreviewRT.renderTexture`) shown in a **`RawImage`** in the
panel centre; the **map camera's culling mask excludes** the layer. **Why:** the in-world preview rendered the unit at
world-origin against whatever the map camera happened to show, so framing/background/lighting were uncontrolled and
depended on the map camera's transform — fragile and ugly. The RT rig isolates the preview from the map scene and gives
controllable framing + a fixed background. This is exactly the "clean version" ADR-032 explicitly deferred (it fixed
the blocker — the opaque `UIThemeApplier` background — but left the in-world preview against the map backdrop).
**Built:** `PaperDollBuilder` (`Tools/Paper-Doll/Build In Open Scene`) now also (1) ensures the `PreviewUnit` layer
exists (TagManager via `SerializedObject`; reuses it or claims a free user slot 8-31; on failure returns -1 and the
rig is **skipped**, keeping the ADR-032 in-world preview — it never falls back to Default, which would blank the main
camera); (2) creates the RenderTexture asset; (3) (re)builds an idempotent `PaperDoll_PreviewRig` (anchor + camera,
parked at y=1000 — the culling mask is what really isolates it); (4) builds + wires the centre `RawImage` (texture =
RT, `raycastTarget=false` so drops still reach the panel-root drop zone, created first so it renders behind the
overlapping Stats button); (5) wires `CustomizationScreen.previewAnchor` to the rig anchor and clears the layer from
`Camera.main`'s mask. `CustomizationScreen` propagates the anchor's layer onto the spawned unit + its lazily-added
equipment/body meshes (re-applied each `RefreshAll`). **Backward-compatible:** until the builder is re-run the anchor
sits on Default, so the unit renders in-world exactly as ADR-032 — the rig only activates after a build.
**Deferred follow-ups:** dedicated preview *lighting* (skipped to avoid URP double-lighting the map; the preview unit
is lit by the existing scene sun for now); making the panel background *opaque* now that see-through is no longer
needed; *disabling* the preview camera while the panel is closed (currently always-on — cheap, an empty layer).
**Status:** code done; **NOT compile/Play-verified by Claude** (Unity bridge offline this session; RT/overlay UI
isn't visible over the user's remote setup). To land: `run_tests` (expect 216/216 — change is additive, no test
touched), then `Tools/Paper-Doll/Build In Open Scene`, **save the scene**, and Play-verify: open the paper-doll →
the unit renders in the centre RawImage against the dark background; equipping gear/body parts updates the preview;
framing looks right (tune `BuildPreviewRig` camera `localPosition`/`fieldOfView` by eye); the map camera no longer
shows the preview unit; drag-drop onto the doll still works (the RawImage doesn't block it).

### ADR-035 — Status behavioral damage hook: `StatusEffectBehavior` + `ModifyIncomingDamage` (BTS-B2)
**Decided:** the damage-pipeline behavioral statuses (Docs/10) work through a custom-SO hook (the "complex
mechanism" the design owner chose to keep, not a stat-fold shortcut). A new abstract `StatusEffectBehavior`
ScriptableObject (`Data.StatusEffects`) exposes `ModifyIncomingDamage(StatusDamageContext, int) → int`;
`StatusEffect_SO` gains an optional `behaviorSO` + a `behaviorMagnitude` seed. `UnitHealthController.TakeDamage`
now takes a **`DamageKind`** (new Core enum: Physical / Elemental / True = basic attacks / abilities / DoT) and,
before reducing HP, calls `UnitStatusController.ModifyIncomingDamage`, which walks the unit's active statuses,
lets each behavior adjust the running amount, then removes any that ask to be consumed.
**Layering:** the hook lives in Data (below Units), so the context carries only Core/Data types — `IUnitRef`,
`DamageKind`, the target's pre-hit HP fraction, and an in/out `BehaviorValue` (per-instance state, e.g. a Shield's
remaining absorb — stored on `ActiveStatusEffect`, threaded through the context). Behaviors never reference Units.
**The 5 behaviors (code; assets authored in BTS-D):** Marked (+25% from all), Frozen (×1.5 **Physical only**),
Protected (negate next hit + self-consume), Shield (absorb a flat pool, deplete, remove when spent), LastStand
(reduce damage below an HP threshold — the +Atk half is a separate conditional stat buff, deferred). First-pass numbers.
**Open/tuning:** multiple simultaneous behaviors compose by simple list-iteration (not sorted) — fine for 1–2
statuses; DoT ticks pass through as `True` (Marked amps, Frozen doesn't), and Protected/Shield can be spent by a
DoT tick (acceptable, flagged). **Status:** code + 8 tests, **224/224 EditMode green**. Unblocks BTS-D's 7
behavioral statuses + the [code] behavioral class-synergy tiers (BTS-E2). No status carries a behavior asset yet → Play-gated.

### ADR-036 — Behavioral class-synergy tiers via granted effects + an ability-host sink (BTS-E2)
**Decided:** a `ClassSynergyTier` can now grant **behavioral [code] effects**, not only StatBuffs. Rather than a
parallel event system, the behaviors reuse the existing ability/event stack: `AbilityController` already subscribes
to the battle event bus (BTS-A) with self-checked `OnKill`/`OnDamageDealt` handlers, so it hosts the synergy heals.
**Seam:** `Core/SynergyEffect.cs` adds `SynergyEffectKind {HealOnKill, HealOnHit}`, a `SynergyEffect {kind, magnitude}`
struct, and an `ISynergyBehaviorSink` interface — all in **Core**, so `SynergyResolver` (Combat) pushes effects to each
unit via `GetComponentInChildren<ISynergyBehaviorSink>()` **without Combat referencing the Abilities assembly**.
`AbilityController` implements the sink; its kill/hit handlers heal `magnitude%` of MaxHp. No new component, no prefab change.
**Resolver:** `behaviorBuilder` is seeded per-unit in pass 1 (so units whose tier deactivates get cleared) and filled
in pass 2 from the unit's own active tier (same-class, like `teamBuffs`), then pushed alongside the buffs.
**Content:** Barbarian heal-on-kill 5% (T4 + T6), Monk heal-on-hit 2% (all tiers). Effects are **repeated on every
tier at/above unlock** because `GetActiveTier` returns only the highest met tier (no cross-tier stacking). Heal
magnitudes are **first-pass** — the roster specifies the trigger, not the %.
**Scope/deferred:** only the two heal kinds are wired. The other [code] tiers (armor-pen / ignore-Def, atk-speed
ramps, HP-conditional buffs, DoT/splash, strike-first, double-shot, reposition, pierce, backline-open) need combat
mechanics that don't exist yet (Def-in-damage, attack-cadence hooks, conditional-stat layer) — extend `SynergyEffectKind`
+ the sink as those land. **Status:** code + 2 resolver tests (push when active / clear below threshold), **226/226
EditMode green**. Heal-in-combat is **Play-gated** (needs a live battle with kills/hits).

### ADR-037 — Unit evolution: persisted XP grows BASE stats, applied at the player spawn (BTS-H)
**Decided:** units gain XP per battle and "evolve" — higher evolution tiers scale their BASE stats. The math is a
pure, testable helper: `EvolutionConfig_SO` (Data) holds ascending cumulative `xpThresholds`, `statGrowthPerTier`,
and `xpPerBattleWin`; `UnitEvolution` (Data, static) maps `TierFor(xp, thresholds)` → tier and
`GrowthMultiplier(tier, rate)` = `1 + tier*rate`. XP lives on `UnitDTO.Xp` (additive; SaveVersion stays 1).
`UnitStatusController.SetEvolutionMultiplier(m)` scales **only the BASE stat** inside `GetModifiedStat/F`, so
buffs/equipment/synergy layer on top of the evolved base (order-independent; no change to `SumBuffs`).
**Earn + apply seams:** `EvolutionGrant.GrantXp(state, config)` (Run) adds `xpPerBattleWin` to each party
`UnitDTO` and is called by `BattleNodeReturn` on ANY win, inside the existing `RunSession.Save()` path. The
multiplier is applied at spawn by **`BattlePartySpawner.ApplyEvolution`** — the sole live player-party spawner —
at BOTH its spawn paths (standalone `SpawnParty` + deployment `SpawnOrMoveAt`), reading that member's `dto.Xp`.
**Why apply in the spawner, not `UnitFactory.Spawn`:** the factory is shared by `EnemyEncounterSpawner` + the
customization preview; injecting there would evolve enemies + previews. `BattlePartySpawner` is player-team-exclusive
by construction, so evolution stays player-only with no per-team flag leaking into the persistence layer. The
injection point was confirmed by a 5-angle read-only spawn-trace workflow (the live path is `UnitFactory.Spawn`,
NOT the `FromDTO` overload, which has no live caller). No asmdef change — `CapsuleWars.Run` already references
Data + Units; usings already present.
**Config plumbing:** a `[SerializeField] EvolutionConfig_SO` on `BattlePartySpawner` (mirrors the one already on
`BattleNodeReturn`); the asset is `Assets/Data/Units/EvolutionConfig.asset` (thresholds 100/250/450/700, +12%/tier,
60 XP/win — first-pass, tunable). Null config = multiplier 1 (no-op), so evolution is optional per scene.
**Status:** code-complete, **243/243 EditMode green** (`UnitEvolutionTests` + `EvolutionWiringTests`).
**Play-gated:** assign the config asset on `BattlePartySpawner` (battle scene) + `BattleNodeReturn` (`Test_M3_Battle`),
then verify a post-win unit spawns with grown stats. **Deferred:** evolution-indexed `Ability_SO` strategy arrays +
`EvolveEffect` + `ChangeSizeEffect` (needs the VFX pipeline).

### ADR-038 — Units acquire their class ability kit via a registry + self-wiring loader (BTS-F part 2)
**Decided:** a unit's move kit is driven by its CLASS, resolved at spawn. A new `ClassAbilitySet_SO` (in the
**Abilities** assembly — the lowest layer that can name BOTH `UnitClass_SO`/Data and `Ability_SO`/Abilities) maps each
class → its `Ability_SO[]`; `AbilitiesFor` matches by class reference, falling back to the stable `ClassId`.
`AbilityController` gains `SetAbilities(...)` + a re-runnable `BuildRuntimes()` (idempotent, lazy-`root` → **order-
independent**). A self-wiring `ClassAbilityLoader` MonoBehaviour on the **shared base unit prefab** reads
`UnitStatusController.UnitClass` at Awake and installs the kit — so BOTH player (`BattlePartySpawner`) and enemy
(`EnemyEncounterSpawner`) units, which clone that prefab through `UnitFactory.Spawn`, are covered with **zero spawner
edits**. `ClassAbilitySetupTool` authors `ClassAbilitySet.asset` (16 classes / 32 abilities) idempotently.
**Why a registry + loader (not a field on `UnitClass_SO`, nor logic in `UnitFactory`):** Data must not reference
Abilities, so `UnitClass_SO` can't hold `Ability_SO`; `UnitFactory` (Persistence) also can't see Abilities; and
`UnitFactory.Spawn` is shared with enemies + the customization preview, so wiring there would over-apply. The
injection point + layering were confirmed by a 5-angle read-only design Workflow.
**Prerequisite surfaced:** NO per-unit class existed — every unit inherited Warrior + a placeholder QuickStrike from
the base prefab (`UnitDTO`/`UnitDefinition_SO` carry no class; generators TODO it). **First-pass activation (chosen
"see it live"):** repoint the base prefab to `Class_Monk` (WC_Unarmed → casts with no weapon, dodging the weapon-gate)
+ add the loader + clear QuickStrike; `Unit_Enemy` inherits via its variant. So every unit is uniformly a Monk for now.
**Deferred:** per-unit class VARIETY (`UnitDTO.ClassId` + a class catalog + generator rolls writing
`UnitStatusController.unitClass`) is the real follow-up that retires the uniform slice + `Class_Warrior`. **Weapon-gate
caveat:** all 32 abilities require a weapon, so any non-Unarmed class spawned without matching equipment cast-locks
until equipment loadouts exist. **Status:** code-complete + base-prefab wired, **245/245 EditMode green** (registry
resolution + `SetAbilities` rebuild). Live in-combat casting is **Play-gated**.

<!-- Add new decisions below as ADR-011, ADR-012, ... -->
