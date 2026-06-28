# Big Cats tint batch — staging manifest (region-tint model, ADR-040)

Staged for human archive/reject review. **NOT imported.** **Live render is PENDING the region-tint shader
milestone** (not yet built) — these are valid shader inputs (color slots + grayscale region masks), authored as data.

- **TintPresets** (`Assets/Generated/Tints/`): `primaryColor` / `secondaryColor` / `accentColor` (+ optional `regionMask`).
- **Masks** (`Assets/Generated/Masks/`): grayscale; **white = secondary/marking region, black = primary/base.**
  Generated via `MaskGen` (Grok, flat grayscale, framing-bypassed). The tiger mask was inverted to the white=marking
  convention (Grok rendered literal black stripes); spot patterns came out correct-convention as-is.

| Preset | Primary | Secondary | Accent | Mask | Status |
|--------|---------|-----------|--------|------|--------|
| Tint_Pantherfolk   | near-black | near-black | amber | — (solid) | render pending region-tint shader |
| Tint_Pumafolk      | tawny | tawny | dark | — (solid) | render pending region-tint shader |
| Tint_Tigerfolk     | orange | black | amber | Mask_Tigerfolk (stripes) | render pending region-tint shader |
| Tint_Leopardfolk   | tawny | dark brown | amber | Mask_Leopardfolk (rosettes) | render pending region-tint shader |
| Tint_Jaguarfolk    | tawny | dark brown | amber | Mask_Jaguarfolk (large rosettes) | render pending region-tint shader |
| Tint_Cheetahfolk   | tan | black | amber | Mask_Cheetahfolk (small spots) | render pending region-tint shader |
| Tint_Snowcatfolk   | pale gray | dark gray | pale | Mask_Snowcatfolk (open rosettes) | render pending region-tint shader |
| Tint_Lynxfolk      | gray-brown | cream | amber | — (mask optional) | render pending region-tint shader |
| Tint_Sabertoothfolk| tan | tan | dark | — (solid, spill) | render pending region-tint shader |

**Provenance:** BIGCATS_BATCH §3, region-tint pivot (chat injection, 2026-06-28). Colors are default starting slots
(player-overridable later). Patterns are now produced as data (masks), not deferred.
