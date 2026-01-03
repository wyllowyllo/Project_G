using Combat.Core;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class DefenderInfoTests
    {
        private MockCombatant _combatant;
        private MockHealthProvider _healthProvider;

        [SetUp]
        public void SetUp()
        {
            _combatant = new MockCombatant(50f, 0.1f, 1.5f, 10f);
            _healthProvider = new MockHealthProvider(80f, 100f);
        }

        #region Constructor Tests

        [Test]
        public void DefenderInfo_Constructor_StoresCorrectValues()
        {
            var defenderInfo = new DefenderInfo(_combatant, _healthProvider);

            Assert.AreEqual(_combatant, defenderInfo.Combatant);
            Assert.AreEqual(_healthProvider, defenderInfo.Health);
        }

        #endregion

        #region Factory Method Tests

        [Test]
        public void DefenderInfo_FromFactory_CreatesCorrectly()
        {
            var defenderInfo = DefenderInfo.From(_combatant, _healthProvider);

            Assert.AreEqual(_combatant, defenderInfo.Combatant);
            Assert.AreEqual(_healthProvider, defenderInfo.Health);
        }

        #endregion

        #region Mock Classes

        private class MockCombatant : ICombatant
        {
            public Transform Transform => null;
            public CombatStats Stats { get; }
            public CombatTeam Team => CombatTeam.Enemy;

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
