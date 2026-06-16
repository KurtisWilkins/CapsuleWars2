using CapsuleWars.Persistence.Dto;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>SettingsDTO defaults, clamping, and JSON round-trip.</summary>
    public class SettingsTests
    {
        [Test]
        public void Defaults_AreSensible()
        {
            var s = new SettingsDTO();
            Assert.AreEqual(1f, s.MasterVolume);
            Assert.AreEqual(1f, s.SfxVolume);
            Assert.AreEqual(1f, s.MusicVolume);
            Assert.AreEqual(-1, s.QualityLevel);
            Assert.IsTrue(s.Fullscreen);
        }

        [Test]
        public void Clamp_BoundsVolumes()
        {
            var s = new SettingsDTO { MasterVolume = 2f, SfxVolume = -1f, MusicVolume = 0.5f };
            s.Clamp();
            Assert.AreEqual(1f, s.MasterVolume);
            Assert.AreEqual(0f, s.SfxVolume);
            Assert.AreEqual(0.5f, s.MusicVolume);
        }

        [Test]
        public void JsonRoundTrip_PreservesValues()
        {
            var s = new SettingsDTO
            {
                MasterVolume = 0.3f, SfxVolume = 0.7f, MusicVolume = 0.2f,
                QualityLevel = 2, Fullscreen = false
            };
            var back = JsonConvert.DeserializeObject<SettingsDTO>(JsonConvert.SerializeObject(s));
            Assert.AreEqual(0.3f, back.MasterVolume);
            Assert.AreEqual(0.7f, back.SfxVolume);
            Assert.AreEqual(2, back.QualityLevel);
            Assert.IsFalse(back.Fullscreen);
        }
    }
}
