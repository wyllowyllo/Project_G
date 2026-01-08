using System.Collections;
using System.Reflection;
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
            _attacker.SetTeamForTest(CombatTeam.Player);

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

            _hitbox.EnableHitbox(CombatTeam.Player);

            Assert.IsTrue(_hitboxCollider.enabled);
        }

        [Test]
        public void EnableHitbox_ClearsHitTargets()
        {
            var targetObject = CreateEnemyTarget();

            int hitCount = 0;
            _hitbox.OnHit += _ => hitCount++;

            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.AreEqual(1, hitCount);

            _hitbox.DisableHitbox();
            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.AreEqual(2, hitCount);

            Object.DestroyImmediate(targetObject);
        }

        #endregion

        #region DisableHitbox Tests

        [Test]
        public void DisableHitbox_DisablesCollider()
        {
            _hitbox.EnableHitbox(CombatTeam.Player);
            Assert.IsTrue(_hitboxCollider.enabled);

            _hitbox.DisableHitbox();

            Assert.IsFalse(_hitboxCollider.enabled);
        }

        [Test]
        public void DisableHitbox_PreventsHitEvents()
        {
            var targetObject = CreateEnemyTarget();

            int hitCount = 0;
            _hitbox.OnHit += _ => hitCount++;

            _hitbox.EnableHitbox(CombatTeam.Player);
            _hitbox.DisableHitbox();
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.AreEqual(0, hitCount);

            Object.DestroyImmediate(targetObject);
        }

        #endregion

        #region OnTriggerEnter Tests

        [Test]
        public void OnTriggerEnter_WhenNotActive_NoEvent()
        {
            var targetObject = CreateEnemyTarget();

            bool eventFired = false;
            _hitbox.OnHit += _ => eventFired = true;

            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.IsFalse(eventFired);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_HitsEnemy_FiresOnHitEvent()
        {
            var targetObject = CreateEnemyTarget();
            var targetCombatant = targetObject.GetComponent<Combatant>();

            HitInfo? receivedHitInfo = null;
            _hitbox.OnHit += hitInfo => receivedHitInfo = hitInfo;

            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.IsNotNull(receivedHitInfo);
            Assert.AreEqual(targetCombatant, receivedHitInfo.Value.Target);
            Assert.AreEqual(targetCombatant, receivedHitInfo.Value.TargetCombatant);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_HitsAlly_NoEvent()
        {
            var targetObject = CreateAllyTarget();

            bool eventFired = false;
            _hitbox.OnHit += _ => eventFired = true;

            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.IsFalse(eventFired);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_SameTarget_FiresOncePerAttack()
        {
            var targetObject = CreateEnemyTarget();

            int hitCount = 0;
            _hitbox.OnHit += _ => hitCount++;

            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.AreEqual(1, hitCount);

            Object.DestroyImmediate(targetObject);
        }

        [UnityTest]
        public IEnumerator OnTriggerEnter_CannotTakeDamage_NoEvent()
        {
            var targetObject = CreateEnemyTarget();
            var target = targetObject.GetComponent<Combatant>();

            target.SetInvincible(1f);
            yield return null;

            bool eventFired = false;
            _hitbox.OnHit += _ => eventFired = true;

            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.IsFalse(eventFired);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_SingleTargetMode_DisablesAfterFirstHit()
        {
            _hitbox.SetHitMultipleTargetsForTest(false);

            var targetObject = CreateEnemyTarget();

            _hitbox.EnableHitbox(CombatTeam.Player);
            Assert.IsTrue(_hitboxCollider.enabled);

            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.IsFalse(_hitboxCollider.enabled);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_NonIDamageable_Ignored()
        {
            var nonDamageableObject = new GameObject("NonDamageable");
            var collider = nonDamageableObject.AddComponent<BoxCollider>();

            bool eventFired = false;
            _hitbox.OnHit += _ => eventFired = true;

            _hitbox.EnableHitbox(CombatTeam.Player);

            Assert.DoesNotThrow(() => SimulateTriggerEnter(_hitbox, collider));
            Assert.IsFalse(eventFired);

            Object.DestroyImmediate(nonDamageableObject);
        }

        [Test]
        public void OnTriggerEnter_IncludesTargetHealthInHitInfo()
        {
            var targetObject = CreateEnemyTarget();
            var targetHealth = targetObject.GetComponent<Health>();

            HitInfo? receivedHitInfo = null;
            _hitbox.OnHit += hitInfo => receivedHitInfo = hitInfo;

            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetObject.GetComponent<Collider>());

            Assert.IsNotNull(receivedHitInfo);
            Assert.AreEqual(targetHealth, receivedHitInfo.Value.TargetHealth);

            Object.DestroyImmediate(targetObject);
        }

        [Test]
        public void OnTriggerEnter_IncludesColliderInHitInfo()
        {
            var targetObject = CreateEnemyTarget();
            var targetCollider = targetObject.GetComponent<Collider>();

            HitInfo? receivedHitInfo = null;
            _hitbox.OnHit += hitInfo => receivedHitInfo = hitInfo;

            _hitbox.EnableHitbox(CombatTeam.Player);
            SimulateTriggerEnter(_hitbox, targetCollider);

            Assert.IsNotNull(receivedHitInfo);
            Assert.AreEqual(targetCollider, receivedHitInfo.Value.TargetCollider);

            Object.DestroyImmediate(targetObject);
        }

        #endregion

        #region Helper Methods

        private GameObject CreateEnemyTarget()
        {
            var targetObject = new GameObject("EnemyTarget");
            targetObject.AddComponent<Health>();
            var combatant = targetObject.AddComponent<Combatant>();
            var collider = targetObject.AddComponent<BoxCollider>();
            combatant.SetTeamForTest(CombatTeam.Enemy);
            return targetObject;
        }

        private GameObject CreateAllyTarget()
        {
            var targetObject = new GameObject("AllyTarget");
            targetObject.AddComponent<Health>();
            var combatant = targetObject.AddComponent<Combatant>();
            var collider = targetObject.AddComponent<BoxCollider>();
            combatant.SetTeamForTest(CombatTeam.Player);
            return targetObject;
        }

        private void SimulateTriggerEnter(HitboxTrigger hitbox, Collider other)
        {
            var method = typeof(HitboxTrigger).GetMethod("OnTriggerEnter",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(hitbox, new object[] { other });
        }

        #endregion
    }
}
