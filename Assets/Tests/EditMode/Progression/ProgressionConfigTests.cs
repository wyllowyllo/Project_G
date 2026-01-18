using NUnit.Framework;
using Progression;
using Skill;
using Tests.Shared;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class ProgressionConfigTests
    {
        private ProgressionConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ProgressionTestUtilities.CreateProgressionConfig();
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
                Object.DestroyImmediate(_config);
        }

        #region XP Curve Tests

        [TestCase(2, 282)]
        [TestCase(30, 16431)]
        public void GetRequiredXp_ValidLevel_ReturnsCorrectValue(int level, int expected)
        {
            Assert.AreEqual(expected, _config.GetRequiredXp(level));
        }

        [TestCase(-1)]
        [TestCase(31)]
        public void GetRequiredXp_InvalidLevel_ReturnsZero(int level)
        {
            Assert.AreEqual(0, _config.GetRequiredXp(level));
        }

        #endregion

        #region Attack Bonus Tests

        [TestCase(1, 0f)]
        [TestCase(10, 45f)]
        [TestCase(30, 145f)]
        public void GetAttackBonus_ReturnsCorrectValue(int level, float expected)
        {
            Assert.AreEqual(expected, _config.GetAttackBonus(level));
        }

        #endregion

        #region Skill Enhancement Tests

        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        public void GetSkillEnhancements_EnhancementLevel_ReturnsAllSkills(int level)
        {
            var skills = _config.GetSkillEnhancements(level);
            Assert.AreEqual(3, skills.Length);
            Assert.Contains(SkillSlot.Q, skills);
            Assert.Contains(SkillSlot.E, skills);
            Assert.Contains(SkillSlot.R, skills);
        }

        [Test]
        public void GetSkillEnhancements_NonEnhancementLevel_ReturnsEmpty()
        {
            Assert.IsEmpty(_config.GetSkillEnhancements(1));
        }

        #endregion

        #region Rank Tests

        [TestCase(9, HunterRank.C)]
        [TestCase(10, HunterRank.B)]
        [TestCase(19, HunterRank.B)]
        [TestCase(20, HunterRank.A)]
        [TestCase(29, HunterRank.A)]
        [TestCase(30, HunterRank.S)]
        public void GetRank_ReturnsCorrectRank(int level, HunterRank expected)
        {
            Assert.AreEqual(expected, ProgressionConfig.GetRank(level));
        }

        #endregion
    }
}
