using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Data.Units;
using CapsuleWars.Persistence.Dto;
using CapsuleWars.Run;
using CapsuleWars.Run.Map;
using CapsuleWars.Units.Controllers;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// BTS-H wiring: the evolution multiplier scales a unit's BASE stats on the controller, XP is granted to the
    /// party after a win, and XP persists through the run DTO round-trip.
    /// </summary>
    public class EvolutionWiringTests
    {
        private static void SetField(object t, string f, object v) =>
            t.GetType().GetField(f, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(t, v);

        [Test]
        public void EvolutionMultiplier_ScalesBaseStats()
        {
            var go = new GameObject("u");
            var status = go.AddComponent<UnitStatusController>();
            SetField(status, "baseAtk", 10);
            SetField(status, "baseMaxHp", 100);

            status.SetEvolutionMultiplier(1.5f);
            Assert.AreEqual(15, status.Atk, "Atk = round(base * multiplier)");
            Assert.AreEqual(150, status.MaxHp);

            status.SetEvolutionMultiplier(1f);
            Assert.AreEqual(10, status.Atk, "multiplier 1 → no change");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void GrantXp_AddsToParty_AndPersistsThroughJson()
        {
            var config = ScriptableObject.CreateInstance<EvolutionConfig_SO>();   // default XpPerBattleWin = 60
            var state = new RunState(new RunMap(new List<MapNode>()), 0, 1);
            var u1 = new UnitDTO("u1", "A", null);
            var u2 = new UnitDTO("u2", "B", null) { Xp = 40 };
            state.SetParty(new[] { u1, u2 });

            int n = EvolutionGrant.GrantXp(state, config);

            Assert.AreEqual(2, n);
            Assert.AreEqual(60, u1.Xp);
            Assert.AreEqual(100, u2.Xp);

            var back = JsonConvert.DeserializeObject<RunStateDTO>(JsonConvert.SerializeObject(state.ToDTO()));
            Assert.AreEqual(60, back.Party[0].Xp, "Xp survives JSON round-trip");
            Assert.AreEqual(100, back.Party[1].Xp);

            Object.DestroyImmediate(config);
        }

        [Test]
        public void GrantXp_NullSafe()
        {
            Assert.AreEqual(0, EvolutionGrant.GrantXp(null, null));
        }
    }
}
