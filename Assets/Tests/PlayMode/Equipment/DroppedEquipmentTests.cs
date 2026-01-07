using System.Collections;
using Combat.Core;
using Combat.Data;
using Equipment;
using NUnit.Framework;
using Tests.Shared;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    [TestFixture]
    public class DroppedEquipmentTests
    {
        private GameObject _playerObject;
        private PlayerEquipment _playerEquipment;
        private CombatStatsData _statsData;
        private EquipmentData _testWeapon;
        private DroppedEquipment _droppedEquipment;

        [SetUp]
        public void SetUp()
        {
            _statsData = ProgressionTestUtilities.CreateStatsData();
            _playerObject = EquipmentTestUtilities.CreatePlayerWithEquipment(
                _statsData, out _, out _playerEquipment);

            _testWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 10f);

            var droppedObj = new GameObject("DroppedEquipment");
            _droppedEquipment = droppedObj.AddComponent<DroppedEquipment>();
            _droppedEquipment.Initialize(_testWeapon);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_statsData != null)
                Object.DestroyImmediate(_statsData);
            if (_testWeapon != null)
                Object.DestroyImmediate(_testWeapon);
            if (_droppedEquipment != null)
                Object.DestroyImmediate(_droppedEquipment.gameObject);
        }

        [UnityTest]
        public IEnumerator TryPickup_ValidPlayer_EquipsAndDestroysDropped()
        {
            var result = _droppedEquipment.TryPickup(_playerEquipment);

            Assert.IsTrue(result);
            Assert.AreEqual(_testWeapon, _playerEquipment.GetEquipment(EquipmentSlot.Weapon));

            yield return null;

            Assert.IsTrue(_droppedEquipment == null);
        }

        [Test]
        public void TryPickup_NullPlayer_ReturnsFalse()
        {
            var result = _droppedEquipment.TryPickup(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryPickup_LowerGradeThanEquipped_ReturnsFalse()
        {
            var rareWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare);
            _playerEquipment.TryEquip(rareWeapon);

            var result = _droppedEquipment.TryPickup(_playerEquipment);

            Assert.IsFalse(result);
            Assert.AreEqual(rareWeapon, _playerEquipment.GetEquipment(EquipmentSlot.Weapon));

            Object.DestroyImmediate(rareWeapon);
        }

        [Test]
        public void EquipmentData_AfterInitialize_ReturnsCorrectData()
        {
            Assert.AreEqual(_testWeapon, _droppedEquipment.EquipmentData);
        }
    }
}
