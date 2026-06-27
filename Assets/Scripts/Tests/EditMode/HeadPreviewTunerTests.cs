using System.Reflection;
using CapsuleWars.Units.Customization;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// HeadPreviewTuner re-seats the floating-sphere head mount from its serialized
    /// size / float-gap / face-forward fields (the offset math behind the editor
    /// "Apply Head Preview" workflow). Pure transform logic — no scene/Play needed.
    /// </summary>
    public class HeadPreviewTunerTests
    {
        private static void SetField(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }

        [Test]
        public void ApplyHeadPreview_SeatsMount_FromFields()
        {
            var root = new GameObject("unit");
            var mount = new GameObject("Mount_Head_Sphere");
            mount.transform.SetParent(root.transform, false);
            mount.transform.localPosition = new Vector3(0.3f, 99f, -0.2f); // x/z preserved, y overwritten

            var tuner = root.AddComponent<HeadPreviewTuner>();
            SetField(tuner, "headMount", mount.transform);
            SetField(tuner, "sphereSize", 0.5f);
            SetField(tuner, "floatGap", 0.2f);
            SetField(tuner, "faceForwardEuler", new Vector3(0f, 90f, 0f));

            tuner.ApplyHeadPreview();

            Assert.AreEqual(0.5f, mount.transform.localScale.x, 1e-4f, "uniform sphere size");
            Assert.AreEqual(0.5f, mount.transform.localScale.y, 1e-4f);
            Assert.AreEqual(0.2f, mount.transform.localPosition.y, 1e-4f, "float gap sets local Y");
            Assert.AreEqual(0.3f, mount.transform.localPosition.x, 1e-4f, "X preserved");
            Assert.AreEqual(-0.2f, mount.transform.localPosition.z, 1e-4f, "Z preserved");
            Assert.Less(Quaternion.Angle(Quaternion.Euler(0f, 90f, 0f), mount.transform.localRotation), 0.01f, "face-forward applied");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ApplyHeadPreview_ClampsSizeAboveZero()
        {
            var root = new GameObject("unit");
            var mount = new GameObject("Mount_Head_Sphere");
            mount.transform.SetParent(root.transform, false);
            var tuner = root.AddComponent<HeadPreviewTuner>();
            SetField(tuner, "headMount", mount.transform);
            SetField(tuner, "sphereSize", 0f);   // degenerate

            tuner.ApplyHeadPreview();

            Assert.Greater(mount.transform.localScale.x, 0f, "size clamped above zero so the head never vanishes");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ApplyHeadPreview_NullMount_DoesNotThrow()
        {
            var root = new GameObject("unit");
            var tuner = root.AddComponent<HeadPreviewTuner>();
            Assert.DoesNotThrow(() => tuner.ApplyHeadPreview());
            Object.DestroyImmediate(root);
        }
    }
}
