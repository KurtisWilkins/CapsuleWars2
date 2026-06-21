using CapsuleWars.Combat.State;
using CapsuleWars.Core;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Verifies the deployment gate on BattleStateManager: combat can't reach Active
    /// while DeploymentRequired is set and DeploymentConfirmed is not, and starts
    /// normally otherwise. (StartBattle doesn't touch the registry, so it runs
    /// without the heavy Awake setup.)
    /// </summary>
    public class DeploymentGateTests
    {
        private GameObject go;
        private BattleStateManager bsm;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("BSM");
            bsm = go.AddComponent<BattleStateManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
            CombatServices.Phase = BattlePhase.PreBattle;   // StartBattle mutates the static; reset
        }

        [Test]
        public void StartBattle_NoDeploymentRequired_GoesActive()
        {
            Assert.AreEqual(BattlePhase.PreBattle, bsm.Phase);
            bsm.StartBattle();
            Assert.AreEqual(BattlePhase.Active, bsm.Phase);
        }

        [Test]
        public void StartBattle_Blocked_UntilDeploymentConfirmed()
        {
            bsm.DeploymentRequired = true;

            bsm.StartBattle();
            Assert.AreEqual(BattlePhase.PreBattle, bsm.Phase, "combat must not start before Assemble");

            bsm.DeploymentConfirmed = true;
            bsm.StartBattle();
            Assert.AreEqual(BattlePhase.Active, bsm.Phase);
        }
    }
}
