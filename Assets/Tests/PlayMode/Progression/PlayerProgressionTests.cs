using Combat.Core;
using Combat.Data;
using NUnit.Framework;
using Progression;
using Tests.Shared;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class PlayerProgressionTests
    {
        private GameObject _playerObject;
        private PlayerProgression _progression;
        private Combatant _combatant;
        private ProgressionConfig _config;
        private CombatStatsData _statsData;

        [SetUp]
        public void SetUp()
        {
            _config = ProgressionTestUtilities.CreateProgressionConfig();
            _statsData = ProgressionTestUtilities.CreateStatsData();
            _playerObject = ProgressionTestUtilities.CreatePlayerWithProgression(
                _config, _statsData, out _combatant, out _progression);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_config != null)
                Object.DestroyImmediate(_config);
            if (_statsData != null)
                Object.DestroyImmediate(_statsData);
        }

        #region Initial State Tests

        [Test]
        public void InitialState_IsLevel1WithZeroXpAndRankC()
        {
            Assert.AreEqual(1, _progression.Level);
            Assert.AreEqual(0, _progression.CurrentXp);
            Assert.AreEqual(HunterRank.C, _progression.Rank);
            Assert.IsFalse(_progression.IsMaxLevel);
        }

        #endregion

        #region AddExperience Tests

        [Test]
        public void AddExperience_PositiveAmount_IncreasesXp()
        {
            _progression.AddExperience(50);
            Assert.AreEqual(50, _progression.CurrentXp);
        }

        [TestCase(0)]
        [TestCase(-100)]
        public void AddExperience_ZeroOrNegative_NoEffect(int amount)
        {
            _progression.AddExperience(amount);
            Assert.AreEqual(0, _progression.CurrentXp);
        }

        #endregion

        #region Level Up Tests

        [Test]
        public void AddExperience_EnoughForLevelUp_IncreasesLevel()
        {
            int xpForLevel2 = _config.GetRequiredXp(2);
            _progression.AddExperience(xpForLevel2);
            Assert.AreEqual(2, _progression.Level);
        }

        [Test]
        public void AddExperience_AtMaxLevel_NoEffect()
        {
            // Add XP in chunks until max level (respecting MAX_XP_PER_ADD limit)
            while (!_progression.IsMaxLevel)
                _progression.AddExperience(100000);

            Assert.AreEqual(30, _progression.Level);
            Assert.IsTrue(_progression.IsMaxLevel);

            int xpBefore = _progression.CurrentXp;
            _progression.AddExperience(1000);
            Assert.AreEqual(xpBefore, _progression.CurrentXp);
        }

        #endregion

        #region Event Tests

        [Test]
        public void LevelUp_FiresOnLevelUpEvent()
        {
            bool eventFired = false;
            int prevLevel = 0, newLevel = 0;
            _progression.OnLevelUp += (prev, next) =>
            {
                eventFired = true;
                prevLevel = prev;
                newLevel = next;
            };

            _progression.AddExperience(_config.GetRequiredXp(2));

            Assert.IsTrue(eventFired);
            Assert.AreEqual(1, prevLevel);
            Assert.AreEqual(2, newLevel);
        }

        [Test]
        public void LevelUp_ToSkillLevel_FiresOnSkillEnhancedEvent()
        {
            SkillSlot enhancedSkill = SkillSlot.None;
            _progression.OnSkillEnhanced += skill => enhancedSkill = skill;

            int xpToLevel11 = 0;
            for (int i = 2; i <= 11; i++)
                xpToLevel11 += _config.GetRequiredXp(i);

            _progression.AddExperience(xpToLevel11);

            Assert.AreEqual(SkillSlot.Q, enhancedSkill);
        }

        #endregion

        #region StatModifier Tests

        [Test]
        public void LevelUp_AppliesAttackModifier()
        {
            float initialAttack = _combatant.Stats.AttackDamage.Value;

            _progression.AddExperience(_config.GetRequiredXp(2));

            float expectedBonus = _config.GetAttackBonus(2);
            Assert.AreEqual(initialAttack + expectedBonus, _combatant.Stats.AttackDamage.Value);
        }

        #endregion
    }
}
