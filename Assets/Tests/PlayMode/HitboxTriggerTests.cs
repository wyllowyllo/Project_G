using System.Collections;
using Combat.Attack;
using Combat.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class HitboxTriggerTests
    {
        private GameObject _attackerObject;
        private Combatant _attacker;
        private HitboxTrigger _hitbox;
        private Collider _hitboxCollider;

        [SetUp]
        public void SetUp()
        {
            _attackerObject = new GameObject("TestAttacker");
            _attackerObject.AddComponent<Health>();
            _attacker = _attackerObject.AddComponent<Combatant>();
            SetCombatantTeam(_attacker, CombatTeam.Player);

            // Create hitbox child object
            var hitboxObject = new GameObject("Hitbox");
            hitboxObject.transform.SetParent(_attackerObject.transform);
            var collider = hitboxObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            _hitbox = hitboxObject.AddComponent<HitboxTrigger>();
            _hitboxCollider = collider;
        }

        [TearDown]
        public void TearDown()
        {
            if (_attackerObject != null)
            {
                Object.DestroyImmediate(_attackerObject);
            }
        }

        #region EnableHitbox Tests

        [Test]
        public void EnableHitbox_EnablesCollider()
        {
            Assert.IsFalse(_hitboxCollider.enabled);

            var attackContext = AttackContext.Scaled(_attacker, 1f);
            _hitbox.EnableHitbox(attackContext);

            Assert.IsTrue(_hitboxCollider.enabled);
        }

        [Test]
        public void EnableHitbox_ClearsHitTargets()
        {
            // This is implicitly tested through the "can hit again after re-enable" behavior
            // We'll test it by hitting a target, disabling, re-enabling, and hitting again

            var targetObject = CreateEnemyTarget();
            var target = targetObject.GetComponent<Combatant>();
            float initialHealth = target.CurrentHealth;

            var attackContext = AttackContext.Fixed(_attacker, 10f);
            _hitbox.EnableHitbox(attackContext);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            // First hit should deal damage
            float healthAfterFirstHit = target.CurrentHealth;
            Assert.Less(healthAfterFirstHit, initialHealth);

            _hitbox.DisableHitbox();
            _hitbox.EnableHitbox(attackContext);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            // Second hit should also deal damage (target was cleared)
            Assert.Less(target.CurrentHealth, healthAfterFirstHit);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void EnableHitbox_StoresAttackContext()
        {
            var targetObject = CreateEnemyTarget();
            var target = targetObject.GetComponent<Combatant>();
            float initialHealth = target.CurrentHealth;

            // Use Fixed damage to verify context is stored
            var attackContext = AttackContext.Fixed(_attacker, 25f);
            _hitbox.EnableHitbox(attackContext);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            // Damage should be applied based on stored context
            Assert.Less(target.CurrentHealth, initialHealth);

            Object.DestroyImmediate(targetObject);
        }

        #endregion

        #region DisableHitbox Tests

        [Test]
        public void DisableHitbox_DisablesCollider()
        {
            var attackContext = AttackContext.Scaled(_attacker, 1f);
            _hitbox.EnableHitbox(attackContext);
            Assert.IsTrue(_hitboxCollider.enabled);

            _hitbox.DisableHitbox();

            Assert.IsFalse(_hitboxCollider.enabled);
        }

        #endregion

        #region OnTriggerEnter Tests

        [Test]
        public void OnTriggerEnter_NullAttacker_NoDamage()
        {
            var targetObject = CreateEnemyTarget();
            var target = targetObject.GetComponent<Combatant>();
            float initialHealth = target.CurrentHealth;

            // Enable hitbox without proper attacker context
            var attackContext = new AttackContext(); // Default with null attacker
            _hitbox.EnableHitbox(attackContext);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.AreEqual(initialHealth, target.CurrentHealth);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_HitsEnemy_DealsDamage()
        {
            var targetObject = CreateEnemyTarget();
            var target = targetObject.GetComponent<Combatant>();
            float initialHealth = target.CurrentHealth;

            var attackContext = AttackContext.Fixed(_attacker, 30f);
            _hitbox.EnableHitbox(attackContext);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.Less(target.CurrentHealth, initialHealth);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_HitsAlly_NoDamage()
        {
            var targetObject = CreateAllyTarget();
            var target = targetObject.GetComponent<Combatant>();
            float initialHealth = target.CurrentHealth;

            var attackContext = AttackContext.Fixed(_attacker, 30f);
            _hitbox.EnableHitbox(attackContext);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.AreEqual(initialHealth, target.CurrentHealth);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_SameTarget_HitsOncePerAttack()
        {
            var targetObject = CreateEnemyTarget();
            var target = targetObject.GetComponent<Combatant>();
            float initialHealth = target.CurrentHealth;

            var attackContext = AttackContext.Fixed(_attacker, 10f);
            _hitbox.EnableHitbox(attackContext);

            // Hit the same target multiple times
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());
            float healthAfterFirstHit = target.CurrentHealth;

            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());
            float healthAfterSecondHit = target.CurrentHealth;

            // Only first hit should deal damage
            Assert.Less(healthAfterFirstHit, initialHealth);
            Assert.AreEqual(healthAfterFirstHit, healthAfterSecondHit);

            Object.DestroyImmediate(targetObject);
        }

        [UnityTest]
        public IEnumerator OnTriggerEnter_CannotTakeDamage_NoDamage()
        {
            var targetObject = CreateEnemyTarget();
            var target = targetObject.GetComponent<Combatant>();
            float initialHealth = target.CurrentHealth;

            // Make target invincible
            target.SetInvincible(1f);
            yield return null; // Let Update run

            var attackContext = AttackContext.Fixed(_attacker, 30f);
            _hitbox.EnableHitbox(attackContext);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.AreEqual(initialHealth, target.CurrentHealth);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_SingleTargetMode_DisablesAfterFirstHit()
        {
            SetHitboxMultipleTargets(_hitbox, false);

            var targetObject = CreateEnemyTarget();

            var attackContext = AttackContext.Fixed(_attacker, 10f);
            _hitbox.EnableHitbox(attackContext);
            Assert.IsTrue(_hitboxCollider.enabled);

            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            // Hitbox should be disabled after first hit
            Assert.IsFalse(_hitboxCollider.enabled);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_NonIDamageable_Ignored()
        {
            // Create object without IDamageable
            var nonDamageableObject = new GameObject("NonDamageable");
            var collider = nonDamageableObject.AddComponent<BoxCollider>();

            var attackContext = AttackContext.Fixed(_attacker, 30f);
            _hitbox.EnableHitbox(attackContext);

            Assert.DoesNotThrow(() => SimulateTriggerEnter(_hitbox, collider));

            Object.DestroyImmediate(nonDamageableObject);
        }

        #endregion

        #region Helper Methods

        private GameObject CreateEnemyTarget()
        {
            var targetObject = new GameObject("EnemyTarget");
            targetObject.AddComponent<Health>();
            var combatant = targetObject.AddComponent<Combatant>();
            var collider = targetObject.AddComponent<BoxCollider>();
            SetCombatantTeam(combatant, CombatTeam.Enemy);
            return targetObject;
        }

        private GameObject CreateAllyTarget()
        {
            var targetObject = new GameObject("AllyTarget");
            targetObject.AddComponent<Health>();
            var combatant = targetObject.AddComponent<Combatant>();
            var collider = targetObject.AddComponent<BoxCollider>();
            SetCombatantTeam(combatant, CombatTeam.Player); // Same team as attacker
            return targetObject;
        }

        private void SimulateTriggerEnter(HitboxTrigger hitbox, Collider other)
        {
            // Use reflection to call OnTriggerEnter since it's private
            var method = typeof(HitboxTrigger).GetMethod("OnTriggerEnter",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(hitbox, new object[] { other });
        }

        private void SetCombatantTeam(Combatant combatant, CombatTeam team)
        {
            var serializedObject = new UnityEditor.SerializedObject(combatant);
            serializedObject.FindProperty("_team").enumValueIndex = (int)team;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void SetHitboxMultipleTargets(HitboxTrigger hitbox, bool hitMultiple)
        {
            var serializedObject = new UnityEditor.SerializedObject(hitbox);
            serializedObject.FindProperty("_hitMultipleTargets").boolValue = hitMultiple;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion
    }
}
