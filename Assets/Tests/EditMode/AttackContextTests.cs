using Combat.Core;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class AttackContextTests
    {
        private MockCombatant _attacker;
        private GameObject _attackerObject;

        [SetUp]
        public void SetUp()
        {
            _attackerObject = new GameObject("Attacker");
            _attackerObject.transform.position = new Vector3(1f, 2f, 3f);

            _attacker = new MockCombatant(
                attackDamage: 100f,
                criticalChance: 0.2f,
                criticalMultiplier: 1.5f,
                defense: 10f,
                transform: _attackerObject.transform
            );
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_attackerObject);
        }

        #region Factory Method Tests

        [Test]
        public void AttackContext_Scaled_SetsCorrectProperties()
        {
            var context = AttackContext.Scaled(_attacker, baseMultiplier: 1.5f, buffMultiplier: 1.2f, type: DamageType.Skill);

            Assert.AreEqual(DamageSource.AttackScaled, context.Source);
            Assert.AreEqual(1.5f, context.BaseValue);
            Assert.AreEqual(1.2f, context.Multiplier);
            Assert.AreEqual(DamageType.Skill, context.DamageType);
        }

        [Test]
        public void AttackContext_Scaled_SnapshotsAttackerStats()
        {
            var context = AttackContext.Scaled(_attacker, baseMultiplier: 1f, buffMultiplier: 1f);

            Assert.AreEqual(100f, context.AttackDamage);
            Assert.AreEqual(0.2f, context.CriticalChance);
            Assert.AreEqual(1.5f, context.CriticalMultiplier);
            Assert.AreEqual(CombatTeam.Player, context.AttackerTeam);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), context.AttackerPosition);
        }

        [Test]
        public void AttackContext_Fixed_SetsCorrectProperties()
        {
            var context = AttackContext.Fixed(_attacker, damage: 50f, multiplier: 2f, type: DamageType.True);

            Assert.AreEqual(DamageSource.Fixed, context.Source);
            Assert.AreEqual(50f, context.BaseValue);
            Assert.AreEqual(2f, context.Multiplier);
            Assert.AreEqual(DamageType.True, context.DamageType);
        }

        [Test]
        public void AttackContext_MaxHpPercent_SetsCorrectProperties()
        {
            var context = AttackContext.MaxHpPercent(_attacker, percent: 0.15f, multiplier: 1.1f, type: DamageType.Normal);

            Assert.AreEqual(DamageSource.MaxHpPercent, context.Source);
            Assert.AreEqual(0.15f, context.BaseValue);
            Assert.AreEqual(1.1f, context.Multiplier);
            Assert.AreEqual(DamageType.Normal, context.DamageType);
        }

        [Test]
        public void AttackContext_CurrentHpPercent_SetsCorrectProperties()
        {
            var context = AttackContext.CurrentHpPercent(_attacker, percent: 0.2f, multiplier: 0.8f, type: DamageType.Skill);

            Assert.AreEqual(DamageSource.CurrentHpPercent, context.Source);
            Assert.AreEqual(0.2f, context.BaseValue);
            Assert.AreEqual(0.8f, context.Multiplier);
            Assert.AreEqual(DamageType.Skill, context.DamageType);
        }

        #endregion

        #region Default DamageType Tests

        [Test]
        public void AttackContext_DefaultDamageType_IsNormal()
        {
            var scaledContext = AttackContext.Scaled(_attacker, 1f, 1f);
            var fixedContext = AttackContext.Fixed(_attacker, 100f);
            var maxHpContext = AttackContext.MaxHpPercent(_attacker, 0.1f);
            var currentHpContext = AttackContext.CurrentHpPercent(_attacker, 0.1f);

            Assert.AreEqual(DamageType.Normal, scaledContext.DamageType);
            Assert.AreEqual(DamageType.Normal, fixedContext.DamageType);
            Assert.AreEqual(DamageType.Normal, maxHpContext.DamageType);
            Assert.AreEqual(DamageType.Normal, currentHpContext.DamageType);
        }

        #endregion

        #region Mock Classes

        private class MockCombatant : ICombatant
        {
            public Transform Transform { get; }
            public CombatStats Stats { get; }
            public CombatTeam Team => CombatTeam.Player;

            public MockCombatant(float attackDamage, float criticalChance, float criticalMultiplier, float defense, Transform transform)
            {
                Stats = new CombatStats(attackDamage, criticalChance, criticalMultiplier, defense);
                Transform = transform;
            }

            public CombatantAttackStats GetAttackStats()
                => new CombatantAttackStats(
                    Stats.AttackDamage.Value,
                    Stats.CriticalChance.Value,
                    Stats.CriticalMultiplier.Value);
        }

        #endregion
    }
}
