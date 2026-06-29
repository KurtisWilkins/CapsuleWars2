using System.Collections.Generic;
using System.Linq;
using CapsuleWars.Core;
using CapsuleWars.Data.Units;
using CapsuleWars.Units.Customization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CapsuleWars.UI.Customization
{
    /// <summary>
    /// Standalone "Unit Builder" SHOWROOM. The unit is framed on the right (full body); the left is an
    /// inventory picker — slot tabs, then a scrollable GRID OF PART ICONS for the selected slot, plus a
    /// PER-PART color row (recolors the selected slot's part). Click an icon to swap a part live; click a
    /// swatch to recolor that part. Pure sandbox: no run, no party, no save — nothing carries into gameplay
    /// (locked parts previewable but flagged). Coat-pattern textures pend the region-tint/pattern shader.
    /// Self-contained: builds its own camera, light, EventSystem (Input System module) and canvas at runtime.
    /// </summary>
    public class UnitBuilderSandbox : MonoBehaviour
    {
        [SerializeField] private GameObject previewPrefab;
        [SerializeField] private PartCatalog_SO catalog;
        [SerializeField] private Material patternMaterial;
        [SerializeField] private CoatPreset_SO[] presets;

        private UnitCustomization preview;
        private Transform previewRoot;
        private readonly Dictionary<PartSlot, BodyPart_SO> loadout = new();
        private readonly Dictionary<PartSlot, Color> partColors = new();
        private readonly Dictionary<PartSlot, int> partPatterns = new();
        private readonly Dictionary<PartSlot, Color> partSecondary = new();
        private readonly Dictionary<PartSlot, Color> partEyeColor = new();
        private bool markingMode;
        private float patternFreq = 9f;
        private readonly List<(bool marking, Image bg)> modeBtns = new();
        private HashSet<string> starterIds;
        private Font font;

        private PartSlot currentSlot;
        private Transform gridContent;
        private Text colorTargetLabel;
        private readonly List<(PartSlot slot, Button tab, Image bg)> slotTabs = new();
        private readonly List<(BodyPart_SO part, Image bg)> gridButtons = new();

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int PrimaryId = Shader.PropertyToID("_Primary");
        private static readonly int SecondaryId = Shader.PropertyToID("_Secondary");
        private static readonly int PatternId = Shader.PropertyToID("_Pattern");
        private static readonly int FreqId = Shader.PropertyToID("_Freq");
        private static readonly int EyeOnId = Shader.PropertyToID("_EyeOn");
        private static readonly int EyeColorId = Shader.PropertyToID("_EyeColor");
        private static readonly int EyeOffsetId = Shader.PropertyToID("_EyeOffset");
        private static readonly int EyeRadiusId = Shader.PropertyToID("_EyeRadius");
        private static readonly Color PanelBg = new Color(0.09f, 0.10f, 0.13f, 0.94f);
        private static readonly Color CellBg = new Color(0.20f, 0.22f, 0.27f, 1f);
        private static readonly Color CellSel = new Color(0.28f, 0.55f, 0.88f, 1f);
        private static readonly Color CellLocked = new Color(0.30f, 0.18f, 0.18f, 1f);
        private static readonly Color TabBg = new Color(0.18f, 0.20f, 0.25f, 1f);
        private static readonly Color TabSel = new Color(0.28f, 0.55f, 0.88f, 1f);

        private void Start()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            starterIds = catalog != null ? new HashSet<string>(catalog.StarterPartIds()) : new HashSet<string>();
            // Robust: build the pattern material at runtime so patterns work even if the serialized
            // reference didn't survive entering Play.
            if (patternMaterial == null)
            {
                var sh = Shader.Find("CapsuleWars/ProceduralPattern");
                if (sh != null) patternMaterial = new Material(sh);
            }
            BuildStage();
            SpawnPreview();
            BuildUI();
            if (preview != null) SelectSlot(preview.MountedSlots.Distinct().FirstOrDefault());
            Reapply();
        }

        private void Update()
        {
            if (previewRoot != null) previewRoot.Rotate(0f, 16f * Time.deltaTime, 0f, Space.World);
        }

        private void BuildStage()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("BuilderCamera", typeof(Camera)) { tag = "MainCamera" };
                cam = camGo.GetComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.14f, 0.15f, 0.18f);
            cam.fieldOfView = 36f;
            cam.transform.position = new Vector3(-1.35f, 1.45f, -5.5f);
            cam.transform.rotation = Quaternion.Euler(2f, 0f, 0f);

            var lg = new GameObject("BuilderLight", typeof(Light));
            var li = lg.GetComponent<Light>();
            li.type = LightType.Directional; li.intensity = 1.25f;
            lg.transform.rotation = Quaternion.Euler(42f, -24f, 0f);

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                var moduleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (moduleType != null) es.AddComponent(moduleType);
                else es.AddComponent<StandaloneInputModule>();
            }
        }

        private void SpawnPreview()
        {
            if (previewPrefab == null) { Debug.LogError("UnitBuilderSandbox: previewPrefab not set."); return; }
            var go = Instantiate(previewPrefab, Vector3.zero, Quaternion.identity);
            previewRoot = go.transform;
            foreach (var b in go.GetComponentsInChildren<Behaviour>(true))
            {
                if (b == null || b is Animator || b is UnitCustomization) continue;
                string n = b.GetType().Name;
                if (n == "NavMeshAgent" || n.EndsWith("Controller") || n.Contains("Ability")
                    || n.Contains("Combat") || n.Contains("Spawner") || n.Contains("Brain") || n.Contains("Health"))
                    b.enabled = false;
            }
            preview = go.GetComponentInChildren<UnitCustomization>();
            if (preview != null && preview.Definition != null)
                foreach (var pa in preview.Definition.Parts)
                    if (pa.part != null) loadout[pa.slot] = pa.part;
        }

        // Apply parts, then push each mount's per-part color via MaterialPropertyBlock (grayscale × tint).
        private void Reapply()
        {
            if (preview == null) return;
            var list = new List<PartAssignment>();
            foreach (var kv in loadout)
                if (kv.Value != null) list.Add(new PartAssignment { slot = kv.Key, part = kv.Value });
            preview.ApplyParts(list, null);  // restores each mount's default grayscale material + mesh

            foreach (var mount in preview.Mounts)
            {
                if (mount.Renderer == null) continue;
                loadout.TryGetValue(mount.Slot, out var part);
                Color primary = partColors.TryGetValue(mount.Slot, out var pc) ? pc : Color.white;
                int pat = partPatterns.TryGetValue(mount.Slot, out var pp) ? pp : 0;
                bool hasEyes = part != null && part.HasEyes;
                var mpb = new MaterialPropertyBlock();
                if ((pat > 0 || hasEyes) && patternMaterial != null)
                {
                    mount.Renderer.sharedMaterial = patternMaterial;
                    var gray = (part != null && part.DefaultMaterials != null && part.DefaultMaterials.Count > 0)
                        ? part.DefaultMaterials[0].GetTexture("_BaseMap") : null;
                    if (gray != null) mpb.SetTexture(BaseMapId, gray);
                    Color sec = partSecondary.TryGetValue(mount.Slot, out var sc) ? sc : new Color(0.10f, 0.08f, 0.06f);
                    mpb.SetColor(PrimaryId, primary);
                    mpb.SetColor(SecondaryId, sec);
                    mpb.SetFloat(PatternId, pat);
                    mpb.SetFloat(FreqId, patternFreq);
                    if (hasEyes)
                    {
                        Color eye = partEyeColor.TryGetValue(mount.Slot, out var ec) ? ec : part.EyeColor;
                        mpb.SetFloat(EyeOnId, 1f);
                        mpb.SetColor(EyeColorId, eye);
                        mpb.SetVector(EyeOffsetId, part.EyeOffset);
                        mpb.SetFloat(EyeRadiusId, part.EyeRadius);
                    }
                    else mpb.SetFloat(EyeOnId, 0f);
                }
                else
                {
                    mpb.SetColor(BaseColorId, primary);  // solid: tint the grayscale
                }
                mount.Renderer.SetPropertyBlock(mpb);
            }
        }

        // --- UI ---

        private void BuildUI()
        {
            var canvasGo = new GameObject("BuilderCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = Panel(canvas.transform);
            Text(panel, "UNIT BUILDER", 26, FontStyle.Bold, 34);
            Text(panel, "showroom — preview only, nothing here is used in-game until unlocked", 14, FontStyle.Italic, 20);

            if (presets != null && presets.Length > 0)
            {
                Text(panel, "RACE PRESETS — load a big cat", 15, FontStyle.Bold, 22);
                var presetGrid = PresetGrid(panel);
                foreach (var p in presets) if (p != null) PresetBtn(presetGrid, p);
            }

            var tabRow = Bar(panel, 44);
            if (preview != null)
                foreach (var slot in preview.MountedSlots.Distinct())
                    SlotTab(tabRow, slot);

            gridContent = ScrollGrid(panel);

            colorTargetLabel = Text(panel, "COLOR", 16, FontStyle.Bold, 24);
            var modeRow = Bar(panel, 36);
            ColorModeBtn(modeRow, "Base color", false);
            ColorModeBtn(modeRow, "Marking color", true);
            var colorGrid = ColorGrid(panel);
            foreach (var c in BuildPalette()) ColorCell(colorGrid, c);

            Text(panel, "PATTERN (per part — primary = color above, markings dark)", 16, FontStyle.Bold, 22);
            var patRow = Bar(panel, 42);
            PatternBtn(patRow, "Solid", 0);
            PatternBtn(patRow, "Stripes", 1);
            PatternBtn(patRow, "Spots", 2);

            Text(panel, "EYE COLOR (head)", 16, FontStyle.Bold, 22);
            var eyeRow = Bar(panel, 42);
            Color[] eyes = { new Color(0.93f,0.76f,0.18f), new Color(0.95f,0.55f,0.10f), new Color(0.50f,0.80f,0.42f),
                new Color(0.25f,0.65f,0.90f), new Color(0.85f,0.30f,0.25f), new Color(0.70f,0.50f,0.85f),
                new Color(0.96f,0.96f,0.96f), new Color(0.12f,0.12f,0.13f) };
            foreach (var c in eyes) EyeSwatch(eyeRow, c);
        }

        private void EyeSwatch(Transform parent, Color col)
        {
            var go = New("EyeSw", parent);
            Layout(go, 46, 38);
            var bg = go.AddComponent<Image>(); bg.color = col;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            btn.onClick.AddListener(() => { partEyeColor[currentSlot] = col; Reapply(); });
        }

        private Transform PresetGrid(Transform parent)
        {
            var go = New("PresetGrid", parent);
            Layout(go, 0, 76);
            var grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(116, 32); grid.spacing = new Vector2(6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 6;
            return go.transform;
        }

        private void PresetBtn(Transform parent, CoatPreset_SO p)
        {
            var go = New("Preset", parent);
            var bg = go.AddComponent<Image>(); bg.color = TabBg;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            btn.onClick.AddListener(() => LoadPreset(p));
            var t = New("T", go.transform); Stretch((RectTransform)t.transform);
            var txt = t.AddComponent<Text>();
            txt.font = font; txt.text = p.DisplayName; txt.fontSize = 13; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
        }

        // One-click "become this race": apply the preset's parts + coat (primary, marking, pattern, eye colour).
        private void LoadPreset(CoatPreset_SO p)
        {
            if (p == null || preview == null) return;
            loadout.Clear();
            foreach (var part in p.Parts) if (part != null) loadout[part.Slot] = part;
            partColors.Clear(); partSecondary.Clear(); partPatterns.Clear(); partEyeColor.Clear();
            patternFreq = p.PatternFrequency > 0f ? p.PatternFrequency : 9f;
            foreach (var slot in preview.MountedSlots.Distinct())
            {
                partColors[slot] = p.Primary;
                partSecondary[slot] = p.Secondary;
            }
            partPatterns[PartSlot.Body] = p.Pattern;
            partPatterns[PartSlot.HeadProp] = p.Pattern;
            partEyeColor[PartSlot.HeadProp] = p.EyeColor;
            SelectSlot(currentSlot);
            Reapply();
        }

        private void PatternBtn(Transform parent, string label, int pat)
        {
            var go = New("Pat", parent);
            Layout(go, 120, 40);
            var bg = go.AddComponent<Image>(); bg.color = TabBg;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            btn.onClick.AddListener(() => { partPatterns[currentSlot] = pat; Reapply(); });
            var t = New("T", go.transform); Stretch((RectTransform)t.transform);
            var txt = t.AddComponent<Text>();
            txt.font = font; txt.text = label; txt.fontSize = 15; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
        }

        private void SelectSlot(PartSlot slot)
        {
            currentSlot = slot;
            foreach (var (s, tab, bg) in slotTabs) bg.color = s == slot ? TabSel : TabBg;
            UpdateColorLabel();

            gridButtons.Clear();
            for (int i = gridContent.childCount - 1; i >= 0; i--) Destroy(gridContent.GetChild(i).gameObject);
            Cell(gridContent, null, "(none)");
            if (catalog != null)
                foreach (var part in catalog.PartsForSlot(slot))
                    Cell(gridContent, part, ShortName(part));
            RefreshGrid();
        }

        private void OnPick(BodyPart_SO part)
        {
            if (part == null) loadout.Remove(currentSlot); else loadout[currentSlot] = part;
            Reapply();
            RefreshGrid();
        }

        private void OnColor(Color c)
        {
            if (markingMode) partSecondary[currentSlot] = c; else partColors[currentSlot] = c;
            Reapply();
        }

        private void UpdateColorLabel()
        {
            if (colorTargetLabel != null)
                colorTargetLabel.text = "COLOR — " + (markingMode ? "MARKING" : "base") + " of " + TabLabel(currentSlot);
        }

        private void ColorModeBtn(Transform parent, string label, bool marking)
        {
            var go = New("Mode", parent);
            Layout(go, 160, 34);
            var bg = go.AddComponent<Image>(); bg.color = marking == markingMode ? TabSel : TabBg;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                markingMode = marking;
                foreach (var m in modeBtns) m.bg.color = m.marking == markingMode ? TabSel : TabBg;
                UpdateColorLabel();
            });
            var t = New("T", go.transform); Stretch((RectTransform)t.transform);
            var txt = t.AddComponent<Text>();
            txt.font = font; txt.text = label; txt.fontSize = 14; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
            modeBtns.Add((marking, bg));
        }

        private void RefreshGrid()
        {
            loadout.TryGetValue(currentSlot, out var cur);
            foreach (var (part, bg) in gridButtons)
            {
                bool selected = cur == part;
                bool locked = part != null && !starterIds.Contains(part.PartId);
                bg.color = selected ? CellSel : (locked ? CellLocked : CellBg);
            }
        }

        private string ShortName(BodyPart_SO p)
        {
            string n = p != null ? p.name : "?";
            if (n.StartsWith("Felid_Base_")) n = n.Substring("Felid_Base_".Length);
            return n;
        }

        // --- uGUI builders ---

        private Transform Panel(Transform parent)
        {
            var go = New("InventoryPanel", parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(760, -48);
            rt.anchoredPosition = new Vector2(24, 0);
            go.AddComponent<Image>().color = PanelBg;
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(18, 18, 18, 18); vlg.spacing = 8;
            vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true; vlg.childForceExpandHeight = false;
            return go.transform;
        }

        private Text Text(Transform parent, string text, int size, FontStyle style, float h)
        {
            var go = New("Text", parent);
            Layout(go, 0, h);
            var t = go.AddComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.fontStyle = style;
            t.alignment = TextAnchor.MiddleLeft; t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private Transform Bar(Transform parent, float h)
        {
            var go = New("Bar", parent);
            Layout(go, 0, h);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6; hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true; hlg.childForceExpandWidth = false;
            hlg.childControlHeight = true; hlg.childForceExpandHeight = true;
            return go.transform;
        }

        private void SlotTab(Transform parent, PartSlot slot)
        {
            var go = New("Tab", parent);
            Layout(go, 112, 40);
            var bg = go.AddComponent<Image>(); bg.color = TabBg;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            btn.onClick.AddListener(() => SelectSlot(slot));
            var t = New("T", go.transform); Stretch((RectTransform)t.transform);
            var txt = t.AddComponent<Text>();
            txt.font = font; txt.text = TabLabel(slot); txt.fontSize = 15; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
            slotTabs.Add((slot, btn, bg));
        }

        private Transform ScrollGrid(Transform parent)
        {
            var scrollGo = New("Grid", parent);
            Layout(scrollGo, 0, 300, expandHeight: false);
            scrollGo.AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 24;

            var viewport = New("Viewport", scrollGo.transform);
            Stretch((RectTransform)viewport.transform);
            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = (RectTransform)viewport.transform;

            var content = New("Content", viewport.transform);
            var crt = (RectTransform)content.transform;
            crt.anchorMin = new Vector2(0, 1); crt.anchorMax = new Vector2(1, 1); crt.pivot = new Vector2(0.5f, 1);
            crt.anchoredPosition = Vector2.zero;
            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(104, 104); grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 6;
            var fit = content.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = crt;
            return content.transform;
        }

        private void Cell(Transform parent, BodyPart_SO part, string label)
        {
            var go = New("Cell", parent);
            var bg = go.AddComponent<Image>(); bg.color = CellBg;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            btn.onClick.AddListener(() => OnPick(part));

            if (part != null && part.Icon != null)
            {
                var ic = New("Icon", go.transform);
                var irt = (RectTransform)ic.transform;
                irt.anchorMin = new Vector2(0, 0.18f); irt.anchorMax = new Vector2(1, 1);
                irt.offsetMin = new Vector2(6, 0); irt.offsetMax = new Vector2(-6, -6);
                var img = ic.AddComponent<Image>(); img.sprite = part.Icon; img.preserveAspect = true;
            }
            var cap = New("Cap", go.transform);
            var crt = (RectTransform)cap.transform;
            crt.anchorMin = new Vector2(0, 0); crt.anchorMax = new Vector2(1, part != null && part.Icon != null ? 0.22f : 1f);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
            var t = cap.AddComponent<Text>();
            t.font = font; t.text = label; t.fontSize = 12; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Truncate;
            gridButtons.Add((part, bg));
        }

        private static List<Color> BuildPalette()
        {
            var cols = new List<Color>
            {
                Color.white, new Color(0.75f,0.75f,0.77f), new Color(0.45f,0.45f,0.47f),
                new Color(0.18f,0.18f,0.20f), new Color(0.07f,0.07f,0.08f),
                new Color(0.90f,0.83f,0.62f), new Color(0.86f,0.78f,0.55f), new Color(0.80f,0.58f,0.34f),
                new Color(0.55f,0.38f,0.22f), new Color(0.38f,0.25f,0.15f)
            };
            for (int v = 0; v < 2; v++)
            {
                float val = v == 0 ? 0.90f : 0.58f;
                float sat = v == 0 ? 0.60f : 0.80f;
                for (int h = 0; h < 12; h++) cols.Add(Color.HSVToRGB(h / 12f, sat, val));
            }
            return cols;
        }

        private Transform ColorGrid(Transform parent)
        {
            var go = New("ColorGrid", parent);
            Layout(go, 0, 150);
            var grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(40, 40); grid.spacing = new Vector2(6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 14;
            return go.transform;
        }

        private void ColorCell(Transform parent, Color col)
        {
            var go = New("Sw", parent);
            var bg = go.AddComponent<Image>(); bg.color = col;
            var btn = go.AddComponent<Button>(); btn.targetGraphic = bg;
            btn.onClick.AddListener(() => OnColor(col));
        }

        private GameObject New(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private LayoutElement Layout(GameObject go, float prefW, float prefH, bool expandHeight = false)
        {
            var le = go.AddComponent<LayoutElement>();
            if (prefW > 0) le.preferredWidth = prefW;
            le.preferredHeight = prefH; le.minHeight = prefH;
            if (expandHeight) le.flexibleHeight = 1;
            return le;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static string TabLabel(PartSlot slot)
        {
            switch (slot)
            {
                case PartSlot.LeftHand: return "L.Hand";
                case PartSlot.RightHand: return "R.Hand";
                case PartSlot.LeftFoot: return "L.Foot";
                case PartSlot.RightFoot: return "R.Foot";
                case PartSlot.HeadProp: return "Head";
                default: return slot.ToString();
            }
        }
    }
}
