using Combat.Core;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class HealthTests
    {
        private GameObject _gameObject;
        private Health _health;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TestHealth");
            _health = _gameObject.AddComponent<Health>();
            // After AddComponent, Awake() is called automatically
            // _currentHealth = _maxHealth (100f by default)
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        #region TakeDamage Tests

        [Test]
        public void TakeDamage_ReducesCurrentHealth()
        {
            _health.TakeDamage(30f);

            Assert.AreEqual(70f, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ZeroOrNegative_NoEffect()
        {
            float initialHealth = _health.CurrentHealth;

            _health.TakeDamage(0f);
            Assert.AreEqual(initialHealth, _health.CurrentHealth);

            _health.TakeDamage(-10f);
            Assert.AreEqual(initialHealth, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_WhenDead_NoEffect()
        {
            _health.TakeDamage(100f); // Kill
            Assert.IsFalse(_health.IsAlive);

            float healthAfterDeath = _health.CurrentHealth;
            _health.TakeDamage(50f);

            Assert.AreEqual(healthAfterDeath, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ExactKill_SetsIsAliveFalse()
        {
            _health.TakeDamage(100f);

            Assert.AreEqual(0f, _health.CurrentHealth);
            Assert.IsFalse(_health.IsAlive);
        }

        [Test]
        public void TakeDamage_ExactKill_TriggersOnDeath()
        {
            bool deathTriggered = false;
            _health.OnDeath += () => deathTriggered = true;

            _health.TakeDamage(100f);

            Assert.IsTrue(deathTriggered);
        }

        [Test]
        public void TakeDamage_Overkill_ClampsToZero()
        {
            _health.TakeDamage(150f);

            Assert.AreEqual(0f, _health.CurrentHealth);
            Assert.IsFalse(_health.IsAlive);
        }

        [Test]
        public void TakeDamage_TriggersOnDamaged_WithCorrectAmount()
        {
            bool eventFired = false;
            float receivedAmount = 0f;
            _health.OnDamaged += (amount) =>
            {
                eventFired = true;
                receivedAmount = amount;
            };

            _health.TakeDamage(50f);

            Assert.IsTrue(eventFired);
            Assert.AreEqual(50f, receivedAmount);
        }

        #endregion

        #region Heal Tests

        [Test]
        public void Heal_IncreasesCurrentHealth()
        {
            _health.TakeDamage(50f);
            Assert.AreEqual(50f, _health.CurrentHealth);

            _health.Heal(30f);

            Assert.AreEqual(80f, _health.CurrentHealth);
        }

        [Test]
        public void Heal_ClampedToMaxHealth()
        {
            _health.TakeDamage(30f);
            Assert.AreEqual(70f, _health.CurrentHealth);

            _health.Heal(50f);

            Assert.AreEqual(100f, _health.CurrentHealth);
        }

        [Test]
        public void Heal_ZeroOrNegative_NoEffect()
        {
            _health.TakeDamage(50f);
            float healthBefore = _health.CurrentHealth;

            _health.Heal(0f);
            Assert.AreEqual(healthBefore, _health.CurrentHealth);

            _health.Heal(-10f);
            Assert.AreEqual(healthBefore, _health.CurrentHealth);
        }

        [Test]
        public void Heal_WhenDead_NoEffect()
        {
            _health.TakeDamage(100f);
            Assert.IsFalse(_health.IsAlive);

            _health.Heal(50f);

            Assert.AreEqual(0f, _health.CurrentHealth);
            Assert.IsFalse(_health.IsAlive);
        }

        [Test]
        public void Heal_TriggersOnHealed_WithActualAmount()
        {
            _health.TakeDamage(30f);

            bool eventFired = false;
            float receivedAmount = 0f;
            _health.OnHealed += (amount) =>
            {
                eventFired = true;
                receivedAmount = amount;
            };

            _health.Heal(50f); // Only 30 can be healed (100 max - 70 current = 30)

            Assert.IsTrue(eventFired);
            Assert.AreEqual(30f, receivedAmount);
        }

        #endregion

        #region SetMaxHealth Tests

        [Test]
        public void SetMaxHealth_UpdatesMaxHealth()
        {
            _health.SetMaxHealth(150f);

            Assert.AreEqual(150f, _health.MaxHealth);
        }

        [Test]
        public void SetMaxHealth_ZeroOrNegative_ClampsToOne()
        {
            _health.SetMaxHealth(0f);
            Assert.AreEqual(1f, _health.MaxHealth);

            _health.SetMaxHealth(-50f);
            Assert.AreEqual(1f, _health.MaxHealth);
        }

        [Test]
        public void SetMaxHealth_ClampsCurrentHealth()
        {
            // Start at 100/100
            _health.TakeDamage(20f); // Now at 80/100

            _health.SetMaxHealth(50f);

            Assert.AreEqual(50f, _health.MaxHealth);
            Assert.AreEqual(50f, _health.CurrentHealth);
        }

        [Test]
        public void SetMaxHealth_WithHealToFull_RestoresHealth()
        {
            _health.TakeDamage(50f);
            Assert.AreEqual(50f, _health.CurrentHealth);

            _health.SetMaxHealth(200f, healToFull: true);

            Assert.AreEqual(200f, _health.MaxHealth);
            Assert.AreEqual(200f, _health.CurrentHealth);
        }

        [Test]
        public void SetMaxHealth_DoesNotTriggerEvents()
        {
            bool damagedFired = false;
            bool healedFired = false;
            _health.OnDamaged += (_) => damagedFired = true;
            _health.OnHealed += (_) => healedFired = true;

            // Reduce max health (which clamps current health)
            _health.SetMaxHealth(50f);

            Assert.IsFalse(damagedFired);
            Assert.IsFalse(healedFired);

            // Increase max health with healToFull
            _health.SetMaxHealth(200f, healToFull: true);

            Assert.IsFalse(damagedFired);
            Assert.IsFalse(healedFired);
        }

        #endregion
    }
}
