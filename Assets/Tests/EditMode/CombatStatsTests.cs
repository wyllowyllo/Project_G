using Combat.Core;
using Combat.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode
{
    [TestFixture]
    public class CombatStatsTests
    {
        private CombatStats _stats;
        private MockModifierSource _source;

        [SetUp]
        public void SetUp()
        {
            _stats = new CombatStats(100f, 0.5f, 2f, 50f);
            _source = new MockModifierSource("TestSource");
        }

        #region Constructor Tests

        [Test]
        public void CombatStats_Constructor_InitializesAllStats()
        {
            var stats = new CombatStats(100f, 0.25f, 1.5f, 30f);

            Assert.AreEqual(100f, stats.AttackDamage.Value);
            Assert.AreEqual(0.25f, stats.CriticalChance.Value);
            Assert.AreEqual(1.5f, stats.CriticalMultiplier.Value);
            Assert.AreEqual(30f, stats.Defense.Value);
        }

        #endregion

        #region FromData Tests

        [Test]
        public void CombatStats_FromData_InitializesFromScriptableObject()
        {
            var data = ScriptableObject.CreateInstance<CombatStatsData>();
            SetCombatStatsData(data, 75f, 0.3f, 1.8f, 20f);

            var stats = CombatStats.FromData(data);

            Assert.AreEqual(75f, stats.AttackDamage.Value);
            Assert.AreEqual(0.3f, stats.CriticalChance.Value);
            Assert.AreEqual(1.8f, stats.CriticalMultiplier.Value);
            Assert.AreEqual(20f, stats.Defense.Value);

            Object.DestroyImmediate(data);
        }

        [Test]
        public void CombatStats_FromNullData_LogsErrorAndUsesDefaults()
        {
            var stats = CombatStats.FromData(null);
            
            LogAssert.Expect("[CombatStats] Cannot create from null CombatStatsData");

            // Default values: 10f, 0.1f, 1.5f, 0f
            Assert.AreEqual(10f, stats.AttackDamage.Value);
            Assert.AreEqual(0.1f, stats.CriticalChance.Value);
            Assert.AreEqual(1.5f, stats.CriticalMultiplier.Value);
            Assert.AreEqual(0f, stats.Defense.Value);
        }

        #endregion

        #region Stat Bounds Tests

        [Test]
        public void CombatStats_StatBounds_AreCorrect()
        {
            // AttackDamage: min 0, max float.MaxValue
            // CriticalChance: min 0, max 1
            // CriticalMultiplier: min 1, max float.MaxValue
            // Defense: min 0, max float.MaxValue

            var attackMod = new StatModifier(-200f, StatModifierType.Additive, _source);
            var critChanceMod = new StatModifier(2f, StatModifierType.Additive, _source);
            var critMultMod = new StatModifier(-5f, StatModifierType.Additive, _source);
            var defenseMod = new StatModifier(-100f, StatModifierType.Additive, _source);

            _stats.AttackDamage.AddModifier(attackMod);
            _stats.CriticalChance.AddModifier(critChanceMod);
            _stats.CriticalMultiplier.AddModifier(critMultMod);
            _stats.Defense.AddModifier(defenseMod);

            // Attack: 100 - 200 = -100 -> clamped to 0
            Assert.AreEqual(0f, _stats.AttackDamage.Value);

            // CriticalChance: 0.5 + 2 = 2.5 -> clamped to 1
            Assert.AreEqual(1f, _stats.CriticalChance.Value);

            // CriticalMultiplier: 2 - 5 = -3 -> clamped to 1
            Assert.AreEqual(1f, _stats.CriticalMultiplier.Value);

            // Defense: 50 - 100 = -50 -> clamped to 0
            Assert.AreEqual(0f, _stats.Defense.Value);
        }

        #endregion

        #region Modifier Management Tests

        [Test]
        public void CombatStats_ClearAllModifiers_ClearsAllStats()
        {
            var mod1 = new StatModifier(50f, StatModifierType.Additive, _source);
            var mod2 = new StatModifier(0.2f, StatModifierType.Additive, _source);
            _stats.AttackDamage.AddModifier(mod1);
            _stats.CriticalChance.AddModifier(mod2);

            _stats.ClearAllModifiers();

            Assert.AreEqual(100f, _stats.AttackDamage.Value);
            Assert.AreEqual(0.5f, _stats.CriticalChance.Value);
        }

        [Test]
        public void CombatStats_RemoveModifiersFromSource_ClearsOnlyTarget()
        {
            var source1 = new MockModifierSource("Source1");
            var source2 = new MockModifierSource("Source2");
            var mod1 = new StatModifier(10f, StatModifierType.Additive, source1);
            var mod2 = new StatModifier(20f, StatModifierType.Additive, source2);

            _stats.AttackDamage.AddModifier(mod1);
            _stats.AttackDamage.AddModifier(mod2);

            bool removed = _stats.RemoveAllModifiersFromSource(source1);

            Assert.IsTrue(removed);
            // 100 (base) + 20 (source2) = 120
            Assert.AreEqual(120f, _stats.AttackDamage.Value);
        }

        [Test]
        public void CombatStats_RemoveModifiersFromNullSource_ReturnsFalse()
        {
            bool removed = _stats.RemoveAllModifiersFromSource(null);

            Assert.IsFalse(removed);
        }

        #endregion

        #region Helper Methods

        private void SetCombatStatsData(CombatStatsData data, float attack, float critChance, float critMult, float defense)
        {
            var serializedObject = new UnityEditor.SerializedObject(data);
            serializedObject.FindProperty("_baseAttackDamage").floatValue = attack;
            serializedObject.FindProperty("_criticalChance").floatValue = critChance;
            serializedObject.FindProperty("_criticalMultiplier").floatValue = critMult;
            serializedObject.FindProperty("_defense").floatValue = defense;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion

        #region Mock Classes

        private class MockModifierSource : IModifierSource
        {
            public string Id { get; }

            public MockModifierSource(string id)
            {
                Id = id;
            }
        }

        #endregion
    }
}
