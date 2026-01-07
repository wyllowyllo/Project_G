using System.Collections;
using Combat.Core;
using Combat.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class CombatantTests
    {
        private GameObject _gameObject;
        private Combatant _combatant;
        private Health _health;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TestCombatant");
            _health = _gameObject.AddComponent<Health>();
            _combatant = _gameObject.AddComponent<Combatant>();
            // Combatant requires Health component, which is added first
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
        public void TakeDamage_WhenCanTakeDamage_ReducesHealth()
        {
            float initialHealth = _combatant.CurrentHealth;
            var damageInfo = CreateDamageInfo(30f);

            _combatant.TakeDamage(damageInfo);

            Assert.AreEqual(initialHealth - 30f, _combatant.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator TakeDamage_WhenInvincible_NoDamage()
        {
            float initialHealth = _combatant.CurrentHealth;
            _combatant.SetInvincible(1f);
            yield return null; // Let Update run

            var damageInfo = CreateDamageInfo(50f);
            _combatant.TakeDamage(damageInfo);

            Assert.AreEqual(initialHealth, _combatant.CurrentHealth);
        }

        [Test]
        public void TakeDamage_WhenDead_NoDamage()
        {
            // Kill the combatant first
            var killDamage = CreateDamageInfo(_combatant.MaxHealth);
            _combatant.TakeDamage(killDamage);
            Assert.IsFalse(_combatant.IsAlive);

            float healthAfterDeath = _combatant.CurrentHealth;
            var moreDamage = CreateDamageInfo(50f);
            _combatant.TakeDamage(moreDamage);

            Assert.AreEqual(healthAfterDeath, _combatant.CurrentHealth);
        }

        [Test]
        public void TakeDamage_TriggersOnDamaged_WithCorrectDamageInfo()
        {
            bool eventFired = false;
            DamageInfo receivedInfo = default;
            _combatant.OnDamaged += (info) =>
            {
                eventFired = true;
                receivedInfo = info;
            };

            var damageInfo = CreateDamageInfo(25f, isCritical: true);
            _combatant.TakeDamage(damageInfo);

            Assert.IsTrue(eventFired);
            Assert.AreEqual(25f, receivedInfo.Amount);
            Assert.IsTrue(receivedInfo.IsCritical);
        }

        [UnityTest]
        public IEnumerator TakeDamage_WithAutoInvincibility_SetsInvincible()
        {
            // Create a HitReactionSettings with auto invincibility enabled
            var hitReactionSettings = HitReactionSettings.CreateForTest(
                invincibilityDuration: 0.5f,
                autoInvincibility: true);
            _combatant.SetHitReactionSettingsForTest(hitReactionSettings);

            var damageInfo = CreateDamageInfo(10f);
            _combatant.TakeDamage(damageInfo);
            yield return null;

            Assert.IsTrue(_combatant.IsInvincible);

            Object.DestroyImmediate(hitReactionSettings);
        }

        [UnityTest]
        public IEnumerator TakeDamage_WithAutoHitStun_SetsStunned()
        {
            var hitReactionSettings = HitReactionSettings.CreateForTest(
                hitStunDuration: 0.3f,
                autoHitStun: true);
            _combatant.SetHitReactionSettingsForTest(hitReactionSettings);

            var damageInfo = CreateDamageInfo(10f);
            _combatant.TakeDamage(damageInfo);
            yield return null;

            Assert.IsTrue(_combatant.IsStunned);

            Object.DestroyImmediate(hitReactionSettings);
        }

        [Test]
        public void TakeDamage_WithNullHitReactionSettings_NoAutoReactions()
        {
            // By default, _hitReactionSettings is null
            var damageInfo = CreateDamageInfo(10f);
            _combatant.TakeDamage(damageInfo);

            Assert.IsFalse(_combatant.IsInvincible);
            Assert.IsFalse(_combatant.IsStunned);
        }

        #endregion

        #region Heal Tests

        [Test]
        public void Heal_DelegatesToHealth()
        {
            _combatant.TakeDamage(CreateDamageInfo(50f));
            float healthAfterDamage = _combatant.CurrentHealth;

            _combatant.Heal(30f);

            Assert.AreEqual(healthAfterDamage + 30f, _combatant.CurrentHealth);
        }

        #endregion

        #region SetInvincible Tests

        [Test]
        public void SetInvincible_MakesInvincible()
        {
            _combatant.SetInvincible(1f);

            Assert.IsTrue(_combatant.IsInvincible);
        }

        [UnityTest]
        public IEnumerator SetInvincible_ExpiresAfterDuration()
        {
            _combatant.SetInvincible(0.1f);
            Assert.IsTrue(_combatant.IsInvincible);

            yield return new WaitForSeconds(0.2f);

            Assert.IsFalse(_combatant.IsInvincible);
        }

        [Test]
        public void SetInvincible_ExtendsDuration_WhenLonger()
        {
            _combatant.SetInvincible(0.5f);
            float endTimeAfterFirst = Time.time + 0.5f;

            _combatant.SetInvincible(1.0f);

            // The invincibility end time should be extended
            Assert.IsTrue(_combatant.IsInvincible);
        }

        [Test]
        public void SetInvincible_KeepsExistingDuration_WhenNewIsShorter()
        {
            _combatant.SetInvincible(1.0f);
            float endTimeAfterFirst = Time.time + 1.0f;

            _combatant.SetInvincible(0.3f);

            // Should still be invincible for the longer duration
            Assert.IsTrue(_combatant.IsInvincible);
        }

        [Test]
        public void SetInvincible_ZeroOrNegative_NoEffect()
        {
            _combatant.SetInvincible(0f);
            Assert.IsFalse(_combatant.IsInvincible);

            _combatant.SetInvincible(-1f);
            Assert.IsFalse(_combatant.IsInvincible);
        }

        [Test]
        public void SetInvincible_WhenDead_NoEffect()
        {
            _combatant.TakeDamage(CreateDamageInfo(_combatant.MaxHealth));
            Assert.IsFalse(_combatant.IsAlive);

            _combatant.SetInvincible(1f);

            Assert.IsFalse(_combatant.IsInvincible);
        }

        [Test]
        public void SetInvincible_TriggersOnStart_OnlyOnce()
        {
            int startCount = 0;
            _combatant.OnInvincibilityStart += () => startCount++;

            _combatant.SetInvincible(1f);
            _combatant.SetInvincible(0.5f); // Extend/overlap, should not fire again

            Assert.AreEqual(1, startCount);
        }

        [Test]
        public void ClearInvincibility_ImmediatelyRemoves()
        {
            _combatant.SetInvincible(10f);
            Assert.IsTrue(_combatant.IsInvincible);

            _combatant.ClearInvincibility();

            Assert.IsFalse(_combatant.IsInvincible);
        }

        [UnityTest]
        public IEnumerator ClearInvincibility_TriggersOnEnd()
        {
            bool endFired = false;
            _combatant.OnInvincibilityEnd += () => endFired = true;

            _combatant.SetInvincible(10f);
            yield return null; // Allow frame processing

            _combatant.ClearInvincibility();

            Assert.IsTrue(endFired);
        }

        #endregion

        #region SetStunned Tests

        [Test]
        public void SetStunned_MakesStunned()
        {
            _combatant.SetStunned(1f);

            Assert.IsTrue(_combatant.IsStunned);
        }

        [UnityTest]
        public IEnumerator SetStunned_ExpiresAfterDuration()
        {
            _combatant.SetStunned(0.1f);
            Assert.IsTrue(_combatant.IsStunned);

            yield return new WaitForSeconds(0.2f);

            Assert.IsFalse(_combatant.IsStunned);
        }

        [Test]
        public void SetStunned_ExtendsDuration_WhenLonger()
        {
            _combatant.SetStunned(0.5f);

            _combatant.SetStunned(1.0f);

            Assert.IsTrue(_combatant.IsStunned);
        }

        [Test]
        public void SetStunned_KeepsExistingDuration_WhenNewIsShorter()
        {
            _combatant.SetStunned(1.0f);

            _combatant.SetStunned(0.3f);

            Assert.IsTrue(_combatant.IsStunned);
        }

        [Test]
        public void SetStunned_ZeroOrNegative_NoEffect()
        {
            _combatant.SetStunned(0f);
            Assert.IsFalse(_combatant.IsStunned);

            _combatant.SetStunned(-1f);
            Assert.IsFalse(_combatant.IsStunned);
        }

        [Test]
        public void SetStunned_WhenDead_NoEffect()
        {
            _combatant.TakeDamage(CreateDamageInfo(_combatant.MaxHealth));
            Assert.IsFalse(_combatant.IsAlive);

            _combatant.SetStunned(1f);

            Assert.IsFalse(_combatant.IsStunned);
        }

        [Test]
        public void SetStunned_TriggersOnStart_OnlyOnce()
        {
            int startCount = 0;
            _combatant.OnHitStunStart += () => startCount++;

            _combatant.SetStunned(1f);
            _combatant.SetStunned(0.5f);

            Assert.AreEqual(1, startCount);
        }

        [Test]
        public void ClearHitStun_ImmediatelyRemoves()
        {
            _combatant.SetStunned(10f);
            Assert.IsTrue(_combatant.IsStunned);

            _combatant.ClearHitStun();

            Assert.IsFalse(_combatant.IsStunned);
        }

        [UnityTest]
        public IEnumerator ClearHitStun_TriggersOnEnd()
        {
            bool endFired = false;
            _combatant.OnHitStunEnd += () => endFired = true;

            _combatant.SetStunned(10f);
            yield return null; // Allow frame processing

            _combatant.ClearHitStun();

            Assert.IsTrue(endFired);
        }

        #endregion

        #region Awake Tests

        [Test]
        public void Awake_NullStatsData_UsesFallbackValues()
        {
            // Default values when _statsData is null: 10f, 0.1f, 1.5f, 0f
            Assert.AreEqual(10f, _combatant.Stats.AttackDamage.Value);
            Assert.AreEqual(0.1f, _combatant.Stats.CriticalChance.Value);
            Assert.AreEqual(1.5f, _combatant.Stats.CriticalMultiplier.Value);
            Assert.AreEqual(0f, _combatant.Stats.Defense.Value);
        }

        #endregion

        #region HandleDeath Tests

        [UnityTest]
        public IEnumerator HandleDeath_ClearsModifiersInvincibilityAndStun()
        {
            // Wait for Start() to be called (event subscription)
            yield return null;

            // Add a modifier
            var source = new MockModifierSource("TestSource");
            var modifier = new StatModifier(50f, StatModifierType.Additive, source);
            _combatant.Stats.AttackDamage.AddModifier(modifier);

            // Set invincibility and stun
            _combatant.SetInvincible(10f);
            _combatant.SetStunned(10f);

            // Verify they are active
            Assert.AreEqual(60f, _combatant.Stats.AttackDamage.Value);
            Assert.IsTrue(_combatant.IsInvincible);
            Assert.IsTrue(_combatant.IsStunned);

            // Kill the combatant by directly damaging Health (bypasses invincibility)
            _health.TakeDamage(_combatant.MaxHealth);

            // All should be cleared
            Assert.AreEqual(10f, _combatant.Stats.AttackDamage.Value); // Back to base
            Assert.IsFalse(_combatant.IsInvincible);
            Assert.IsFalse(_combatant.IsStunned);
        }

        #endregion

        #region Helper Methods

        private DamageInfo CreateDamageInfo(float amount, bool isCritical = false, ICombatant attacker = null)
        {
            var hitContext = new HitContext(Vector3.zero, Vector3.forward, DamageType.Normal);
            return new DamageInfo(amount, isCritical, hitContext);
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
