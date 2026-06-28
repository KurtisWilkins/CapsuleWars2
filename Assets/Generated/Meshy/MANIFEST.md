# Felid (Big Cats) — staged mesh manifest

> **Staging record — UNCOMMITTED.** Human archive/reject is the gate (review window: Tools ▸ CapsuleWars ▸ Asset Review; `_contact_sheet_final.png` for the corrected 9-up overview).
> **Provenance:** Roster Category 2 (Big Cats, races 64–72), felid base family. Pipeline: grayscale Grok image → Meshy image-to-3D → textured FBX. Style: chunky floating-limb, matte neutral grayscale, single isolated part. Color is NEVER baked — the grayscale `_BaseColor.png` is the part's source of truth + the region-tint shader's luminance.
> **Review status:** all images approved in the window; the rejected first passes were regenerated (see corrections below).

## Base parts (6) — all textured (FBX + `_BaseColor.png` + `_Mat.mat`)
| Part | File | Gender | Notes |
|------|------|--------|-------|
| Torso (male) | `Body/Felid_Base_Torso 1.fbx` | M | smooth neck (no socket/ruff), lean + fur-ruff design #2 |
| Torso (female) | `Body/Felid_Base_Torso_F.fbx` | F | smooth neck |
| Head (male) | `Helmet/Felid_Base_Head_M.fbx` | M | cat ears/muzzle/fangs, smooth underside |
| Head (female) | `Helmet/Felid_Base_Head_F.fbx` | F | softer features, smooth underside |
| Hand (left) | `LeftHand/Felid_Base_Hand_L 2.fbx` | neutral | **CLOSED FIST** w/ grip channel (corrected from open paw); gender-shared |
| Foot (left) | `LeftFoot/Felid_Base_Foot_L 1.fbx` | neutral | **NORMAL foot** — smooth furry top, claws at toes, pads on hidden underside (corrected); gender-shared |

## Gear (3) — all textured, gender-neutral, race-shared
| Item | File | Slot |
|------|------|------|
| Fanged helm | `Helmet/Felid_Helm_Fanged.fbx` | Helmet |
| Clawed pauldron | `Felid_Pauldron_Clawed.fbx` (slot-1 folder) | Shoulders |
| Claw-blade weapon | `Felid_Weapon_ClawBlade.fbx` (slot-0 folder) | Weapon (1H) |

> **Gauntlet DROPPED** — "gauntlet/glove" is not an equipment slot per `Docs/EQUIPMENT_CATALOG.md` (6 slots: Helmet, Chest, Shoulders, Back, Right hand, Left hand). The `Felid_Gauntlet_Claw` request + its stale mesh are rejected/abandoned and not part of the set.

## Right side — mirrored in Unity, NO separate asset
The R hand + foot are produced by instantiating the L mesh with `localScale.x = -1` at prefab-wiring time ("mirror, never regenerate"). Linked `_R` request records exist (`Felid_Base_Hand_L_Right`, `Felid_Base_Foot_L_Right`, refreshed from the corrected L images) but carry no mesh — the engine mirrors the L mesh. No R Meshy spend.

## Companion data (earlier in run)
- 9 `TintPreset` assets in `Assets/Generated/Tints/` (region-tint: primary/secondary/accent + mask slot).
- 5 grayscale region masks in `Assets/Generated/Masks/` (pattern races: tiger/leopard/jaguar/cheetah/snowcat).

## Review notes (park)
- Heads, fist hand, and helm baked **light/low-contrast** grayscale — fine for tinting, but may read flat once colored; consider a midtone pass if so.
- Live recolor of all of the above PENDS the not-yet-built region-tint shader (ADR-040).
- Old superseded meshes still on disk (open-paw `Felid_Base_Hand_L 1.fbx`, pads-on-top `Felid_Base_Foot_L.fbx`, the gauntlet, stale spike torso) — ignore / delete during the archive pass; the request `importedModel` points at the correct latest mesh.
