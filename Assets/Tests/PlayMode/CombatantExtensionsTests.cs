using System.Collections;
using Combat.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class CombatantExtensionsTests
    {
        private GameObject _attackerObject;
        private GameObject _targetObject;
        private Combatant _attacker;
        private Combatant _target;

        [SetUp]
        public void SetUp()
        {
            _attackerObject = new GameObject("TestAttacker");
            _attackerObject.AddComponent<Health>();
            _attacker = _attackerObject.AddComponent<Combatant>();
            _attacker.SetTeamForTest(CombatTeam.Player);

            _targetObject = new GameObject("TestTarget");
            _targetObject.AddComponent<Health>();
            _target = _targetObject.AddComponent<Combatant>();
            _target.SetTeamForTest(CombatTeam.Enemy);
        }

        [TearDown]
        public void TearDown()
        {
            if (_attackerObject != null)
            {
                Object.DestroyImmediate(_attackerObject);
            }
            if (_targetObject != null)
            {
                Object.DestroyImmediate(_targetObject);
            }
        }

        #region DealDamageTo Tests

        [Test]
        public void DealDamageTo_ValidTarget_AppliesDamage()
        {
            float initialHealth = _target.CurrentHealth;
            float damage = 30f;

            _attacker.DealDamageTo(_target, damage);

            Assert.Less(_target.CurrentHealth, initialHealth);
        }

        [Test]
        public void DealDamageTo_NullTarget_NoException()
        {
            Assert.DoesNotThrow(() => _attacker.DealDamageTo(null, 30f));
        }

        [Test]
        public void DealDamageTo_DeadTarget_NoException()
        {
            // Kill the target first
            _target.TakeDamage(CreateDamageInfo(_target.MaxHealth));
            Assert.IsFalse(_target.IsAlive);

            Assert.DoesNotThrow(() => _attacker.DealDamageTo(_target, 30f));
        }

        #endregion

        #region TakeDamageFrom Tests

        [Test]
        public void TakeDamageFrom_AppliesEnvironmentalDamage()
        {
            float initialHealth = _target.CurrentHealth;
            float damage = 25f;

            _target.TakeDamageFrom(damage);

            Assert.AreEqual(initialHealth - damage, _target.CurrentHealth);
        }

        [Test]
        public void TakeDamageFrom_NullTarget_NoException()
        {
            Combatant nullTarget = null;
            Assert.DoesNotThrow(() => nullTarget.TakeDamageFrom(30f));
        }

        [Test]
        public void TakeDamageFrom_CorrectDamageType()
        {
            DamageInfo receivedInfo = default;
            _target.OnDamaged += info => receivedInfo = info;

            _target.TakeDamageFrom(30f, DamageType.Normal);

            Assert.AreEqual(DamageType.Normal, receivedInfo.Type);
        }

        #endregion

        #region DestroyOnDeath Tests

        [UnityTest]
        public IEnumerator DestroyOnDeath_DestroysGameObject()
        {
            var testObject = new GameObject("TestDestroyOnDeath");
            testObject.AddComponent<Health>();
            var combatant = testObject.AddComponent<Combatant>();

            combatant.DestroyOnDeath();

            // Kill the combatant
            combatant.TakeDamage(CreateDamageInfo(combatant.MaxHealth));

            yield return null; // Wait for destruction

            Assert.IsTrue(testObject == null);
        }

        #endregion

        #region DisableOnDeath Tests

        [Test]
        public void DisableOnDeath_DisablesGameObject()
        {
            _target.DisableOnDeath();

            // Kill the combatant
            _target.TakeDamage(CreateDamageInfo(_target.MaxHealth));

            Assert.IsFalse(_targetObject.activeSelf);
        }

        #endregion

        #region LogDamage Tests

        [Test]
        public void LogDamage_OutputsCorrectFormat()
        {
            _target.LogDamage();

            LogAssert.Expect(LogType.Log, $"[{_target.name}] Took 50.0 damage");
            _target.TakeDamage(CreateDamageInfo(50f));
        }

        #endregion

        #region ApplyKnockbackOnDamage Tests

        [UnityTest]
        public IEnumerator ApplyKnockbackOnDamage_AppliesForce()
        {
            var rb = _targetObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0f;

            Vector3 initialPosition = _targetObject.transform.position;
            float force = 100f;

            _target.ApplyKnockbackOnDamage(rb, force);

            // Create damage with hit direction
            var hitContext = HitContext.FromCollision(Vector3.zero, Vector3.forward, DamageType.Normal);
            var damageInfo = new DamageInfo(10f, false, hitContext);
            _target.TakeDamage(damageInfo);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.AreNotEqual(initialPosition, _targetObject.transform.position);
        }

        #endregion

        #region Helper Methods

        private DamageInfo CreateDamageInfo(float amount, bool isCritical = false)
        {
            var hitContext = HitContext.FromCollision(Vector3.zero, Vector3.forward, DamageType.Normal);
            return new DamageInfo(amount, isCritical, hitContext);
        }

        #endregion
    }
}
