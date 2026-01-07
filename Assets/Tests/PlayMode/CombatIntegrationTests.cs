using System.Collections;
using Combat.Attack;
using Combat.Core;
using Combat.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class CombatIntegrationTests
    {
        private GameObject _attackerObject;
        private Combatant _attacker;
        private MeleeAttacker _meleeAttacker;
        private HitboxTrigger _hitbox;

        private GameObject _defenderObject;
        private Combatant _defender;

        [SetUp]
        public void SetUp()
        {
            // Create attacker with full combat setup
            _attackerObject = new GameObject("Attacker");
            _attackerObject.AddComponent<Health>();
            _attacker = _attackerObject.AddComponent<Combatant>();
            SetCombatantTeam(_attacker, CombatTeam.Player);
            SetCombatantStats(_attacker, attackDamage: 100f, critChance: 0f, critMult: 2f, defense: 0f);

            // Create hitbox child object
            var hitboxObject = new GameObject("Hitbox");
            hitboxObject.transform.SetParent(_attackerObject.transform);
            var collider = hitboxObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            _hitbox = hitboxObject.AddComponent<HitboxTrigger>();

            _meleeAttacker = _attackerObject.AddComponent<MeleeAttacker>();
            SetMeleeAttackerHitbox(_meleeAttacker, _hitbox);

            // Re-trigger OnEnable to subscribe to hitbox events (since _hitbox was null during initial OnEnable)
            _meleeAttacker.enabled = false;
            _meleeAttacker.enabled = true;

            // Create defender
            _defenderObject = new GameObject("Defender");
            _defenderObject.AddComponent<Health>();
            _defender = _defenderObject.AddComponent<Combatant>();
            var defenderCollider = _defenderObject.AddComponent<BoxCollider>();
            SetCombatantTeam(_defender, CombatTeam.Enemy);
            SetCombatantStats(_defender, attackDamage: 50f, critChance: 0f, critMult: 1.5f, defense: 0f);

            // Position defender in front of attacker
            _defenderObject.transform.position = _attackerObject.transform.position + Vector3.forward * 0.5f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_attackerObject != null)
            {
                Object.DestroyImmediate(_attackerObject);
            }
            if (_defenderObject != null)
            {
                Object.DestroyImmediate(_defenderObject);
            }
        }

        #region Integration Tests

        [Test]
        public void FullCombatFlow_AttackerHitsDefender_DamageApplied()
        {
            float initialHealth = _defender.CurrentHealth;

            // Perform attack and hit
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());

            Assert.Less(_defender.CurrentHealth, initialHealth);
        }

        [Test]
        public void ComboAttack_ThreeHits_AppliesIncreasingDamage()
        {
            // Set defender health high enough to survive multiple hits
            SetHealthMaxHealth(_defenderObject, 1000f);

            // Set up combo settings with multipliers
            var comboSettings = ComboSettings.CreateForTest(new float[] { 1.0f, 1.1f, 1.3f });
            SetMeleeAttackerComboSettings(_meleeAttacker, comboSettings);

            float[] damageDealt = new float[3];
            int hitCount = 0;

            _defender.OnDamaged += info =>
            {
                if (hitCount < 3)
                {
                    damageDealt[hitCount] = info.Amount;
                    hitCount++;
                }
            };

            // First attack
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());
            _meleeAttacker.OnAttackHitEnd();
            _meleeAttacker.OnAttackAnimationEnd();
            _defender.Heal(1000f); // Reset health

            // Second attack
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());
            _meleeAttacker.OnAttackHitEnd();
            _meleeAttacker.OnAttackAnimationEnd();
            _defender.Heal(1000f);

            // Third attack
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());

            // Verify increasing damage
            Assert.Greater(damageDealt[1], damageDealt[0], "Second hit should deal more damage than first");
            Assert.Greater(damageDealt[2], damageDealt[1], "Third hit should deal more damage than second");

            Object.DestroyImmediate(comboSettings);
        }

        [Test]
        public void CriticalHit_AppliesCriticalMultiplier()
        {
            // Set 100% crit chance for deterministic test
            SetCombatantStats(_attacker, attackDamage: 100f, critChance: 1f, critMult: 2f, defense: 0f);

            bool wasCritical = false;
            _defender.OnDamaged += info => wasCritical = info.IsCritical;

            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());

            Assert.IsTrue(wasCritical);
        }

        [Test]
        public void DefenderDies_WhenHealthDepleted()
        {
            // Set damage higher than defender's max health
            SetCombatantStats(_attacker, attackDamage: 1000f, critChance: 0f, critMult: 1f, defense: 0f);

            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());

            Assert.IsFalse(_defender.IsAlive);
            Assert.AreEqual(0f, _defender.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator InvincibilityFrame_PreventsConsecutiveDamage()
        {
            // Set defender health high enough to survive hits
            SetHealthMaxHealth(_defenderObject, 1000f);

            // Set up auto invincibility on hit
            var hitReactionSettings = HitReactionSettings.CreateForTest(
                invincibilityDuration: 0.5f,
                autoInvincibility: true);
            SetCombatantHitReactionSettings(_defender, hitReactionSettings);

            float initialHealth = _defender.CurrentHealth;

            // First hit
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());
            _meleeAttacker.OnAttackHitEnd();
            _meleeAttacker.OnAttackAnimationEnd();

            float healthAfterFirstHit = _defender.CurrentHealth;
            Assert.Less(healthAfterFirstHit, initialHealth, "First hit should deal damage");

            yield return null; // Let invincibility apply

            Assert.IsTrue(_defender.IsInvincible, "Defender should be invincible after hit");

            // Second attack during invincibility
            _meleeAttacker.TryAttack();
            _meleeAttacker.OnAttackHitStart();
            SimulateTriggerEnter(_hitbox, _defenderObject.GetComponent<Collider>());

            Assert.AreEqual(healthAfterFirstHit, _defender.CurrentHealth, "Second hit should not deal damage during invincibility");

            Object.DestroyImmediate(hitReactionSettings);
        }

        #endregion

        #region Helper Methods

        private void SimulateTriggerEnter(HitboxTrigger hitbox, Collider other)
        {
            var method = typeof(HitboxTrigger).GetMethod("OnTriggerEnter",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(hitbox, new object[] { other });
        }

        private void SetCombatantTeam(Combatant combatant, CombatTeam team)
        {
            combatant.SetTeamForTest(team);
        }

        private void SetCombatantStats(Combatant combatant, float attackDamage, float critChance, float critMult, float defense)
        {
            var statsData = CombatStatsData.CreateForTest(attackDamage, critChance, critMult, defense);
            combatant.SetStatsDataForTest(statsData);
        }

        private void SetHealthMaxHealth(GameObject obj, float maxHealth)
        {
            var health = obj.GetComponent<Health>();
            health.SetMaxHealthForTest(maxHealth);
        }

        private void SetMeleeAttackerHitbox(MeleeAttacker attacker, HitboxTrigger hitbox)
        {
            attacker.SetHitboxForTest(hitbox);
        }

        private void SetMeleeAttackerComboSettings(MeleeAttacker attacker, ComboSettings settings)
        {
            var comboHandler = attacker.GetComponent<ComboAttackHandler>();
            comboHandler.SetComboSettingsForTest(settings);
        }

        private void SetCombatantHitReactionSettings(Combatant combatant, HitReactionSettings settings)
        {
            combatant.SetHitReactionSettingsForTest(settings);
        }

        #endregion
    }
}
