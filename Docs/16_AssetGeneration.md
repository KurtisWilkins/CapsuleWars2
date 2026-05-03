# Asset Generation Pipeline

## Status
**Post-MVP.** Stubs in `Assets/Scripts/Generation/` from M0; full implementation post-launch.

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
