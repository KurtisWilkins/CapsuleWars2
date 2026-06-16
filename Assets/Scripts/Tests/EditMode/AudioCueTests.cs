using System.Reflection;
using CapsuleWars.Audio;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>AudioCueSO clip/pitch selection logic (no playback).</summary>
    public class AudioCueTests
    {
        private static AudioClip Clip(string n) => AudioClip.Create(n, 1, 1, 44100, false);

        private static AudioCueSO Cue(AudioClip[] clips, Vector2 pitch)
        {
            var c = ScriptableObject.CreateInstance<AudioCueSO>();
            typeof(AudioCueSO).GetField("clips", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, clips);
            typeof(AudioCueSO).GetField("pitchRange", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(c, pitch);
            return c;
        }

        [Test]
        public void EmptyCue_HasNoClips_PicksNull()
        {
            var c = Cue(new AudioClip[0], Vector2.one);
            Assert.IsFalse(c.HasClips);
            Assert.IsNull(c.PickClip(new System.Random(1)));
            Object.DestroyImmediate(c);
        }

        [Test]
        public void SingleClip_AlwaysPicked()
        {
            var clip = Clip("a");
            var c = Cue(new[] { clip }, Vector2.one);
            Assert.IsTrue(c.HasClips);
            Assert.AreSame(clip, c.PickClip(new System.Random(1)));
            Object.DestroyImmediate(c);
            Object.DestroyImmediate(clip);
        }

        [Test]
        public void PickClip_DeterministicWithSeed()
        {
            var a = Clip("a");
            var b = Clip("b");
            var c = Cue(new[] { a, b }, Vector2.one);
            Assert.AreSame(c.PickClip(new System.Random(7)), c.PickClip(new System.Random(7)));
            Object.DestroyImmediate(c);
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void PickPitch_StaysWithinRange()
        {
            var c = Cue(new AudioClip[0], new Vector2(0.8f, 1.3f));
            for (int i = 0; i < 25; i++)
            {
                float p = c.PickPitch(new System.Random(i));
                Assert.GreaterOrEqual(p, 0.8f);
                Assert.LessOrEqual(p, 1.3f);
            }
            Object.DestroyImmediate(c);
        }
    }
}
