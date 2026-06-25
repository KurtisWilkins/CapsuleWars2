using CapsuleWars.Combat.Deployment;
using CapsuleWars.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CapsuleWars.UI.CameraControl
{
    /// <summary>
    /// Pans and zooms the battle camera during deployment AND combat (TFT-style free camera).
    /// Pan: WASD / arrow keys, mouse drag (configurable button), and single-finger touch drag.
    /// Zoom: mouse scroll / two-finger pinch, moving along the camera's view direction. Movement is
    /// clamped to a world-space XZ rectangle and a camera-height range. Control during battle is gated
    /// by <see cref="allowControlDuringBattle"/> (default on); the camera auto-frames the board on
    /// deployment start and eases to a ~45° battle view on Assemble. Control is disabled only during
    /// the transition lerp.
    ///
    /// Uses the new Input System low-level device APIs (Mouse / Keyboard /
    /// Touchscreen .current) directly, so no .inputactions asset wiring is needed;
    /// Active Input Handling in this project is "Input System Package" only, so the
    /// legacy UnityEngine.Input API is unavailable.
    ///
    /// Attach to the battle camera (or a camera rig). Bounds, speeds, tilts and the zoom range are
    /// designer-tunable; verify the feel in Play mode. In the editor, tweak a knob then press F5
    /// (re-frame deployment) / F6 (re-frame battle), or use the component's right-click context menu.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DeploymentCameraController : MonoBehaviour
    {
        public enum DragButton { Left, Right, Middle }

        [Header("Pan")]
        [Tooltip("Keyboard pan speed in world units per second.")]
        [SerializeField, Min(0f)] private float keyboardPanSpeed = 12f;
        [Tooltip("Drag pan speed in world units per screen pixel.")]
        [SerializeField, Min(0f)] private float dragPanSpeed = 0.04f;
        [SerializeField] private DragButton dragButton = DragButton.Right;

        [Header("Zoom (along the camera's view, clamped by height)")]
        [SerializeField, Min(0f)] private float scrollZoomSpeed = 1.5f;
        [SerializeField, Min(0f)] private float pinchZoomSpeed = 0.03f;
        [Tooltip("Lowest camera height (zoom-in limit). The view-direction zoom is clamped against this.")]
        [SerializeField] private float minHeight = 3f;
        [Tooltip("Highest camera height (zoom-out limit).")]
        [SerializeField] private float maxHeight = 50f;

        [Header("World bounds (XZ)")]
        [Tooltip("Minimum world X (x) and Z (y) the camera position is clamped to.")]
        [SerializeField] private Vector2 boundsMin = new Vector2(-15f, -25f);
        [Tooltip("Maximum world X (x) and Z (y) the camera position is clamped to.")]
        [SerializeField] private Vector2 boundsMax = new Vector2(40f, 45f);

        [Header("Gating")]
        [Tooltip("When true, the camera only moves during BattlePhase.PreBattle UNLESS allowControlDuringBattle is on.")]
        [SerializeField] private bool restrictToDeployment = true;
        [Tooltip("When true, pan + zoom also work DURING battle (TFT-style free camera), not just deployment. " +
                 "Overrides restrictToDeployment for the combat phase; control is still disabled during the transition lerp.")]
        [SerializeField] private bool allowControlDuringBattle = true;

        [Header("Auto-frame (deployment)")]
        [Tooltip("On deployment start, lerp to a pose that frames the whole board; ease to the battle pose on Assemble.")]
        [SerializeField] private bool autoFrameOnDeployment = true;
        [Tooltip("When true, the framing pose is COMPUTED from the deployment grid (always fits, follows cellSize/row " +
                 "changes). Falls back to the manual pose below if no DeploymentManager is found.")]
        [SerializeField] private bool computeFramingFromGrid = true;
        [Tooltip("Pitch (deg) for the computed deployment frame. 90 = straight top-down; steeper (toward 90) spreads " +
                 "the near player rows up the screen instead of compressing them into the bottom HUD band. ~84 balances " +
                 "clearing the HUD against keeping some depth.")]
        [SerializeField, Range(40f, 90f)] private float deploymentTiltDegrees = 84f;
        [Tooltip("Fallback camera position (used only when not computing from the grid).")]
        [SerializeField] private Vector3 deploymentPosition = new Vector3(4.5f, 14f, 6f);
        [Tooltip("Fallback camera euler rotation (used only when not computing from the grid).")]
        [SerializeField] private Vector3 deploymentEuler = new Vector3(70f, 0f, 0f);
        [Tooltip("Field of view during deployment (0 = keep current). Also used as the FOV for the computed frame.")]
        [SerializeField] private float deploymentFov = 0f;
        [Tooltip("Fraction of screen height to leave clear at the BOTTOM for the deployment HUD, so the player rows " +
                 "aren't hidden behind it. The HUD is a 230px band in a 720x1280 CanvasScaler (match 0.5) ≈ 0.18 of the " +
                 "reference but up to ~0.32 on a wide/landscape view, so default 0.30 covers the worst case.")]
        [SerializeField, Range(0f, 0.45f)] private float bottomViewportInset = 0.30f;
        [Tooltip("Extra world-space nudge added to the computed DEPLOYMENT framing position, for fine-tuning what's " +
                 "on screen (e.g. z+ moves the camera toward the board to recenter it). Not applied to the battle frame.")]
        [SerializeField] private Vector3 framingOffset = new Vector3(0f, 0f, 6f);
        [Tooltip("Seconds for the camera transition in/out of the deployment framing.")]
        [SerializeField, Min(0.01f)] private float transitionSeconds = 0.6f;

        [Header("Auto-frame (battle, on Assemble)")]
        [Tooltip("On Assemble, ease to a board-framing pose at this pitch (45 ≈ Teamfight-Tactics view) instead of " +
                 "snapping to the authored scene pose.")]
        [SerializeField, Range(20f, 80f)] private float battleTiltDegrees = 45f;
        [Tooltip("Field of view for the battle frame (0 = keep current).")]
        [SerializeField] private float battleFov = 0f;
        [Tooltip("When true, the battle pose is COMPUTED from the grid at battleTiltDegrees; falls back to the " +
                 "authored scene pose (captured in Awake) when no grid is found.")]
        [SerializeField] private bool computeBattleFromGrid = true;

        private bool dragging;
        private Vector2 lastDragPos;
        private bool pinching;
        private float lastPinchDist;

        private Camera cam;
        private DeploymentPhaseController phase;
        private DeploymentManager gridSource;
        private Vector3 authoredPosition;
        private Quaternion authoredRotation;
        private float authoredFov;
        private bool transitioning;
        private Vector3 fromPos, targetPos;
        private Quaternion fromRot, targetRot;
        private float fromFov, targetFov;
        private float transitionT;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            authoredPosition = transform.position;   // authored scene pose — fallback battle frame if no grid
            authoredRotation = transform.rotation;
            authoredFov = cam != null ? cam.fieldOfView : 60f;

            phase = FindAnyObjectByType<DeploymentPhaseController>();
            if (phase != null) phase.OnConfirmed += FrameBattle;
            gridSource = FindAnyObjectByType<DeploymentManager>();
        }

        private void Start()
        {
            if (autoFrameOnDeployment && phase != null) FrameDeployment();
        }

        private void OnDestroy()
        {
            if (phase != null) phase.OnConfirmed -= FrameBattle;
        }

        /// <summary>Lerp to a pose that frames the whole board for deployment (computed from the grid when possible).</summary>
        public void FrameDeployment()
        {
            if (computeFramingFromGrid &&
                TryComputeBoardFraming(deploymentTiltDegrees, bottomViewportInset, deploymentFov, framingOffset, out var pos, out var rot, out var fov))
                BeginTransition(pos, rot, fov);
            else
                BeginTransition(deploymentPosition, Quaternion.Euler(deploymentEuler), deploymentFov);
        }

        /// <summary>
        /// Ease to the battle view (called on Assemble): a board-framing pose computed from the grid at
        /// <see cref="battleTiltDegrees"/> (≈45° TFT-style), or the authored scene pose as a fallback.
        /// No bottom inset — the deployment HUD hides on Assemble, so the board can use the full view.
        /// </summary>
        public void FrameBattle()
        {
            if (computeBattleFromGrid &&
                TryComputeBoardFraming(battleTiltDegrees, 0f, battleFov, Vector3.zero, out var pos, out var rot, out var fov))
                BeginTransition(pos, rot, fov);
            else
                BeginTransition(authoredPosition, authoredRotation, authoredFov);
        }

        /// <summary>
        /// Compute a pose that frames the whole grid at the given pitch, derived from the deployment
        /// grid's size so it always fits and follows cellSize/row changes. Distance is the larger of the
        /// width-fit and depth-fit (so the board fits both screen axes for the current aspect); the camera
        /// sits above and behind the player's near side. <paramref name="bottomInset"/> biases the board
        /// up to leave a clear band at the bottom (for the deployment HUD; pass 0 for battle). Returns
        /// false if no grid source is available.
        /// </summary>
        private bool TryComputeBoardFraming(float tiltDegrees, float bottomInset, float fovParam, Vector3 offset,
                                            out Vector3 pos, out Quaternion rot, out float fov)
        {
            pos = default; rot = Quaternion.identity; fov = 0f;
            if (gridSource == null || gridSource.Config == null) return false;

            var cfg = gridSource.Config;
            float width = cfg.columns * cfg.cellSize;   // X
            float depth = cfg.rows * cfg.cellSize;      // Z
            Vector3 center = cfg.origin + new Vector3((cfg.columns - 1) * 0.5f * cfg.cellSize, 0f,
                                                      (cfg.rows - 1) * 0.5f * cfg.cellSize);

            fov = fovParam > 0f ? fovParam : (cam != null ? cam.fieldOfView : 60f);
            float aspect = cam != null && cam.aspect > 0.01f ? cam.aspect : (9f / 16f);
            float vHalf = fov * 0.5f * Mathf.Deg2Rad;
            float hHalf = Mathf.Atan(Mathf.Tan(vHalf) * aspect);
            float margin = cfg.cellSize;                // ~one cell of padding

            float distW = (width * 0.5f + margin) / Mathf.Max(0.01f, Mathf.Tan(hHalf));
            float distD = (depth * 0.5f + margin) / Mathf.Max(0.01f, Mathf.Tan(vHalf));
            float dist = Mathf.Max(distW, distD);

            // Bias the whole rig toward the player (near) side so the board sits in the
            // UPPER part of the screen, leaving a clear band at the bottom for the HUD.
            center.z -= depth * bottomInset * 0.5f;

            float tilt = tiltDegrees * Mathf.Deg2Rad;
            pos = center + new Vector3(0f, dist * Mathf.Sin(tilt), -dist * Mathf.Cos(tilt)) + offset;
            rot = Quaternion.Euler(tiltDegrees, 0f, 0f);
            return true;
        }

        private void BeginTransition(Vector3 pos, Quaternion rot, float fov)
        {
            fromPos = transform.position; targetPos = pos;
            fromRot = transform.rotation; targetRot = rot;
            fromFov = cam != null ? cam.fieldOfView : 60f;
            targetFov = fov > 0f ? fov : fromFov;
            transitionT = 0f;
            transitioning = true;
            dragging = false; pinching = false;
        }

        private void Update()
        {
#if UNITY_EDITOR
            HandleEditorTuningHotkeys();
#endif
            if (transitioning) { AdvanceTransition(); return; }   // no manual control mid-transition

            // Locked only when restricted to deployment AND past PreBattle AND battle control is off.
            if (restrictToDeployment && !allowControlDuringBattle
                && CombatServices.Phase != BattlePhase.PreBattle)
            {
                dragging = false;
                pinching = false;
                return;
            }

            Vector3 move = ReadKeyboardPan() + ReadMouseDragPan() + ReadTouchPan();
            float zoom = ReadZoom();   // > 0 = zoom in (move along the camera's view direction)

            if (move != Vector3.zero || !Mathf.Approximately(zoom, 0f))
            {
                // Zoom along the view direction so it dollies toward/away from the board at any pitch
                // (a 45° battle view zooms into the board, not just down). Clamp keeps it in bounds.
                Vector3 p = transform.position + move + transform.forward * zoom;
                transform.position = Clamp(p);
            }
        }

        private void AdvanceTransition()
        {
            transitionT += Time.deltaTime / Mathf.Max(0.01f, transitionSeconds);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(transitionT));
            transform.position = Vector3.Lerp(fromPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);
            if (cam != null) cam.fieldOfView = Mathf.Lerp(fromFov, targetFov, t);
            if (transitionT >= 1f) transitioning = false;
        }

        /// <summary>
        /// Clamp a candidate position to the world XZ bounds and the zoom-height
        /// range. Pure (no side effects) so it can be reasoned about and unit-tested.
        /// </summary>
        public Vector3 Clamp(Vector3 p)
        {
            p.x = Mathf.Clamp(p.x, boundsMin.x, boundsMax.x);
            p.z = Mathf.Clamp(p.z, boundsMin.y, boundsMax.y);
            p.y = Mathf.Clamp(p.y, minHeight, maxHeight);
            return p;
        }

        // Camera facing projected onto the ground plane, so panning tracks the
        // map regardless of the camera's pitch.
        private Vector3 PlanarRight()
        {
            Vector3 r = transform.right; r.y = 0f;
            return r.sqrMagnitude > 1e-4f ? r.normalized : Vector3.right;
        }

        private Vector3 PlanarForward()
        {
            Vector3 f = transform.forward; f.y = 0f;
            return f.sqrMagnitude > 1e-4f ? f.normalized : Vector3.forward;
        }

        private Vector3 ReadKeyboardPan()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector3.zero;

            float x = 0f, z = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) z -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) z += 1f;
            if (x == 0f && z == 0f) return Vector3.zero;

            Vector3 dir = (PlanarRight() * x + PlanarForward() * z).normalized;
            return dir * (keyboardPanSpeed * Time.deltaTime);
        }

        private Vector3 ReadMouseDragPan()
        {
            var mouse = Mouse.current;
            if (mouse == null) return Vector3.zero;

            var btn = dragButton switch
            {
                DragButton.Left => mouse.leftButton,
                DragButton.Middle => mouse.middleButton,
                _ => mouse.rightButton,
            };
            if (!btn.isPressed) { dragging = false; return Vector3.zero; }

            Vector2 pos = mouse.position.ReadValue();
            if (!dragging) { dragging = true; lastDragPos = pos; return Vector3.zero; }

            Vector2 delta = pos - lastDragPos;
            lastDragPos = pos;
            return PanFromScreenDelta(delta);
        }

        private Vector3 ReadTouchPan()
        {
            var ts = Touchscreen.current;
            if (ts == null) return Vector3.zero;

            Vector2 first = default;
            int count = CountTouches(ts, ref first, ref first);

            if (count == 1)
            {
                if (!dragging) { dragging = true; lastDragPos = first; return Vector3.zero; }
                Vector2 delta = first - lastDragPos;
                lastDragPos = first;
                return PanFromScreenDelta(delta);
            }

            dragging = false; // 0 or 2+ touches: no single-finger pan this frame
            return Vector3.zero;
        }

        private float ReadZoom()
        {
            float delta = 0f;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (!Mathf.Approximately(scroll, 0f))
                    delta += Mathf.Sign(scroll) * scrollZoomSpeed; // scroll up -> move forward (zoom in)
            }

            var ts = Touchscreen.current;
            if (ts != null)
            {
                Vector2 a = default, b = default;
                if (CountTouches(ts, ref a, ref b) >= 2)
                {
                    float dist = Vector2.Distance(a, b);
                    if (pinching) delta += (dist - lastPinchDist) * pinchZoomSpeed; // fingers apart -> zoom in
                    lastPinchDist = dist;
                    pinching = true;
                }
                else
                {
                    pinching = false;
                }
            }

            return delta;
        }

        // Count active touches, writing the first two positions to a/b.
        private static int CountTouches(Touchscreen ts, ref Vector2 a, ref Vector2 b)
        {
            int count = 0;
            var touches = ts.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                if (!touches[i].press.isPressed) continue;
                if (count == 0) a = touches[i].position.ReadValue();
                else if (count == 1) b = touches[i].position.ReadValue();
                count++;
                if (count >= 2) break;
            }
            return count;
        }

        // Dragging moves the world under the cursor, so the camera moves opposite
        // the screen-space drag delta.
        private Vector3 PanFromScreenDelta(Vector2 delta)
        {
            return (PlanarRight() * -delta.x + PlanarForward() * -delta.y) * dragPanSpeed;
        }

#if UNITY_EDITOR
        // ---- Editor-only tuning aids (compiled out of player builds) ----------------------------
        // Tweak the serialized knobs in the Inspector during Play, then re-apply a frame to see the
        // result live without recompiling: right-click the component header, or press F5 / F6.

        [ContextMenu("Re-apply deployment frame")]
        private void EditorReapplyDeploymentFrame() => FrameDeployment();

        [ContextMenu("Re-apply battle frame")]
        private void EditorReapplyBattleFrame() => FrameBattle();

        private void HandleEditorTuningHotkeys()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.f5Key.wasPressedThisFrame) FrameDeployment();
            if (kb.f6Key.wasPressedThisFrame) FrameBattle();
        }
#endif
    }
}
