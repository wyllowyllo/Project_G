using Combat.Core;
using Equipment;
using NUnit.Framework;
using Tests.Shared;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class EquipmentDropOnDeathTests
    {
        private GameObject _monsterObject;
        private Health _monsterHealth;
        private EquipmentDropOnDeath _equipmentDrop;
        private DropTableData _dropTable;
        private EquipmentData _testWeapon;
        private DroppedEquipment _droppedEquipmentPrefab;

        [SetUp]
        public void SetUp()
        {
            _testWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 10f);

            _dropTable = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(_dropTable, normalPool: new[] { _testWeapon });

            _droppedEquipmentPrefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab();

            _monsterObject = EquipmentTestUtilities.CreateMonsterWithEquipmentDrop(
                _dropTable, _droppedEquipmentPrefab, out _monsterHealth, out _equipmentDrop);
        }

        [TearDown]
        public void TearDown()
        {
            if (_monsterObject != null)
                Object.DestroyImmediate(_monsterObject);
            if (_dropTable != null)
                Object.DestroyImmediate(_dropTable);
            if (_testWeapon != null)
                Object.DestroyImmediate(_testWeapon);
            if (_droppedEquipmentPrefab != null)
                Object.DestroyImmediate(_droppedEquipmentPrefab.gameObject);

            var droppedItems = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None);
            foreach (var item in droppedItems)
                Object.DestroyImmediate(item.gameObject);
        }

        #region Drop Tests

        [Test]
        public void MonsterDeath_WithPrefab_SpawnsDroppedEquipment()
        {
            var countBefore = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None).Length;

            _monsterHealth.TakeDamage(100f);

            var allDropped = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None);
            Assert.AreEqual(countBefore + 1, allDropped.Length);

            var spawned = System.Array.Find(allDropped, d => d.EquipmentData != null);
            Assert.IsNotNull(spawned);
            Assert.AreEqual(_testWeapon, spawned.EquipmentData);
        }

        [Test]
        public void MonsterDeath_WithoutPrefab_DoesNotThrow()
        {
            var countBefore = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None).Length;

            var monsterNoPrefab = EquipmentTestUtilities.CreateMonsterWithEquipmentDrop(
                _dropTable, null, out var health, out _);

            Assert.DoesNotThrow(() => health.TakeDamage(100f));

            var countAfter = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(countBefore, countAfter);

            Object.DestroyImmediate(monsterNoPrefab);
        }

        [Test]
        public void MonsterDeath_WithoutDropTable_DoesNotThrow()
        {
            var monsterNoTable = new GameObject("MonsterNoTable");
            var health = monsterNoTable.AddComponent<Health>();
            monsterNoTable.AddComponent<EquipmentDropOnDeath>();

            Assert.DoesNotThrow(() => health.TakeDamage(100f));

            Object.DestroyImmediate(monsterNoTable);
        }

        [Test]
        public void MonsterDeath_ZeroDropChance_NoDroppedEquipment()
        {
            var countBefore = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None).Length;

            var zeroChanceTable = EquipmentTestUtilities.CreateDropTableData(dropChance: 0f);
            EquipmentTestUtilities.SetDropTablePools(zeroChanceTable, normalPool: new[] { _testWeapon });

            var monster = EquipmentTestUtilities.CreateMonsterWithEquipmentDrop(
                zeroChanceTable, _droppedEquipmentPrefab, out var health, out _);

            health.TakeDamage(100f);

            var countAfter = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(countBefore, countAfter);

            Object.DestroyImmediate(monster);
            Object.DestroyImmediate(zeroChanceTable);
        }

        [Test]
        public void MonsterDeath_DroppedEquipmentPosition_MatchesMonsterPosition()
        {
            var expectedPosition = new Vector3(5f, 2f, 3f);
            _monsterObject.transform.position = expectedPosition;

            _monsterHealth.TakeDamage(100f);

            var allDropped = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None);
            var spawned = System.Array.Find(allDropped, d => d.EquipmentData != null);
            Assert.IsNotNull(spawned);
            Assert.AreEqual(expectedPosition, spawned.transform.position);
        }

        #endregion
    }
}
