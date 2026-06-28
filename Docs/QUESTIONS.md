# QUESTIONS.md — parked items from generation runs

> Items an autonomous run couldn't resolve without a human/design decision. Each: ambiguity / tried /
> recommend. Parked, not blocking — the run continued past them.

### ✅ RESOLVED — Pattern markings (stripes / spots / rosettes) — Big Cats run, 5 races
- **resolution (2026-06-28, region-tint pivot ADR-040):** the TintPreset model became primary/secondary/accent + a
  region mask, so a marking IS the secondary region of a mask. Patterns are now produced as DATA, not deferred — all
  5 grayscale marking masks were generated (`Assets/Generated/Masks/`) and attached to their presets. Live render
  still pends the region-tint shader milestone. Original park below for history:
- **ambiguity:** Tigerfolk (stripes), Leopardfolk (spots), Jaguarfolk (rosettes), Cheetahfolk (small spots),
  and Snowcatfolk (pale + dark spots) need surface *markings*, but the runtime tint system (ADR-039) is a
  luminance→3-color ramp — it can recolor but **cannot produce patterns**. Faking patterns via the ramp is
  explicitly forbidden by GENERATION_CONTRACT.md.
- **tried:** authored base-color `TintPreset` assets for all 5 (orange / tawny / tawny / tan / pale) staged in
  `Assets/Generated/Tints/` — the color half is done; the marking layer is the open part.
- **recommend:** a pattern-layer mechanism, two candidate paths: (a) a **shader pattern layer** — extend
  `CapsuleWars/TintRamp` with an optional masked detail/marking texture (per-pattern tiling mask) sampled and
  blended over the ramp, driven by a per-instance pattern id; or (b) **baked grayscale value** — bake the
  marking into the part's grayscale source so the ramp shades it (loses the "one base mesh, many variants"
  economy — a marked mesh per pattern race). (a) keeps one base mesh + is data-driven, so it's the better fit
  for the family model — but it's its own milestone (a "pattern milestone" on top of the tint milestone).
  Decision needed before any pattern race ships looking correct.

### Part-slot manifest mismatch — felid base geometry (Big Cats run §1)
- **ambiguity:** BIGCATS_BATCH.md §1 lists 6 anatomical base parts (Head, Torso, UpperArm, Forearm, Thigh,
  Shin), but the live rig (`PartSlot`: Body, LeftHand, RightHand, LeftFoot, RightFoot, HeadProp) has **no
  separate upper/lower arm or thigh/shin slots** — limbs are single "hand"/"foot" parts on a floating-limb rig.
- **tried:** confirmed the live enums (`Core/PartSlot.cs`, `Core/EquipmentSlot.cs`, pipeline
  `Style/PartTemplate.cs` PartType). The batch's 6→ live 6 don't line up 1:1.
- **recommend:** map the felid base to the LIVE slots — `HeadProp` (fanged head), `Body` (feline torso),
  `LeftHand`/`RightHand` (clawed hands; mirror R from L), `LeftFoot`/`RightFoot` (digitigrade feet; mirror R
  from L). Drop the separate UpperArm/Forearm and Thigh/Shin breakdown (the rig doesn't expose those joints as
  parts). Net: **5 generated base parts** (head, torso, 1 hand, 1 foot) + 2 mirrors — not 6 + 4. Trust live
  state per the contract; logged here + in SESSION_LOG. No blocker — applies when §1 mesh generation runs.
