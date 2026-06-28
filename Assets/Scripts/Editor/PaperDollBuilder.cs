using CapsuleWars.UI.Customization;
using CapsuleWars.UI.Inspection;
using CapsuleWars.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CapsuleWars.Editor
{
    /// <summary>
    /// One-shot builder for the paper-doll customization panel (ADR-021). Generates the layout
    /// containers + footer + Gear/Body bag + buttons under the existing CustomizationScreen panel and
    /// wires every serialized ref via SerializedObject, so the scene assembly is deterministic instead
    /// of hand-wired. Idempotent: re-running deletes the previous build first. The slot/bag item
    /// widgets themselves are generated at runtime by CustomizationScreen — this only builds the
    /// containers it generates INTO. Also builds the preview rig (ADR-034): a dedicated PreviewUnit
    /// layer + off-map camera → RenderTexture → centre RawImage, replacing the fragile in-world preview.
    /// Editor-only; never ships.
    /// </summary>
    public static class PaperDollBuilder
    {
        private const string Marker = "PaperDoll_Built";
        private const string Font = "LegacyRuntime.ttf";
        private const string PalettePath = "Assets/Data/DarkTheme.asset";

        // Preview rig (ADR-034): a dedicated layer + camera → RenderTexture → RawImage, so the preview
        // unit renders isolated from the map instead of in-world behind a transparent panel.
        private const string PreviewLayerName = "PreviewUnit";
        private const string RigMarker = "PaperDoll_PreviewRig";
        private const string PreviewRtPath = "Assets/Data/Customization/CustomizationPreviewRT.renderTexture";

        [MenuItem("Tools/Paper-Doll/Build In Open Scene")]
        public static void Build()
        {
            var screens = Object.FindObjectsByType<CustomizationScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (screens.Length == 0) { Debug.LogError("PaperDollBuilder: no CustomizationScreen in the open scene."); return; }
            var cs = screens[0];
            var so = new SerializedObject(cs);

            // Panel root = the existing panelRoot field, else the screen's own GameObject.
            var panelProp = so.FindProperty("panelRoot");
            var panel = panelProp.objectReferenceValue as GameObject;
            if (panel == null) { panel = cs.gameObject; panelProp.objectReferenceValue = panel; }

            // Idempotent: clear a previous build.
            var prev = panel.transform.Find(Marker);
            if (prev != null) Object.DestroyImmediate(prev.gameObject);

            // The new design free-positions child containers, so drop the old list VerticalLayoutGroup.
            var oldVlg = panel.GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) Object.DestroyImmediate(oldVlg);

            var panelRT = panel.GetComponent<RectTransform>() ?? panel.AddComponent<RectTransform>();
            Stretch(panelRT);

            // Panel bg transparent (3D preview renders behind the overlay; must show through) but raycastable
            // (the runtime drop zone reuses this Image).
            var bg = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0f);
            bg.raycastTarget = true;

            var palette = AssetDatabase.LoadAssetAtPath<UIThemePalette>(PalettePath);
            var btnColor = palette != null ? palette.buttonNormal : new Color(0.22f, 0.26f, 0.34f, 1f);
            var panelColor = palette != null ? palette.panelBackground : new Color(0.10f, 0.12f, 0.16f, 0.92f);

            // Preview rig (ADR-034): ensure the PreviewUnit layer + RenderTexture exist, then (re)build the
            // off-map camera rig that renders the preview unit into the RT. If no dedicated layer can be
            // claimed we SKIP the rig entirely and fall back to the in-world preview (ADR-032), rather than
            // risk hijacking a builtin layer / blanking the main camera.
            int previewLayer = EnsurePreviewLayer();
            RenderTexture previewRt = null;
            Transform previewAnchor = null;
            if (previewLayer >= 0)
            {
                previewRt = EnsurePreviewRenderTexture();
                previewAnchor = BuildPreviewRig(previewLayer, previewRt);
            }

            // Root holder for everything we generate (one child → easy idempotent cleanup).
            var root = NewRect(Marker, panel.transform);
            Stretch(root);

            // Live preview surface: a RawImage fed by the rig's RenderTexture, in the centre column. Created
            // first so it renders BEHIND the slots/footer/Stats button that overlap it; raycastTarget off so
            // drops fall through to the panel-root drop zone. (Skipped if the rig couldn't be built.)
            if (previewRt != null)
            {
                var preview = NewRect("PreviewImage", root);
                Anchor(preview, new Vector2(0.18f, 0.31f), new Vector2(0.60f, 0.93f));
                preview.SetAsFirstSibling();
                var previewImg = preview.gameObject.AddComponent<RawImage>();
                previewImg.texture = previewRt;
                previewImg.raycastTarget = false;
            }

            // Two flanking gear columns.
            var left = Column("LeftColumn", root, new Vector2(0.02f, 0.28f), new Vector2(0.17f, 0.93f), panelColor);
            var right = Column("RightColumn", root, new Vector2(0.60f, 0.28f), new Vector2(0.75f, 0.93f), panelColor);

            // Cosmetic body-part row (bottom centre).
            var body = NewRect("BodyRow", root); Anchor(body, new Vector2(0.20f, 0.04f), new Vector2(0.74f, 0.17f));
            AddImage(body.gameObject, panelColor);
            var bodyH = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            bodyH.spacing = 6; bodyH.childAlignment = TextAnchor.MiddleCenter;
            bodyH.childControlWidth = bodyH.childControlHeight = true; bodyH.childForceExpandWidth = false;

            // Stat footer (HP / DAMAGE / ARMOR) below the preview.
            var footer = NewRect("StatFooter", root); Anchor(footer, new Vector2(0.23f, 0.18f), new Vector2(0.59f, 0.31f));
            AddImage(footer.gameObject, panelColor);
            var footH = footer.gameObject.AddComponent<HorizontalLayoutGroup>();
            footH.childAlignment = TextAnchor.MiddleCenter; footH.spacing = 4;
            footH.childControlWidth = footH.childControlHeight = true; footH.childForceExpandWidth = true;
            var hp = FooterText("HPText", footer, "HP\n—");
            var dmg = FooterText("DamageText", footer, "DAMAGE\n—");
            var arm = FooterText("ArmorText", footer, "ARMOR\n—");

            // Buttons: Stats (under footer), Close (top-left), Gear/Body tabs (above the bag).
            var stats = Button("StatsButton", root, "STATS", btnColor, new Vector2(0.23f, 0.32f), new Vector2(0.40f, 0.39f));
            var close = Button("CloseButton", root, "CLOSE", btnColor, new Vector2(0.01f, 0.93f), new Vector2(0.11f, 0.99f));
            var gearTab = Button("GearTab", root, "Gear", btnColor, new Vector2(0.76f, 0.92f), new Vector2(0.875f, 0.99f));
            var bodyTab = Button("BodyTab", root, "Body", btnColor, new Vector2(0.885f, 0.92f), new Vector2(0.99f, 0.99f));

            // Bag scroll view (right side): Bag(ScrollRect) → Viewport(mask) → Content(grid).
            var bag = NewRect("Bag", root); Anchor(bag, new Vector2(0.76f, 0.16f), new Vector2(0.99f, 0.90f));
            AddImage(bag.gameObject, panelColor);
            var scroll = bag.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = NewRect("Viewport", bag); Stretch(viewport);
            var vpImg = AddImage(viewport.gameObject, new Color(0f, 0f, 0f, 0.001f));
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = NewRect("BagContent", viewport);
            content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f); content.offsetMin = new Vector2(0f, content.offsetMin.y); content.offsetMax = Vector2.zero;
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(82, 82); grid.spacing = new Vector2(6, 6); grid.padding = new RectOffset(6, 6, 6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport; scroll.content = content;

            // Theme applier on the panel.
            // Assign the palette but DON'T let the applier recolor the panel's OWN background. Historically the
            // root Image was kept transparent (line ~52) so the IN-WORLD preview showed through, and
            // colorOwnBackground=true buried it (the "preview doesn't show up" bug, ADR-032). With the preview now
            // rendered to a RawImage via the rig (ADR-034) the see-through is no longer required, but we keep the
            // root transparent + colorOwnBackground=false here too — making the panel opaque is deferred polish
            // (see ADR-034). Children (buttons/text) are still themed.
            var applier = panel.GetComponent<UIThemeApplier>() ?? panel.AddComponent<UIThemeApplier>();
            {
                var aso = new SerializedObject(applier);
                if (palette != null) { var pp = aso.FindProperty("palette"); if (pp != null) pp.objectReferenceValue = palette; }
                var cob = aso.FindProperty("colorOwnBackground");
                if (cob != null) cob.boolValue = false;
                aso.ApplyModifiedPropertiesWithoutUndo();
            }

            // Inspection panel: keep existing, else find one in the scene.
            var inspProp = so.FindProperty("inspectionPanel");
            if (inspProp.objectReferenceValue == null)
            {
                var insp = Object.FindObjectsByType<UnitInspectionPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (insp.Length > 0) inspProp.objectReferenceValue = insp[0];
            }

            // --- wire everything ---
            if (previewAnchor != null) Set(so, "previewAnchor", previewAnchor);
            Set(so, "leftColumnRoot", left);
            Set(so, "rightColumnRoot", right);
            Set(so, "bodyRoot", body);
            Set(so, "bagContentRoot", content);
            Set(so, "hpText", hp);
            Set(so, "damageText", dmg);
            Set(so, "armorText", arm);
            Set(so, "statsButton", stats);
            Set(so, "bagGearTabButton", gearTab);
            Set(so, "bagBodyTabButton", bodyTab);
            Set(so, "closeButton", close);
            if (palette != null) so.FindProperty("palette").objectReferenceValue = palette;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(cs);
            EditorSceneManager.MarkSceneDirty(cs.gameObject.scene);
            AssetDatabase.SaveAssets(); // flush the RenderTexture asset + any new TagManager layer
            Debug.Log("PaperDollBuilder: built + wired the paper-doll panel + preview rig (ADR-034). " +
                      "Review layout/framing, then SAVE THE SCENE.");
            Selection.activeGameObject = panel;
        }

        // ---- helpers -------------------------------------------------------------------------

        private static void Set(SerializedObject so, string field, Component value)
        {
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"PaperDollBuilder: field '{field}' not found."); return; }
            p.objectReferenceValue = value;
        }

        // ---- preview rig (ADR-034) -----------------------------------------------------------

        // Ensure a user layer named PreviewLayerName exists; return its index, or -1 if one can't be
        // claimed (caller then skips the rig and keeps the in-world preview — never falls back to Default,
        // which would make the preview camera render the whole map and blank the main camera).
        private static int EnsurePreviewLayer()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("PaperDollBuilder: couldn't load TagManager — skipping the preview rig (in-world preview kept).");
                return -1;
            }
            var tm = new SerializedObject(assets[0]);
            var layers = tm.FindProperty("layers");
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == PreviewLayerName) return i;
            for (int i = 8; i < layers.arraySize; i++)
            {
                var el = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(el.stringValue))
                {
                    el.stringValue = PreviewLayerName;
                    tm.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log($"PaperDollBuilder: created layer '{PreviewLayerName}' at index {i}.");
                    return i;
                }
            }
            Debug.LogWarning("PaperDollBuilder: no free user layer (8-31) for the preview — skipping the rig (in-world preview kept). Free one and re-run.");
            return -1;
        }

        private static RenderTexture EnsurePreviewRenderTexture()
        {
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(PreviewRtPath);
            if (rt != null) return rt;

            if (!AssetDatabase.IsValidFolder("Assets/Data/Customization"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder("Assets/Data", "Customization");
            }
            rt = new RenderTexture(512, 768, 16, RenderTextureFormat.ARGB32)
            {
                name = "CustomizationPreviewRT",
                antiAliasing = 2,
            };
            AssetDatabase.CreateAsset(rt, PreviewRtPath);
            Debug.Log($"PaperDollBuilder: created preview RenderTexture at {PreviewRtPath}.");
            return rt;
        }

        // (Re)build the off-map preview rig: an anchor (where the unit spawns) + a camera that renders only
        // the preview layer into the RT. Idempotent. Returns the anchor transform to wire into the screen.
        private static Transform BuildPreviewRig(int previewLayer, RenderTexture rt)
        {
            var prev = GameObject.Find(RigMarker);
            if (prev != null) Object.DestroyImmediate(prev);

            var rig = new GameObject(RigMarker) { layer = previewLayer };
            // Parked far from the map; the camera's culling mask is what really isolates it.
            rig.transform.position = new Vector3(0f, 1000f, 0f);

            var anchor = new GameObject("PreviewAnchor") { layer = previewLayer };
            anchor.transform.SetParent(rig.transform, false);

            var camGo = new GameObject("PreviewCamera") { layer = previewLayer };
            camGo.transform.SetParent(rig.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.cullingMask = 1 << previewLayer;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.10f, 1f); // matches the dark theme panel
            cam.orthographic = false;
            cam.fieldOfView = 30f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 50f;
            cam.targetTexture = rt;
            // Front-on, slightly raised; tune by eye for the unit's height/scale.
            camGo.transform.localPosition = new Vector3(0f, 1.2f, -3.5f);
            camGo.transform.LookAt(anchor.transform.position + Vector3.up * 1.0f);

            // Keep the preview unit out of the gameplay (main) camera.
            var main = Camera.main;
            if (main != null) main.cullingMask &= ~(1 << previewLayer);
            else Debug.LogWarning("PaperDollBuilder: no Camera.main — manually clear the 'PreviewUnit' layer from the map camera's Culling Mask.");

            return anchor.transform;
        }

        // Test helper: open the paper-doll for the first party unit, bypassing the in-scene button
        // navigation. Requires Play mode + an active run (draft a party + Start Run first).
        [MenuItem("Tools/Paper-Doll/TEST - Open First Party Unit (Play)")]
        public static void TestOpenFirstUnit()
        {
            if (!Application.isPlaying) { Debug.LogError("PaperDollBuilder: enter Play mode + start a run first."); return; }
            var cs = Object.FindFirstObjectByType<CustomizationScreen>(FindObjectsInactive.Include);
            if (cs == null) { Debug.LogError("PaperDollBuilder: no CustomizationScreen in the scene."); return; }
            if (!CapsuleWars.Run.RunSession.IsActive || CapsuleWars.Run.RunSession.Current.Party.Count == 0)
            {
                Debug.LogError("PaperDollBuilder: no active run / empty party — draft a party + Start Run first.");
                return;
            }
            cs.Show(CapsuleWars.Run.RunSession.Current.Party[0].Id);
            Debug.Log("PaperDollBuilder: opened the paper-doll for the first party unit.");
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min; rt.anchorMax = max;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static RectTransform Column(string name, Transform parent, Vector2 min, Vector2 max, Color bg)
        {
            var rt = NewRect(name, parent); Anchor(rt, min, max);
            AddImage(rt.gameObject, bg);
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 6; v.padding = new RectOffset(4, 4, 6, 6);
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            return rt;
        }

        private static Image AddImage(GameObject go, Color c)
        {
            var img = go.AddComponent<Image>();
            img.color = c;
            return img;
        }

        private static Text FooterText(string name, Transform parent, string text)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>(Font);
            t.alignment = TextAnchor.MiddleCenter; t.fontSize = 16; t.text = text;
            t.color = Color.white; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private static Button Button(string name, Transform parent, string label, Color bg, Vector2 min, Vector2 max)
        {
            var rt = NewRect(name, parent); Anchor(rt, min, max);
            var img = AddImage(rt.gameObject, bg);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var lrt = NewRect("Text", rt); Stretch(lrt);
            var t = lrt.gameObject.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>(Font);
            t.alignment = TextAnchor.MiddleCenter; t.fontSize = 14; t.text = label; t.color = Color.white;
            t.raycastTarget = false;
            return b;
        }
    }
}
