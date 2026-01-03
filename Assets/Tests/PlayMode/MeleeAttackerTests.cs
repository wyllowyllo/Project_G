using System.Collections;
using Combat.Attack;
using Combat.Core;
using Combat.Data;
using Combat.Damage;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    [TestFixture]
    public class MeleeAttackerTests
    {
        private GameObject _attackerObject;
        private Combatant _combatant;
        private MeleeAttacker _meleeAttacker;
        private HitboxTrigger _hitbox;

        [SetUp]
        public void SetUp()
        {
            _attackerObject = new GameObject("TestAttacker");
            _attackerObject.AddComponent<Health>();
            _combatant = _attackerObject.AddComponent<Combatant>();

            // Create hitbox child object
            var hitboxObject = new GameObject("Hitbox");
            hitboxObject.transform.SetParent(_attackerObject.transform);
            var collider = hitboxObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            _hitbox = hitboxObject.AddComponent<HitboxTrigger>();

            _meleeAttacker = _attackerObject.AddComponent<MeleeAttacker>();
            SetMeleeAttackerHitbox(_meleeAttacker, _hitbox);
        }

        [TearDown]
        public void TearDown()
        {
            if (_attackerObject != null)
            {
                Object.DestroyImmediate(_attackerObject);
            }
        }

        #region CanAttack Tests

        [Test]
        public void CanAttack_WhenReady_ReturnsTrue()
        {
            Assert.IsTrue(_meleeAttacker.CanAttack);
        }

        [Test]
        public void CanAttack_WhenAttacking_ReturnsFalse()
        {
            _meleeAttacker.TryAttack();

            Assert.IsFalse(_meleeAttacker.CanAttack);
        }

        [Test]
        public void CanAttack_WhenStunned_ReturnsFalse()
        {
            _combatant.SetStunned(1f);

            Assert.IsFalse(_meleeAttacker.CanAttack);
        }

        [Test]
        public void CanAttack_WhenDead_ReturnsFalse()
        {
            var damageInfo = CreateDamageInfo(_combatant.MaxHealth);
            _combatant.TakeDamage(damageInfo);

            Assert.IsFalse(_meleeAttacker.CanAttack);
        }

        #endregion

        #region TryAttack Tests

        [Test]
        public void TryAttack_WhenCanAttack_ReturnsTrue()
        {
            bool result = _meleeAttacker.TryAttack();

            Assert.IsTrue(result);
        }

        [Test]
        public void TryAttack_WhenCannotAttack_ReturnsFalse()
        {
            _meleeAttacker.TryAttack(); // Start attacking

            bool result = _meleeAttacker.TryAttack(); // Try again while attacking

            Assert.IsFalse(result);
        }

        [Test]
        public void Attack_CallsTryAttack()
        {
            int comboEventCount = 0;
            _meleeAttacker.OnComboAttack += (step, mult) => comboEventCount++;

            _meleeAttacker.Attack();

            Assert.AreEqual(1, comboEventCount);
            Assert.IsTrue(_meleeAttacker.IsAttacking);
        }

        [Test]
        public void TryAttack_TriggersOnComboAttack()
        {
            bool eventFired = false;
            int receivedStep = 0;
            float receivedMultiplier = 0f;
            _meleeAttacker.OnComboAttack += (step, mult) =>
            {
                eventFired = true;
                receivedStep = step;
                receivedMultiplier = mult;
            };

            _meleeAttacker.TryAttack();

            Assert.IsTrue(eventFired);
            Assert.AreEqual(1, receivedStep);
            Assert.AreEqual(1.0f, receivedMultiplier);
        }

        #endregion

        #region Combo Tests

        [Test]
        public void TryAttack_WithinComboWindow_ContinuesCombo()
        {
            _meleeAttacker.TryAttack(); // Step 1
            _meleeAttacker.OnAttackAnimationEnd(); // End attack animation

            _meleeAttacker.TryAttack(); // Step 2

            Assert.AreEqual(2, _meleeAttacker.CurrentComboStep);
        }

        [UnityTest]
        public IEnumerator TryAttack_AfterComboWindow_ResetsAndTriggersEvent()
        {
            bool resetFired = false;
            _meleeAttacker.OnComboReset += () => resetFired = true;

            _meleeAttacker.TryAttack(); // Step 1
            _meleeAttacker.OnAttackAnimationEnd();

            // Wait for combo window to expire (default 0.8s + safety margin)
            yield return new WaitForSeconds(1.2f);

            _meleeAttacker.TryAttack(); // Should reset and start at step 1

            Assert.IsTrue(resetFired);
            Assert.AreEqual(1, _meleeAttacker.CurrentComboStep);
        }

        [Test]
        public void TryAttack_MaxComboReached_WrapsToFirst()
        {
            // Default max combo steps is 3
            _meleeAttacker.TryAttack(); // Step 1
            _meleeAttacker.OnAttackAnimationEnd();

            _meleeAttacker.TryAttack(); // Step 2
            _meleeAttacker.OnAttackAnimationEnd();

            _meleeAttacker.TryAttack(); // Step 3
            _meleeAttacker.OnAttackAnimationEnd();

            _meleeAttacker.TryAttack(); // Step 4 -> wraps to 1

            Assert.AreEqual(1, _meleeAttacker.CurrentComboStep);
        }

        [Test]
        public void TryAttack_NoComboSettings_UsesDefaults()
        {
            // Without ComboSettings, uses DEFAULT_MAX_COMBO_STEPS = 3
            // and _defaultComboMultipliers = { 1.0f, 1.1f, 1.3f }
            Assert.AreEqual(3, _meleeAttacker.MaxComboSteps);

            _meleeAttacker.TryAttack();
            Assert.AreEqual(1.0f, _meleeAttacker.CurrentMultiplier);

            _meleeAttacker.OnAttackAnimationEnd();
            _meleeAttacker.TryAttack();
            Assert.AreEqual(1.1f, _meleeAttacker.CurrentMultiplier);
        }

        [Test]
        public void ResetCombo_ResetsToInitialState()
        {
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackAnimationEnd();
            _meleeAttacker.TryAttack();

            _meleeAttacker.ResetCombo();

            Assert.AreEqual(0, _meleeAttacker.CurrentComboStep);
            Assert.AreEqual(1f, _meleeAttacker.CurrentMultiplier);
            Assert.IsFalse(_meleeAttacker.IsAttacking);
        }

        [UnityTest]
        public IEnumerator Update_ComboExpired_AutoResetsCombo()
        {
            bool resetFired = false;
            _meleeAttacker.OnComboReset += () => resetFired = true;

            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackAnimationEnd();
            Assert.AreEqual(1, _meleeAttacker.CurrentComboStep);

            // Wait for combo window to expire (default 0.8s + safety margin)
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(resetFired);
            Assert.AreEqual(0, _meleeAttacker.CurrentComboStep);
        }

        #endregion

        #region Hitbox Management Tests

        [Test]
        public void OnAttackHitStart_EnablesHitboxWithContext()
        {
            _meleeAttacker.TryAttack();

            _meleeAttacker.OnAttackHitStart();

            // Hitbox collider should be enabled
            var collider = _hitbox.GetComponent<Collider>();
            Assert.IsTrue(collider.enabled);
        }

        [Test]
        public void OnAttackHitEnd_DisablesHitbox()
        {
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();

            _meleeAttacker.OnAttackHitEnd();

            var collider = _hitbox.GetComponent<Collider>();
            Assert.IsFalse(collider.enabled);
        }

        [Test]
        public void OnAttackHitStart_WithNullHitbox_NoException()
        {
            SetMeleeAttackerHitbox(_meleeAttacker, null);

            _meleeAttacker.TryAttack();

            Assert.DoesNotThrow(() => _meleeAttacker.OnAttackHitStart());
        }

        [Test]
        public void OnAttackHitEnd_WithNullHitbox_NoException()
        {
            SetMeleeAttackerHitbox(_meleeAttacker, null);

            Assert.DoesNotThrow(() => _meleeAttacker.OnAttackHitEnd());
        }

        #endregion

        #region OnHit Event Tests

        [UnityTest]
        public IEnumerator OnHit_TriggeredOnHitboxHit()
        {
            // Wait for MeleeAttacker.Start() to subscribe to HitboxTrigger.OnHit
            yield return null;

            // Create a target
            var targetObject = new GameObject("Target");
            targetObject.AddComponent<Health>();
            var targetCombatant = targetObject.AddComponent<Combatant>();
            var targetCollider = targetObject.AddComponent<BoxCollider>();

            // Add Rigidbody for trigger detection (required by Unity physics)
            var rb = targetObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Set target as enemy team
            SetCombatantTeam(targetCombatant, CombatTeam.Enemy);

            // Set attacker as player team
            SetCombatantTeam(_combatant, CombatTeam.Player);

            // Position target within hitbox range
            targetObject.transform.position = _attackerObject.transform.position + Vector3.forward * 0.5f;

            // Wait for target's Start() to be called
            yield return null;

            // Subscribe to OnHit
            bool hitFired = false;
            IDamageable hitTarget = null;
            _meleeAttacker.OnHit += (target, info) =>
            {
                hitFired = true;
                hitTarget = target;
            };

            // Perform attack
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();

            // Wait for physics
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Cleanup
            Object.DestroyImmediate(targetObject);

            Assert.IsTrue(hitFired);
            Assert.AreEqual(targetCombatant, hitTarget);
        }

        #endregion

        #region Helper Methods

        private DamageInfo CreateDamageInfo(float amount, bool isCritical = false)
        {
            var hitContext = new HitContext(Vector3.zero, Vector3.forward, DamageType.Normal);
            return new DamageInfo(amount, isCritical, null, hitContext);
        }

        private void SetMeleeAttackerHitbox(MeleeAttacker attacker, HitboxTrigger hitbox)
        {
            var serializedObject = new UnityEditor.SerializedObject(attacker);
            serializedObject.FindProperty("_hitbox").objectReferenceValue = hitbox;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void SetCombatantTeam(Combatant combatant, CombatTeam team)
        {
            var serializedObject = new UnityEditor.SerializedObject(combatant);
            serializedObject.FindProperty("_team").enumValueIndex = (int)team;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion
    }
}
