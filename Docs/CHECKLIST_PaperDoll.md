# Checklist — assemble the paper-doll customization panel (`Test_M7_Map`)

> The paper-doll **code is complete, compiling, and EditMode-green (169)** and committed. This is the in-editor
> scene assembly + wiring, which must be done by hand: in this session the MCP bridge could not read component
> refs / page the hierarchy / run editor code, so the panel could not be built blind safely. Do this in the
> Unity editor where you can see the layout. Estimated ~15 min. Then run the Play verification at the bottom.

## What the code already does for you (so this stays small)
The `CustomizationScreen` (`Assets/Scripts/UI/Customization/CustomizationScreen.cs`) **generates** the slot
widgets and bag items at runtime, and **auto-adds** the rest. So you do NOT create:
- the equipment/body **slot** widgets — generated from `EquipmentSlot` (8) + the preview's mounted `PartSlot`s,
- the **bag item** widgets — generated from the catalog/parts,
- the **drop zone** — `EnsureDropZone()` adds a `PaperDollDropZone` + a raycast Image to `panelRoot` at runtime,
- the **drag ghost** — created at runtime under the panel's canvas,
- the **foreground** Canvas/GraphicRaycaster/CanvasGroup — `EnsureForeground()` adds them to `panelRoot`.

You only create **empty layout containers + the footer/tab/close controls**, then wire refs.

## A. Reuse the existing panel as `panelRoot`
The old list UI (the former `equipmentListRoot` + `equipButtonPrefab`) is gone from the script; those serialized
refs are dropped. Keep the existing customization panel object as `panelRoot` (so the `CustomizationLauncher`
keeps working) and **delete the leftover old list children** under it (the old scroll/list + per-item buttons).

**CRITICAL — preview compositing:** the preview is a live **3D unit** spawned at root `PreviewAnchor` and rendered
by the Main Camera; the panel is an overlay on top. So `panelRoot`'s background Image must be **transparent**
(alpha ≈ 0) — or have no opaque graphic over the center — or it will hide the preview. (The runtime drop zone
keeps `panelRoot` raycast-able regardless, so a near-zero-alpha Image is fine.) Put opaque backgrounds only on
the edge widgets (columns / bag / footer).

## B. Create these empty containers under `panelRoot`
Anchors are suggestions — tune by eye. The 3D preview sits in the screen center; flank it.

| Object | Components | Rough placement |
|---|---|---|
| `LeftColumn` | RectTransform + `VerticalLayoutGroup` | left edge, vertical strip |
| `RightColumn` | RectTransform + `VerticalLayoutGroup` | right-center, flanking the preview |
| `BodyRow` | RectTransform + `HorizontalLayoutGroup` | bottom-center strip |
| `BagViewport` | RectTransform + `Image` + `RectMask2D` | right side panel |
| `BagContent` (child of `BagViewport`) | RectTransform + `GridLayoutGroup` + `ContentSizeFitter` (Vertical = Preferred) | top-anchored, stretches width |
| `Bag` (parent of `BagViewport`) | `ScrollRect` (set `content`→`BagContent`, `viewport`→`BagViewport`, Horizontal off) | right side |
| `StatFooter` | RectTransform + `HorizontalLayoutGroup` | below the preview |
| `HPText`, `DamageText`, `ArmorText` (children of `StatFooter`) | `Text` | three readouts |
| `StatsButton` | `Button` + child `Text` "STATS" | near the footer |
| `GearTabButton` | `Button` + child `Text` "Gear" | above the bag |
| `BodyTabButton` | `Button` + child `Text` "Body" | above the bag |
| `CloseButton` | `Button` + child `Text` "Done/Close" | a corner (reuse the existing one if present) |

`VerticalLayoutGroup`/`GridLayoutGroup` tips: turn on Child Control Width/Height; set a sensible cell size for the
grid (~80×80) and a small spacing. The generated slot widgets carry a `LayoutElement` (70×70 preferred) for the
columns.

## C. Wire the `CustomizationScreen` component (on the customization object)
Already wired (leave as-is): `baseUnitPrefab`, `definitionCatalog`, `partCatalog`, `equipmentCatalog`,
`previewAnchor`, `starterItems`, `inspectionPanel`.

Set these NEW fields:
- `panelRoot` → the panel root (if it changed)
- `leftColumnRoot` → `LeftColumn`
- `rightColumnRoot` → `RightColumn`
- `bodyRoot` → `BodyRow`
- `bagContentRoot` → `BagContent`
- `hpText` / `damageText` / `armorText` → the three footer Texts
- `statsButton` → `StatsButton`
- `bagGearTabButton` → `GearTabButton`
- `bagBodyTabButton` → `BodyTabButton`
- `closeButton` → `CloseButton`
- `palette` → the shared **`UIThemePalette`** asset (the one `Canvas`/`LegacyPanel` already use)

## D. Theme
Add a `UIThemeApplier` on `panelRoot` and assign the palette → it recolors the static buttons/text. The generated
slot/bag widgets self-color from `palette.buttonNormal` (empty) / `palette.accent` (filled/equipped), which the
screen passes in. Do **not** replicate any fantasy art style — the existing theme only.

## E. Body-part slots prerequisite
Cosmetic slots appear only for `PartSlot`s the **preview prefab's `UnitCustomization` has a SlotMount for**, and
the Body bag only lists `PartCatalog` parts for those slots. If no body slots show, add SlotMounts to the preview
prefab and/or parts to the catalog (separate task).

## Play verification (the behavioral gate — do all of these)
1. **Open**: in a run, on the map, click Customize → the paper-doll opens on top, preview unit visible in center.
2. **Tap-to-route (gear)**: tap a Gear bag item → it equips to ITS correct slot (slot read from the item), the
   mesh appears on the unit's socket, and HP/DAMAGE/ARMOR update live.
3. **Tap-to-route (body)**: switch to the **Body** tab → tap a part → it lands in its correct body slot and the
   cosmetic mesh swaps on the preview.
4. **Drag-and-drop**: drag a bag item onto its matching slot → equips; drag onto a WRONG slot → rejected (red
   flash), nothing equips; drag onto the doll background → auto-routes to the correct slot.
5. **Unequip**: tap a filled slot → it clears and returns to the placeholder; the bag item un-highlights.
6. **Stats button**: opens the shared `UnitInspectionPanel` with the full breakdown; live-updates on equip.
7. **Persistence — gear**: equip gear, Close, reopen → the gear is still equipped; confirm it shows on the combat
   unit too.
8. **Persistence — body parts (the new bit)**: change a body part, Close, reopen → the part change persisted
   (`dto.Parts`). A gear-only session should NOT alter a definition-driven unit's parts.
9. **Mobile**: on the Simulator/touch, tap-to-route works; the bag scrolls (add a Scrollbar if dragging items
   fights the scroll — tap-to-route is the primary touch path).

## Notes / tradeoffs
- The preview is the existing 3D unit (not a 2D portrait); columns flank the camera view — alignment is by eye.
- No run-inventory yet: the bag = `EquipmentCatalog ∪ starterItems` (gear) and `PartCatalog` (body); equipped =
  highlighted, not consumed (backlog: real owned-inventory).
- Body-part persistence writes `dto.Parts` only when a part was edited (guarded by `partsDirty`).
