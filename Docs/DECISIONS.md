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
**Status:** code complete, **169/169 EditMode green**, committed. In-editor scene assembly + wiring is a manual
checklist (`Docs/CHECKLIST_PaperDoll.md`) — the MCP bridge in that session couldn't read refs / page the
hierarchy / run editor code, so the panel couldn't be assembled blind safely. Behavior gate is the Play list in
that checklist + PROJECT_STATE.

<!-- Add new decisions below as ADR-011, ADR-012, ... -->
