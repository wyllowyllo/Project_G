using Equipment;
using NUnit.Framework;
using Tests.Shared;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class DroppedEquipmentTests
    {
        private EquipmentData _testWeapon;
        private DroppedEquipment _droppedEquipment;

        [SetUp]
        public void SetUp()
        {
            _testWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 10f);

            var droppedObj = new GameObject("DroppedEquipment");
            droppedObj.AddComponent<BoxCollider>();
            _droppedEquipment = droppedObj.AddComponent<DroppedEquipment>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testWeapon != null)
                Object.DestroyImmediate(_testWeapon);
            if (_droppedEquipment != null)
                Object.DestroyImmediate(_droppedEquipment.gameObject);
        }

        [Test]
        public void EquipmentData_AfterInitialize_ReturnsCorrectData()
        {
            _droppedEquipment.Initialize(_testWeapon);

            Assert.AreEqual(_testWeapon, _droppedEquipment.EquipmentData);
        }

        [Test]
        public void EquipmentData_BeforeInitialize_ReturnsNull()
        {
            Assert.IsNull(_droppedEquipment.EquipmentData);
        }
    }
}
