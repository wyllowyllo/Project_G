using Combat.Core;
using Combat.Data;
using NUnit.Framework;
using Progression;
using Tests.Shared;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class XpDropOnDeathTests
    {
        private GameObject _monsterObject;
        private GameObject _playerObject;
        private XpDropOnDeath _xpDrop;
        private Health _monsterHealth;
        private PlayerProgression _playerProgression;
        private ProgressionConfig _config;
        private CombatStatsData _statsData;

        [SetUp]
        public void SetUp()
        {
            _config = ProgressionTestUtilities.CreateProgressionConfig();
            _statsData = ProgressionTestUtilities.CreateStatsData();
            _playerObject = ProgressionTestUtilities.CreatePlayerWithProgression(
                _config, _statsData, out _playerProgression);
            _monsterObject = ProgressionTestUtilities.CreateMonsterWithXpDrop(
                50, out _monsterHealth, out _xpDrop);
        }

        [TearDown]
        public void TearDown()
        {
            if (_monsterObject != null)
                Object.DestroyImmediate(_monsterObject);
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_config != null)
                Object.DestroyImmediate(_config);
            if (_statsData != null)
                Object.DestroyImmediate(_statsData);
        }

        #region XP Drop Tests

        [Test]
        public void MonsterDeath_WithPlayer_GrantsXp()
        {
            _xpDrop.SetPlayerForTest(_playerProgression);

            _monsterHealth.TakeDamage(100f);

            Assert.AreEqual(50, _playerProgression.CurrentXp);
        }

        [Test]
        public void MonsterDeath_WithoutPlayer_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _monsterHealth.TakeDamage(100f));
            Assert.AreEqual(0, _playerProgression.CurrentXp);
        }

        [Test]
        public void MultipleMonsterDeaths_AccumulatesXp()
        {
            _xpDrop.SetPlayerForTest(_playerProgression);

            var monster2 = ProgressionTestUtilities.CreateMonsterWithXpDrop(
                75, out var health2, out var xpDrop2);
            xpDrop2.SetPlayerForTest(_playerProgression);

            _monsterHealth.TakeDamage(100f);
            health2.TakeDamage(100f);

            Assert.AreEqual(125, _playerProgression.CurrentXp);

            Object.DestroyImmediate(monster2);
        }

        #endregion
    }
}
