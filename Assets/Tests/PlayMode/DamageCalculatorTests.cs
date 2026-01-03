using Combat.Core;
using Combat.Damage;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class DamageCalculatorTests
    {
        private MockCombatant _attacker;
        private MockCombatant _defender;
        private MockHealthProvider _defenderHealth;

        [SetUp]
        public void SetUp()
        {
            _attacker = new MockCombatant(
                attackDamage: 100f,
                criticalChance: 0f,
                criticalMultiplier: 2f,
                defense: 0f
            );
            _defender = new MockCombatant(
                attackDamage: 50f,
                criticalChance: 0f,
                criticalMultiplier: 1.5f,
                defense: 0f
            );
            _defenderHealth = new MockHealthProvider(currentHealth: 80f, maxHealth: 100f);
        }

        #region Base Damage Calculation Tests

        [Test]
        public void CalculateBaseDamage_AttackScaled_UsesAttackerStats()
        {
            var attack = AttackContext.Scaled(_attacker, baseMultiplier: 1f, buffMultiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // 100 (AttackDamage) * 1 (baseMultiplier) * 1 (buffMultiplier) = 100
            Assert.AreEqual(100f, result.FinalDamage);
        }

        [Test]
        public void CalculateBaseDamage_Fixed_IgnoresAttackerStats()
        {
            var attack = AttackContext.Fixed(_attacker, damage: 50f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // 50 (fixed damage) * 1 (multiplier) = 50
            Assert.AreEqual(50f, result.FinalDamage);
        }

        [Test]
        public void CalculateBaseDamage_MaxHpPercent_UsesDefenderMaxHp()
        {
            var attack = AttackContext.MaxHpPercent(_attacker, percent: 0.1f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // 100 (MaxHealth) * 0.1 (percent) * 1 (multiplier) = 10
            Assert.AreEqual(10f, result.FinalDamage);
        }

        [Test]
        public void CalculateBaseDamage_CurrentHpPercent_UsesDefenderCurrentHp()
        {
            var attack = AttackContext.CurrentHpPercent(_attacker, percent: 0.1f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // 80 (CurrentHealth) * 0.1 (percent) * 1 (multiplier) = 8
            Assert.AreEqual(8f, result.FinalDamage);
        }

        [Test]
        public void CalculateBaseDamage_HpPercent_NullDefenderHealth_ReturnsZero()
        {
            var attack = AttackContext.MaxHpPercent(_attacker, percent: 0.1f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, null);

            var result = DamageCalculator.Calculate(attack, defender);

            // null health returns 0, but minimum damage is 1
            Assert.AreEqual(1f, result.FinalDamage);
        }

        [Test]
        public void CalculateBaseDamage_ZeroDamage_ClampsToMinimum()
        {
            var attack = AttackContext.Fixed(_attacker, damage: 0f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            Assert.AreEqual(1f, result.FinalDamage);
        }

        #endregion

        #region Defense Application Tests

        [Test]
        public void ApplyDefense_ReducesDamage()
        {
            var defenderWithDefense = new MockCombatant(50f, 0f, 1.5f, defense: 100f);
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f);
            var defender = new DefenderInfo(defenderWithDefense, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // reduction = 100 / (100 + 100) = 0.5
            // 100 * (1 - 0.5) = 50
            Assert.AreEqual(50f, result.FinalDamage);
        }

        [Test]
        public void ApplyDefense_ZeroDefense_NoDamageReduction()
        {
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            Assert.AreEqual(100f, result.FinalDamage);
        }

        [Test]
        public void ApplyDefense_HighDefense_NeverReachesZero()
        {
            var defenderWithHighDefense = new MockCombatant(50f, 0f, 1.5f, defense: 10000f);
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f);
            var defender = new DefenderInfo(defenderWithHighDefense, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // reduction = 10000 / (10000 + 100) = 0.99
            // 100 * (1 - 0.99) = 1
            // Even with very high defense, damage never reaches 0 (minimum 1)
            Assert.GreaterOrEqual(result.FinalDamage, 1f);
        }

        [Test]
        public void Calculate_NullDefenderCombatant_UsesZeroDefense()
        {
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f);
            var defender = new DefenderInfo(null, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // null combatant means 0 defense, so no reduction
            Assert.AreEqual(100f, result.FinalDamage);
        }

        [Test]
        public void Calculate_NullAttacker_ThrowsOrHandles()
        {
            // AttackContext with null attacker should be handled gracefully
            // The system should either throw or return a default result
            Assert.Throws<System.NullReferenceException>(() =>
            {
                var attack = AttackContext.Scaled(null, 1f, 1f);
                var defender = new DefenderInfo(_defender, _defenderHealth);
                DamageCalculator.Calculate(attack, defender);
            });
        }

        #endregion

        #region Final Damage Tests

        [Test]
        public void Calculate_TrueDamage_IgnoresDefense()
        {
            var defenderWithDefense = new MockCombatant(50f, 0f, 1.5f, defense: 100f);
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f, type: DamageType.True);
            var defender = new DefenderInfo(defenderWithDefense, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // True damage ignores defense
            Assert.AreEqual(100f, result.FinalDamage);
        }

        [Test]
        public void Calculate_MinimumDamage_NeverBelowOne()
        {
            var defenderWithHighDefense = new MockCombatant(50f, 0f, 1.5f, defense: 100000f);
            var attack = AttackContext.Fixed(_attacker, damage: 1f, multiplier: 1f);
            var defender = new DefenderInfo(defenderWithHighDefense, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            Assert.AreEqual(1f, result.FinalDamage);
        }

        [Test]
        public void Calculate_CriticalChance0_NeverCritical()
        {
            var attackerNoCrit = new MockCombatant(100f, criticalChance: 0f, criticalMultiplier: 2f, defense: 0f);
            var attack = AttackContext.Fixed(attackerNoCrit, damage: 100f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            // Run multiple times to ensure no critical hits occur
            for (int i = 0; i < 10; i++)
            {
                var result = DamageCalculator.Calculate(attack, defender);
                Assert.IsFalse(result.IsCritical);
                Assert.AreEqual(100f, result.FinalDamage);
            }
        }

        [Test]
        public void Calculate_CriticalChance100_AlwaysCritical()
        {
            var attackerAlwaysCrit = new MockCombatant(100f, criticalChance: 1f, criticalMultiplier: 2f, defense: 0f);
            var attack = AttackContext.Fixed(attackerAlwaysCrit, damage: 100f, multiplier: 1f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            // Run multiple times to ensure critical always occurs
            for (int i = 0; i < 10; i++)
            {
                var result = DamageCalculator.Calculate(attack, defender);
                Assert.IsTrue(result.IsCritical);
                Assert.AreEqual(200f, result.FinalDamage);
            }
        }

        [Test]
        public void Calculate_ComboMultiplier_AppliedCorrectly()
        {
            // Combo multiplier is passed via AttackContext.Multiplier
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1.5f);
            var defender = new DefenderInfo(_defender, _defenderHealth);

            var result = DamageCalculator.Calculate(attack, defender);

            // 100 * 1.5 = 150
            Assert.AreEqual(150f, result.FinalDamage);
        }

        #endregion

        #region Mock Classes

        private class MockCombatant : ICombatant
        {
            public Transform Transform => null;
            public CombatStats Stats { get; }
            public CombatTeam Team => CombatTeam.Player;

            public MockCombatant(float attackDamage, float criticalChance, float criticalMultiplier, float defense)
            {
                Stats = new CombatStats(attackDamage, criticalChance, criticalMultiplier, defense);
            }
        }

        private class MockHealthProvider : IHealthProvider
        {
            public float CurrentHealth { get; }
            public float MaxHealth { get; }

            public MockHealthProvider(float currentHealth, float maxHealth)
            {
                CurrentHealth = currentHealth;
                MaxHealth = maxHealth;
            }
        }

        #endregion
    }
}
