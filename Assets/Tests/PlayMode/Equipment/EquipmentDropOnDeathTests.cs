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
        private EquipmentData _testWeaponData;
        private GameObject _testWeaponPrefab;

        [SetUp]
        public void SetUp()
        {
            _testWeaponData = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 10f);

            _testWeaponPrefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab(_testWeaponData);

            _dropTable = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(_dropTable, normalPool: new[] { _testWeaponPrefab });

            _monsterObject = EquipmentTestUtilities.CreateMonsterWithEquipmentDrop(
                _dropTable, out _monsterHealth, out _equipmentDrop);
        }

        [TearDown]
        public void TearDown()
        {
            if (_monsterObject != null)
                Object.DestroyImmediate(_monsterObject);
            if (_dropTable != null)
                Object.DestroyImmediate(_dropTable);
            if (_testWeaponData != null)
                Object.DestroyImmediate(_testWeaponData);
            if (_testWeaponPrefab != null)
                Object.DestroyImmediate(_testWeaponPrefab);

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
            Assert.AreEqual(_testWeaponData, spawned.EquipmentData);
        }

        [Test]
        public void MonsterDeath_WithEmptyDropTable_DoesNotThrow()
        {
            var countBefore = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None).Length;

            var emptyTable = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            var monsterEmptyTable = EquipmentTestUtilities.CreateMonsterWithEquipmentDrop(
                emptyTable, out var health, out _);

            Assert.DoesNotThrow(() => health.TakeDamage(100f));

            var countAfter = Object.FindObjectsByType<DroppedEquipment>(FindObjectsSortMode.None).Length;
            Assert.AreEqual(countBefore, countAfter);

            Object.DestroyImmediate(monsterEmptyTable);
            Object.DestroyImmediate(emptyTable);
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
            EquipmentTestUtilities.SetDropTablePools(zeroChanceTable, normalPool: new[] { _testWeaponPrefab });

            var monster = EquipmentTestUtilities.CreateMonsterWithEquipmentDrop(
                zeroChanceTable, out var health, out _);

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

        #region Injection Tests

        [Test]
        public void SetDropTable_WhenEmpty_SetsTable()
        {
            var monster = new GameObject("Monster");
            monster.AddComponent<Health>();
            var dropper = monster.AddComponent<EquipmentDropOnDeath>();

            dropper.SetDropTable(_dropTable);

            monster.GetComponent<Health>().TakeDamage(100f);

            var dropped = Object.FindFirstObjectByType<DroppedEquipment>();
            Assert.IsNotNull(dropped);

            Object.DestroyImmediate(monster);
        }

        [Test]
        public void SetDropTable_WhenAlreadySet_IgnoresNewTable()
        {
            var otherWeaponData = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Helmet, EquipmentGrade.Rare, defenseBonus: 20f);
            var otherWeaponPrefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab(otherWeaponData);
            var otherTable = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(otherTable, rarePool: new[] { otherWeaponPrefab });

            _equipmentDrop.SetDropTable(otherTable);

            _monsterHealth.TakeDamage(100f);

            var dropped = Object.FindFirstObjectByType<DroppedEquipment>();
            Assert.IsNotNull(dropped);
            Assert.AreEqual(_testWeaponData, dropped.EquipmentData);

            Object.DestroyImmediate(otherTable);
            Object.DestroyImmediate(otherWeaponData);
            Object.DestroyImmediate(otherWeaponPrefab);
        }

        #endregion
    }
}
