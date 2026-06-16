using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using NUnit.Framework;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Meta-progression unlock economy (PlayerProfileDTO) + run reward math.
    /// Pure data — no scene load.
    /// </summary>
    public class UnlockProgressionTests
    {
        [Test]
        public void NewLegacyProfile_HasFreshPlayerProfile()
        {
            var p = new LegacyProfileDTO();
            Assert.IsNotNull(p.PlayerProfile);
            Assert.AreEqual(0, p.PlayerProfile.UnlockPoints);
        }

        [Test]
        public void AddPoints_Accumulates_AndIgnoresNonPositive()
        {
            var pp = new PlayerProfileDTO();
            pp.AddPoints(5);
            pp.AddPoints(0);
            pp.AddPoints(-3);
            Assert.AreEqual(5, pp.UnlockPoints);
        }

        [Test]
        public void TryUnlockPart_SpendsPoints_AndRecords()
        {
            var pp = new PlayerProfileDTO();
            pp.AddPoints(5);
            Assert.IsTrue(pp.TryUnlockPart("hand_claw", 2));
            Assert.AreEqual(3, pp.UnlockPoints);
            Assert.IsTrue(pp.HasPart("hand_claw"));
        }

        [Test]
        public void TryUnlockPart_FailsWhenInsufficient_OrDuplicate()
        {
            var pp = new PlayerProfileDTO();
            pp.AddPoints(2);
            Assert.IsFalse(pp.TryUnlockPart("body_big", 3));   // not enough points
            Assert.AreEqual(2, pp.UnlockPoints);
            Assert.IsTrue(pp.TryUnlockPart("body_big", 2));
            Assert.IsFalse(pp.TryUnlockPart("body_big", 0));   // already owned
        }

        [Test]
        public void GrantPart_IsFree_AndIdempotent()
        {
            var pp = new PlayerProfileDTO();
            pp.GrantPart("foot_peg");
            pp.GrantPart("foot_peg");
            Assert.IsTrue(pp.HasPart("foot_peg"));
            Assert.AreEqual(1, pp.UnlockedPartIds.Count);
            Assert.AreEqual(0, pp.UnlockPoints);   // no spend
        }

        [Test]
        public void TryUnlockPalette_SpendsAndRecords()
        {
            var pp = new PlayerProfileDTO();
            pp.AddPoints(1);
            Assert.IsTrue(pp.TryUnlockPalette("sunset", 1));
            Assert.IsTrue(pp.HasPalette("sunset"));
            Assert.AreEqual(0, pp.UnlockPoints);
        }

        [Test]
        public void Rewards_FloorsPlusWinBonus()
        {
            Assert.AreEqual(5 + UnlockRewards.CompletionBonus, UnlockRewards.PointsForRun(5, true));
            Assert.AreEqual(3, UnlockRewards.PointsForRun(3, false));
            Assert.AreEqual(UnlockRewards.CompletionBonus, UnlockRewards.PointsForRun(0, true));
            Assert.AreEqual(0, UnlockRewards.PointsForRun(-1, false));
        }
    }
}
