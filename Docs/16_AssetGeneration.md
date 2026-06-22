# Asset Generation Pipeline

## Status
**Assisted-manual pipeline implemented** (ADR-015). The queue + prompt workflow ship in the editor as
**Tools ▸ CapsuleWars ▸ Asset Pipeline** (`Assets/Scripts/Editor/AssetPipeline/`). No image/3D APIs are
configured yet, so the tool **writes the Grok/Meshy prompts and you run them**, then paste results back; the
`IGenerationService` + `SecretsConfig` seam lets "Generate" buttons activate once a key is added. The
auto-generation API calls below remain the post-launch target.

### Implemented now (assisted-manual)
- `AssetRequest` ScriptableObject (one per asset) holds every stage's artifact + a `PipelineStage`; persists
  under `Assets/Editor/AssetPipeline/Requests/` so the queue survives sessions.
- **Asset Pipeline** EditorWindow: queue grouped by stage; add / advance / rollback; **Copy Grok prompt** /
  **Copy Meshy prompt** (clipboard); paste **Chosen image** + **Imported model**; set category/slot/socket;
  **Create / Wire item**; edit description.
- `PromptTemplates` bakes the locked Rayman/AssetHunts style into the concept/Grok/Meshy prompts.
- `AssetPipelineImporter` builds a prefab under `Assets/Generated/Meshy/{slot}/` + creates an `Equipment_SO`
  (Weapon/Armor) or `BodyPart_SO` (BodyPart) and adds it to `EquipmentCatalog_SO` / `PartCatalog_SO`.
- Claude works the queue over the MCP bridge by writing concepts/prompts/description into the `AssetRequest`.

## Goals
1. **Meshy AI** — generate new low-poly capsule-style limb variants, weapons, enemy parts.
2. **Image generation API** (Grok or similar) — generate ability icons with consistent style.

Both feed an Editor-only workflow; nothing runs at game runtime.

## Meshy workflow (target)
1. Editor window: `Tools → CapsuleWars → Meshy Asset Generator`.
2. Inputs:
   - Reference 3D model (from AssetHunts! Capsule kit, our visual baseline).
   - Style prompt template (locked to Rayman-style low-poly).
   - Variant prompt (e.g. "crab claw hand", "wooden peg leg").
   - Output target (which `BodyPart_SO` slot category).
3. Editor calls Meshy API, polls until complete, downloads FBX.
4. Imports to `Assets/Generated/Meshy/{slot}/{name}.fbx`.
5. Auto-creates a `BodyPart_SO` referencing the imported mesh.
6. Adds to a review queue — designer approves before adding to unlock pool.

## Image gen workflow (target)
1. Editor window: `Tools → CapsuleWars → Ability Icon Generator`.
2. Inputs:
   - Ability name + description.
   - Element family (drives palette hint).
   - Style template prompt (locked).
3. Calls image API, downloads PNG.
4. Imports to `Assets/Generated/Icons/Abilities/{name}.png` with proper sprite import settings.
5. Wires icon onto the matching `Ability_SO`.

## Configuration
- API keys live in `Tools/Editor/SecretsConfig.json` — git-ignored. Optional fallback to env vars.
- Rate limits respected via per-job throttle.
- Cost ceiling: per-session token/dollar cap configurable in Editor preferences.

## Style locking
Both pipelines use template prompts that bake in the Rayman/AssetHunts! aesthetic. Designers fill in variant text only; full prompt is constructed by the tool. This keeps generated assets visually coherent.

## Review queue
Generated assets land in `Assets/Generated/_Review/` with a `.review` marker. Designer approves → moved to `Assets/Generated/Approved/{slot}/`. Only approved assets are added to runtime SOs.

## Open items
- Specific Meshy API endpoints and model versions (subject to change).
- Whether icon gen uses Grok, OpenAI, or a local model.
- Whether to keep generated assets in the main repo or LFS / external bucket. Default: LFS.
