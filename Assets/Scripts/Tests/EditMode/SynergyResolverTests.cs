using System.Collections.Generic;
using System.Reflection;
using CapsuleWars.Combat.Stats;
using CapsuleWars.Core;
using CapsuleWars.Data.Classes;
using CapsuleWars.Data.StatusEffects;
using CapsuleWars.Units.Controllers;
using NUnit.Framework;
using UnityEngine;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Tests SynergyResolver: counts live units per class, picks the
    /// highest met tier, applies tier buffs to those units.
    /// </summary>
    public class SynergyResolverTests
    {
        private readonly List<GameObject> spawned = new();
        private MockRegistry registry;
        private UnitClass_SO warriorClass;
        private UnitClass_SO wizardClass;
        private readonly List<UnitClass_SO> extraClasses = new();

        [SetUp]
        public void Setup()
        {
            registry = new MockRegistry();

            warriorClass = ScriptableObject.CreateInstance<UnitClass_SO>();
            SetField(warriorClass, "tiers", new List<ClassSynergyTier>
            {
                new ClassSynergyTier
                {
                    threshold = 2,
                    teamBuffs = new List<StatBuff>
                    {
                        new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 10 }
                    }
                },
                new ClassSynergyTier
                {
                    threshold = 3,
                    teamBuffs = new List<StatBuff>
                    {
                        new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 25 }
                    }
                },
            });
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var go in spawned)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            spawned.Clear();
            Object.DestroyImmediate(warriorClass);
            if (wizardClass != null) Object.DestroyImmediate(wizardClass);
            foreach (var c in extraClasses) if (c != null) Object.DestroyImmediate(c);
            extraClasses.Clear();
        }

        [Test]
        public void BelowThreshold_NoBuffApplied()
        {
            var (root, status) = SpawnWarrior(Team.Player);
            registry.Register(root);

            var resolver = new SynergyResolver(registry);
            int before = status.Atk;
            resolver.RecomputeSynergies();
            Assert.AreEqual(before, status.Atk);
        }

        [Test]
        public void AtThreshold2_AppliesLowerTier()
        {
            var (r1, s1) = SpawnWarrior(Team.Player);
            var (r2, s2) = SpawnWarrior(Team.Player);
            registry.Register(r1);
            registry.Register(r2);

            int before = s1.Atk;
            new SynergyResolver(registry).RecomputeSynergies();

            // Tier 2: +10 Atk
            Assert.AreEqual(before + 10, s1.Atk);
            Assert.AreEqual(before + 10, s2.Atk);
        }

        [Test]
        public void AtThreshold3_AppliesHigherTier_NotBothStacked()
        {
            var (r1, s1) = SpawnWarrior(Team.Player);
            var (r2, _) = SpawnWarrior(Team.Player);
            var (r3, _) = SpawnWarrior(Team.Player);
            registry.Register(r1);
            registry.Register(r2);
            registry.Register(r3);

            int before = s1.Atk;
            new SynergyResolver(registry).RecomputeSynergies();

            // Highest met tier wins (tier 3 = +25), not stacked with tier 2.
            Assert.AreEqual(before + 25, s1.Atk);
        }

        [Test]
        public void EnemiesDoNotContributeToPlayerSynergy()
        {
            var (r1, s1) = SpawnWarrior(Team.Player);
            var (r2, _) = SpawnWarrior(Team.Enemy);
            var (r3, _) = SpawnWarrior(Team.Enemy);
            registry.Register(r1);
            registry.Register(r2);
            registry.Register(r3);

            int before = s1.Atk;
            new SynergyResolver(registry).RecomputeSynergies();

            // Player has only 1 warrior; no tier active for player.
            Assert.AreEqual(before, s1.Atk);
        }

        [Test]
        public void DownedUnits_DoNotCountTowardsThreshold()
        {
            var (r1, s1) = SpawnWarrior(Team.Player);
            var (r2, _) = SpawnWarrior(Team.Player);
            registry.Register(r1);
            registry.Register(r2);

            // Down one of them
            r2.Health.TakeDamage(99999, null);

            int before = s1.Atk;
            new SynergyResolver(registry).RecomputeSynergies();

            // Only 1 warrior alive; tier 2 not reached.
            Assert.AreEqual(before, s1.Atk);
        }

        [Test]
        public void GlobalBuffs_ApplyToWholeTeam_RegardlessOfClass()
        {
            // Wizard tier (threshold 2): +8 Atk to wizards (teamBuffs) + +5 Def to the WHOLE team (globalBuffs).
            wizardClass = ScriptableObject.CreateInstance<UnitClass_SO>();
            SetField(wizardClass, "tiers", new List<ClassSynergyTier>
            {
                new ClassSynergyTier
                {
                    threshold = 2,
                    teamBuffs = new List<StatBuff>
                    {
                        new StatBuff { stat = StatType.Atk, modType = StatBuffModType.Flat, amount = 8 }
                    },
                    globalBuffs = new List<StatBuff>
                    {
                        new StatBuff { stat = StatType.Def, modType = StatBuffModType.Flat, amount = 5 }
                    }
                },
            });

            var (rw1, sw1) = SpawnOf(Team.Player, wizardClass);
            var (rw2, _) = SpawnOf(Team.Player, wizardClass);
            var (rWar, sWar) = SpawnOf(Team.Player, warriorClass); // lone warrior — below warrior tier 2
            registry.Register(rw1);
            registry.Register(rw2);
            registry.Register(rWar);

            int warAtkBefore = sWar.Atk;
            int warDefBefore = sWar.Def;
            int wizAtkBefore = sw1.Atk;
            int wizDefBefore = sw1.Def;

            new SynergyResolver(registry).RecomputeSynergies();

            // Wizard global (+5 Def) reaches the warrior; wizard team buff (+8 Atk) does NOT.
            Assert.AreEqual(warDefBefore + 5, sWar.Def, "warrior should receive the wizard globalBuff");
            Assert.AreEqual(warAtkBefore, sWar.Atk, "warrior must NOT receive the wizard teamBuff");

            // Wizards get both their team buff and the global buff.
            Assert.AreEqual(wizAtkBefore + 8, sw1.Atk, "wizard should receive its teamBuff");
            Assert.AreEqual(wizDefBefore + 5, sw1.Def, "wizard should receive its globalBuff");
        }

        [Test]
        public void SynergyEffects_PushedToSink_WhenTierActive()
        {
            // BTS-E2: a threshold-2 tier granting HealOnKill 5% → pushed to every same-class unit's sink.
            var healClass = ScriptableObject.CreateInstance<UnitClass_SO>();
            extraClasses.Add(healClass);
            SetField(healClass, "tiers", new List<ClassSynergyTier>
            {
                new ClassSynergyTier
                {
                    threshold = 2,
                    synergyEffects = new List<SynergyEffect>
                    {
                        new SynergyEffect { kind = SynergyEffectKind.HealOnKill, magnitude = 5f }
                    }
                },
            });

            var (r1, _, sink1) = SpawnWithSink(Team.Player, healClass);
            var (r2, _, sink2) = SpawnWithSink(Team.Player, healClass);
            registry.Register(r1);
            registry.Register(r2);

            new SynergyResolver(registry).RecomputeSynergies();

            Assert.AreEqual(1, sink1.Effects.Count, "tier active → one synergy effect pushed");
            Assert.AreEqual(SynergyEffectKind.HealOnKill, sink1.Effects[0].kind);
            Assert.AreEqual(5f, sink1.Effects[0].magnitude, 0.001f);
            Assert.AreEqual(1, sink2.Effects.Count, "both same-class units receive the effect");
        }

        [Test]
        public void SynergyEffects_NotPushed_BelowThreshold()
        {
            var healClass = ScriptableObject.CreateInstance<UnitClass_SO>();
            extraClasses.Add(healClass);
            SetField(healClass, "tiers", new List<ClassSynergyTier>
            {
                new ClassSynergyTier
                {
                    threshold = 2,
                    synergyEffects = new List<SynergyEffect>
                    {
                        new SynergyEffect { kind = SynergyEffectKind.HealOnHit, magnitude = 2f }
                    }
                },
            });

            var (r1, _, sink1) = SpawnWithSink(Team.Player, healClass);
            registry.Register(r1);   // lone unit → below threshold 2

            new SynergyResolver(registry).RecomputeSynergies();

            Assert.AreEqual(0, sink1.Effects.Count, "below threshold → sink cleared (no effects)");
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private (UnitRoot, UnitStatusController) SpawnWarrior(Team team) => SpawnOf(team, warriorClass);

        private (UnitRoot, UnitStatusController, RecordingSink) SpawnWithSink(Team team, UnitClass_SO cls)
        {
            var (root, status) = SpawnOf(team, cls);
            var sink = root.gameObject.AddComponent<RecordingSink>();
            return (root, status, sink);
        }

        private (UnitRoot, UnitStatusController) SpawnOf(Team team, UnitClass_SO cls)
        {
            var go = new GameObject($"Unit_{team}");
            spawned.Add(go);
            var root = go.AddComponent<UnitRoot>();
            var status = go.AddComponent<UnitStatusController>();
            var health = go.AddComponent<UnitHealthController>();
            SetField(root, "team", team);
            SetField(root, "status", status);
            SetField(root, "health", health);
            SetField(status, "unitClass", cls);
            // Awake on UnitRoot has already run via AddComponent ordering; force-relink fields.
            return (root, status);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        // Stand-in for AbilityController (the real ISynergyBehaviorSink) — records what the resolver pushes.
        private sealed class RecordingSink : MonoBehaviour, ISynergyBehaviorSink
        {
            public readonly List<SynergyEffect> Effects = new();
            public void SetSynergyEffects(IReadOnlyList<SynergyEffect> effects)
            {
                Effects.Clear();
                if (effects != null) Effects.AddRange(effects);
            }
        }

        private sealed class MockRegistry : ICombatRegistry
        {
            private readonly List<IUnitRef> units = new();
            public IReadOnlyList<IUnitRef> Units => units;
            public event System.Action<IUnitRef> OnUnitRegistered;
            public event System.Action<IUnitRef> OnUnitUnregistered;
            public void Register(IUnitRef u) { units.Add(u); OnUnitRegistered?.Invoke(u); }
            public void Unregister(IUnitRef u) { units.Remove(u); OnUnitUnregistered?.Invoke(u); }
        }
    }
}
