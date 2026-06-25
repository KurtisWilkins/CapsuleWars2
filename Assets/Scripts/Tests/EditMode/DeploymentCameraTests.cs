using System.Reflection;
using CapsuleWars.UI.CameraControl;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// DeploymentCameraController.Clamp keeps the camera inside the world XZ rectangle and the
    /// zoom-height range — the bound that stops the view-direction zoom from flying off the board.
    /// Pure, so it's testable without a scene or Play mode (battle-camera fix, ADR-014).
    /// </summary>
    public class DeploymentCameraTests
    {
        private static DeploymentCameraController MakeController(
            Vector2 boundsMin, Vector2 boundsMax, float minHeight, float maxHeight)
        {
            var go = new GameObject("cam");
            go.AddComponent<Camera>();
            var ctrl = go.AddComponent<DeploymentCameraController>();
            Set(ctrl, "boundsMin", boundsMin);
            Set(ctrl, "boundsMax", boundsMax);
            Set(ctrl, "minHeight", minHeight);
            Set(ctrl, "maxHeight", maxHeight);
            return ctrl;
        }

        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

        [Test]
        public void Clamp_PullsXZAndHeightToTheBounds()
        {
            var ctrl = MakeController(new Vector2(-5f, -30f), new Vector2(35f, 40f), 5f, 55f);

            var high = ctrl.Clamp(new Vector3(100f, 100f, 100f));
            Assert.AreEqual(35f, high.x, 1e-4f);   // x -> boundsMax.x
            Assert.AreEqual(55f, high.y, 1e-4f);   // y -> maxHeight
            Assert.AreEqual(40f, high.z, 1e-4f);   // z -> boundsMax.y

            var low = ctrl.Clamp(new Vector3(-100f, -100f, -100f));
            Assert.AreEqual(-5f, low.x, 1e-4f);    // x -> boundsMin.x
            Assert.AreEqual(5f, low.y, 1e-4f);     // y -> minHeight
            Assert.AreEqual(-30f, low.z, 1e-4f);   // z -> boundsMin.y

            Object.DestroyImmediate(ctrl.gameObject);
        }

        [Test]
        public void Clamp_LeavesAnInBoundsPositionUnchanged()
        {
            var ctrl = MakeController(new Vector2(-5f, -30f), new Vector2(35f, 40f), 5f, 55f);
            var inside = new Vector3(10.5f, 28f, -14f);   // ~the 45° battle pose
            Assert.AreEqual(inside, ctrl.Clamp(inside));
            Object.DestroyImmediate(ctrl.gameObject);
        }

        [Test]
        public void Clamp_BoundsTheForwardZoom_AtMinHeight()
        {
            // The new zoom moves along the camera's view direction (position + forward*zoom), then
            // Clamp. A 45°-down camera's forward has a downward y, so a hard zoom-in must not drop the
            // camera below minHeight.
            var ctrl = MakeController(new Vector2(-50f, -50f), new Vector2(50f, 50f), 5f, 55f);
            Vector3 forward = new Vector3(0f, -Mathf.Sin(45f * Mathf.Deg2Rad), Mathf.Cos(45f * Mathf.Deg2Rad));
            Vector3 pos = new Vector3(10.5f, 10f, 0f);

            var clamped = ctrl.Clamp(pos + forward * 100f);   // zoom in hard
            Assert.AreEqual(5f, clamped.y, 1e-4f);            // stopped at minHeight, not below the floor

            Object.DestroyImmediate(ctrl.gameObject);
        }
    }
}
