using Combat.Core;
using Combat.Damage;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class DamageProcessorTests
    {
        private GameObject _attackerObject;
        private GameObject _defenderObject;
        private MockCombatant _attacker;
        private MockCombatant _defender;
        private MockHealthProvider _defenderHealth;
        private MockDamageable _damageable;
        private BoxCollider _defenderCollider;

        [SetUp]
        public void SetUp()
        {
            _attackerObject = new GameObject("Attacker");
            _attackerObject.transform.position = Vector3.zero;

            _defenderObject = new GameObject("Defender");
            _defenderObject.transform.position = new Vector3(5f, 0f, 0f);
            _defenderCollider = _defenderObject.AddComponent<BoxCollider>();

            _attacker = new MockCombatant(
                attackDamage: 100f,
                criticalChance: 0f,
                criticalMultiplier: 2f,
                defense: 0f,
                transform: _attackerObject.transform
            );

            _defender = new MockCombatant(
                attackDamage: 50f,
                criticalChance: 0f,
                criticalMultiplier: 1.5f,
                defense: 0f,
                transform: _defenderObject.transform
            );

            _defenderHealth = new MockHealthProvider(currentHealth: 100f, maxHealth: 100f);
            _damageable = new MockDamageable();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_attackerObject);
            Object.DestroyImmediate(_defenderObject);
        }

        #region Process Tests

        [Test]
        public void Process_ReturnsDamageInfo_WithCorrectAmount()
        {
            var attack = AttackContext.Scaled(_attacker, baseMultiplier: 1f, buffMultiplier: 1f);
            var hitInfo = CreateHitInfo();

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            Assert.AreEqual(100f, result.Amount);
        }

        [Test]
        public void Process_WithDefense_AppliesDefenseReduction()
        {
            var defenderWithDefense = new MockCombatant(50f, 0f, 1.5f, defense: 100f, _defenderObject.transform);
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f);
            var hitInfo = new HitInfo(_damageable, defenderWithDefense, _defenderHealth, _defenderCollider);

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            // reduction = 100 / (100 + 100) = 0.5
            // 100 * (1 - 0.5) = 50
            Assert.AreEqual(50f, result.Amount);
        }

        [Test]
        public void Process_TrueDamage_IgnoresDefense()
        {
            var defenderWithDefense = new MockCombatant(50f, 0f, 1.5f, defense: 100f, _defenderObject.transform);
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f, type: DamageType.True);
            var hitInfo = new HitInfo(_damageable, defenderWithDefense, _defenderHealth, _defenderCollider);

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            Assert.AreEqual(100f, result.Amount);
        }

        [Test]
        public void Process_ReturnsDamageInfo_WithCorrectDamageType()
        {
            var attack = AttackContext.Fixed(_attacker, damage: 100f, multiplier: 1f, type: DamageType.True);
            var hitInfo = CreateHitInfo();

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            Assert.AreEqual(DamageType.True, result.Type);
        }

        [Test]
        public void Process_CalculatesHitPoint_FromAttackerPosition()
        {
            _attackerObject.transform.position = new Vector3(0f, 0f, 0f);
            _defenderObject.transform.position = new Vector3(10f, 0f, 0f);

            var attack = AttackContext.Scaled(_attacker, baseMultiplier: 1f);
            var hitInfo = CreateHitInfo();

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            // HitPoint should be the closest point on the collider to the attacker
            // BoxCollider center is at (10, 0, 0), size is (1, 1, 1)
            // Closest point to (0, 0, 0) is approximately (9.5, 0, 0)
            Assert.That(result.HitPoint.x, Is.GreaterThan(0f));
        }

        [Test]
        public void Process_CalculatesHitDirection_Normalized()
        {
            _attackerObject.transform.position = new Vector3(0f, 0f, 0f);
            _defenderObject.transform.position = new Vector3(10f, 0f, 0f);

            var attack = AttackContext.Scaled(_attacker, baseMultiplier: 1f);
            var hitInfo = CreateHitInfo();

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            // Direction should be normalized (magnitude ~= 1)
            Assert.That(result.HitDirection.magnitude, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void Process_HitDirection_PointsTowardsDefender()
        {
            _attackerObject.transform.position = new Vector3(0f, 0f, 0f);
            _defenderObject.transform.position = new Vector3(10f, 0f, 0f);

            var attack = AttackContext.Scaled(_attacker, baseMultiplier: 1f);
            var hitInfo = CreateHitInfo();

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            // Direction should point towards the defender (positive X)
            Assert.That(result.HitDirection.x, Is.GreaterThan(0f));
        }

        [Test]
        public void Process_CriticalHit_AppliesCriticalMultiplier()
        {
            var attackerAlwaysCrit = new MockCombatant(100f, criticalChance: 1f, criticalMultiplier: 2f, defense: 0f, _attackerObject.transform);
            var attack = AttackContext.Fixed(attackerAlwaysCrit, damage: 100f, multiplier: 1f);
            var hitInfo = CreateHitInfo();

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            Assert.IsTrue(result.IsCritical);
            Assert.AreEqual(200f, result.Amount);
        }

        [Test]
        public void Process_NoCritical_DoesNotApplyCriticalMultiplier()
        {
            var attackerNoCrit = new MockCombatant(100f, criticalChance: 0f, criticalMultiplier: 2f, defense: 0f, _attackerObject.transform);
            var attack = AttackContext.Fixed(attackerNoCrit, damage: 100f, multiplier: 1f);
            var hitInfo = CreateHitInfo();

            var result = DamageProcessor.Process(attack, hitInfo, _attackerObject.transform.position);

            Assert.IsFalse(result.IsCritical);
            Assert.AreEqual(100f, result.Amount);
        }

        #endregion

        #region Helper Methods

        private HitInfo CreateHitInfo()
        {
            return new HitInfo(_damageable, _defender, _defenderHealth, _defenderCollider);
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

            public float GetDefense() => Stats.Defense.Value;

            public bool IsAlly(CombatTeam team) => Team == team;
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

        private class MockDamageable : IDamageable
        {
            public DamageInfo? LastDamageInfo { get; private set; }

            public void TakeDamage(DamageInfo damageInfo)
            {
                LastDamageInfo = damageInfo;
            }

            public bool CanTakeDamage { get; }
        }

        #endregion
    }
}
