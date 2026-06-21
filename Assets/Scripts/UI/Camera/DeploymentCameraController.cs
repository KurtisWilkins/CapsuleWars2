using CapsuleWars.Combat.Deployment;
using CapsuleWars.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CapsuleWars.UI.CameraControl
{
    /// <summary>
    /// Pans and zooms the battle camera during deployment (BattlePhase.PreBattle).
    /// Pan: WASD / arrow keys, mouse drag (configurable button), and single-finger
    /// touch drag. Zoom: mouse scroll wheel and two-finger pinch. Movement is
    /// clamped to a world-space XZ rectangle and a camera-height range. While
    /// <see cref="restrictToDeployment"/> is on (default) the camera is locked once
    /// combat starts, so the player can only reposition it during setup.
    ///
    /// Uses the new Input System low-level device APIs (Mouse / Keyboard /
    /// Touchscreen .current) directly, so no .inputactions asset wiring is needed;
    /// Active Input Handling in this project is "Input System Package" only, so the
    /// legacy UnityEngine.Input API is unavailable.
    ///
    /// Attach to the battle camera (or a camera rig). Bounds, speeds and the zoom
    /// height range are designer-tunable; verify the feel in Play mode.
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

        [Header("Zoom (camera height)")]
        [SerializeField, Min(0f)] private float scrollZoomSpeed = 1.5f;
        [SerializeField, Min(0f)] private float pinchZoomSpeed = 0.03f;
        [SerializeField] private float minHeight = 3f;
        [SerializeField] private float maxHeight = 50f;

        [Header("World bounds (XZ)")]
        [Tooltip("Minimum world X (x) and Z (y) the camera position is clamped to.")]
        [SerializeField] private Vector2 boundsMin = new Vector2(-15f, -25f);
        [Tooltip("Maximum world X (x) and Z (y) the camera position is clamped to.")]
        [SerializeField] private Vector2 boundsMax = new Vector2(40f, 45f);

        [Header("Gating")]
        [Tooltip("When true, the camera only moves during BattlePhase.PreBattle (locked once combat is active).")]
        [SerializeField] private bool restrictToDeployment = true;

        [Header("Auto-frame (deployment)")]
        [Tooltip("On deployment start, lerp to a pose that frames the whole board; restore to the battle pose on Assemble.")]
        [SerializeField] private bool autoFrameOnDeployment = true;
        [Tooltip("When true, the framing pose is COMPUTED from the deployment grid (always fits, follows cellSize/row " +
                 "changes). Falls back to the manual pose below if no DeploymentManager is found.")]
        [SerializeField] private bool computeFramingFromGrid = true;
        [Tooltip("Pitch (deg) for the computed frame. 90 = straight top-down; ~78 tilts slightly behind the player side.")]
        [SerializeField, Range(40f, 90f)] private float deploymentTiltDegrees = 78f;
        [Tooltip("Fallback camera position (used only when not computing from the grid).")]
        [SerializeField] private Vector3 deploymentPosition = new Vector3(4.5f, 14f, 6f);
        [Tooltip("Fallback camera euler rotation (used only when not computing from the grid).")]
        [SerializeField] private Vector3 deploymentEuler = new Vector3(70f, 0f, 0f);
        [Tooltip("Field of view during deployment (0 = keep current). Also used as the FOV for the computed frame.")]
        [SerializeField] private float deploymentFov = 0f;
        [Tooltip("Seconds for the camera transition in/out of the deployment framing.")]
        [SerializeField, Min(0.01f)] private float transitionSeconds = 0.6f;

        private bool dragging;
        private Vector2 lastDragPos;
        private bool pinching;
        private float lastPinchDist;

        private Camera cam;
        private DeploymentPhaseController phase;
        private DeploymentManager gridSource;
        private Vector3 battlePosition;
        private Quaternion battleRotation;
        private float battleFov;
        private bool transitioning;
        private Vector3 fromPos, targetPos;
        private Quaternion fromRot, targetRot;
        private float fromFov, targetFov;
        private float transitionT;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            battlePosition = transform.position;     // store the authored battle pose to restore later
            battleRotation = transform.rotation;
            battleFov = cam != null ? cam.fieldOfView : 60f;

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

        /// <summary>Lerp to a pose that frames the whole board (computed from the grid when possible).</summary>
        public void FrameDeployment()
        {
            if (computeFramingFromGrid && TryComputeBoardFraming(out var pos, out var rot, out var fov))
                BeginTransition(pos, rot, fov);
            else
                BeginTransition(deploymentPosition, Quaternion.Euler(deploymentEuler), deploymentFov);
        }

        /// <summary>
        /// Compute a near-top-down pose that frames the whole grid, derived from the
        /// deployment grid's size so it always fits and follows cellSize/row changes.
        /// Distance is the larger of the width-fit and depth-fit (so the board fits in
        /// both screen axes for the current aspect); the camera sits above and slightly
        /// behind the player's near side. Returns false if no grid source is available.
        /// </summary>
        private bool TryComputeBoardFraming(out Vector3 pos, out Quaternion rot, out float fov)
        {
            pos = default; rot = Quaternion.identity; fov = 0f;
            if (gridSource == null || gridSource.Config == null) return false;

            var cfg = gridSource.Config;
            float width = cfg.columns * cfg.cellSize;   // X
            float depth = cfg.rows * cfg.cellSize;      // Z
            Vector3 center = cfg.origin + new Vector3((cfg.columns - 1) * 0.5f * cfg.cellSize, 0f,
                                                      (cfg.rows - 1) * 0.5f * cfg.cellSize);

            fov = deploymentFov > 0f ? deploymentFov : (cam != null ? cam.fieldOfView : 60f);
            float aspect = cam != null && cam.aspect > 0.01f ? cam.aspect : (9f / 16f);
            float vHalf = fov * 0.5f * Mathf.Deg2Rad;
            float hHalf = Mathf.Atan(Mathf.Tan(vHalf) * aspect);
            float margin = cfg.cellSize;                // ~one cell of padding

            float distW = (width * 0.5f + margin) / Mathf.Max(0.01f, Mathf.Tan(hHalf));
            float distD = (depth * 0.5f + margin) / Mathf.Max(0.01f, Mathf.Tan(vHalf));
            float dist = Mathf.Max(distW, distD);

            float tilt = deploymentTiltDegrees * Mathf.Deg2Rad;
            pos = center + new Vector3(0f, dist * Mathf.Sin(tilt), -dist * Mathf.Cos(tilt));
            rot = Quaternion.Euler(deploymentTiltDegrees, 0f, 0f);
            return true;
        }

        /// <summary>Lerp back to the camera's stored battle pose (called on Assemble).</summary>
        public void FrameBattle() => BeginTransition(battlePosition, battleRotation, battleFov);

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
            if (transitioning) { AdvanceTransition(); return; }   // no manual control mid-transition

            if (restrictToDeployment && CombatServices.Phase != BattlePhase.PreBattle)
            {
                dragging = false;
                pinching = false;
                return;
            }

            Vector3 move = ReadKeyboardPan() + ReadMouseDragPan() + ReadTouchPan();
            float heightDelta = ReadZoom();

            if (move != Vector3.zero || !Mathf.Approximately(heightDelta, 0f))
            {
                Vector3 p = transform.position + move;
                p.y += heightDelta;
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
                    delta -= Mathf.Sign(scroll) * scrollZoomSpeed; // scroll up -> lower height (zoom in)
            }

            var ts = Touchscreen.current;
            if (ts != null)
            {
                Vector2 a = default, b = default;
                if (CountTouches(ts, ref a, ref b) >= 2)
                {
                    float dist = Vector2.Distance(a, b);
                    if (pinching) delta -= (dist - lastPinchDist) * pinchZoomSpeed; // fingers apart -> zoom in
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
    }
}
