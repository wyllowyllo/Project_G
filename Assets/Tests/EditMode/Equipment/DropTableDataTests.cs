using Equipment;
using NUnit.Framework;
using Tests.Shared;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class DropTableDataTests
    {
        private DropTableData _dropTable;
        private EquipmentData _normalWeaponData;
        private EquipmentData _rareWeaponData;
        private EquipmentData _uniqueWeaponData;
        private EquipmentData _legendaryWeaponData;
        private GameObject _normalPrefab;
        private GameObject _rarePrefab;
        private GameObject _uniquePrefab;
        private GameObject _legendaryPrefab;

        [SetUp]
        public void SetUp()
        {
            _normalWeaponData = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, equipmentName: "NormalWeapon");
            _rareWeaponData = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare, equipmentName: "RareWeapon");
            _uniqueWeaponData = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Unique, equipmentName: "UniqueWeapon");
            _legendaryWeaponData = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Legendary, equipmentName: "LegendaryWeapon");

            _normalPrefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab(_normalWeaponData);
            _rarePrefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab(_rareWeaponData);
            _uniquePrefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab(_uniqueWeaponData);
            _legendaryPrefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab(_legendaryWeaponData);

            _dropTable = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(
                _dropTable,
                normalPool: new[] { _normalPrefab },
                rarePool: new[] { _rarePrefab },
                uniquePool: new[] { _uniquePrefab },
                legendaryPool: new[] { _legendaryPrefab });
        }

        [TearDown]
        public void TearDown()
        {
            if (_dropTable != null)
                Object.DestroyImmediate(_dropTable);
            if (_normalWeaponData != null)
                Object.DestroyImmediate(_normalWeaponData);
            if (_rareWeaponData != null)
                Object.DestroyImmediate(_rareWeaponData);
            if (_uniqueWeaponData != null)
                Object.DestroyImmediate(_uniqueWeaponData);
            if (_legendaryWeaponData != null)
                Object.DestroyImmediate(_legendaryWeaponData);
            if (_normalPrefab != null)
                Object.DestroyImmediate(_normalPrefab);
            if (_rarePrefab != null)
                Object.DestroyImmediate(_rarePrefab);
            if (_uniquePrefab != null)
                Object.DestroyImmediate(_uniquePrefab);
            if (_legendaryPrefab != null)
                Object.DestroyImmediate(_legendaryPrefab);
        }

        private EquipmentData GetEquipmentData(GameObject prefab)
        {
            return prefab?.GetComponent<DroppedEquipment>()?.EquipmentData;
        }

        #region RollDrop Tests

        [Test]
        public void RollDrop_ZeroDropChance_ReturnsNull()
        {
            var table = EquipmentTestUtilities.CreateDropTableData(dropChance: 0f);
            EquipmentTestUtilities.SetDropTablePools(table, normalPool: new[] { _normalPrefab });

            var result = table.RollDrop();

            Assert.IsNull(result);

            Object.DestroyImmediate(table);
        }

        [Test]
        public void RollDrop_FullDropChance_ReturnsEquipment()
        {
            var result = _dropTable.RollDrop();

            Assert.IsNotNull(result);
        }

        [Test]
        public void RollDrop_OnlyNormalWeight_ReturnsNormal()
        {
            var table = EquipmentTestUtilities.CreateDropTableData(
                dropChance: 1f,
                normalWeight: 100,
                rareWeight: 0,
                uniqueWeight: 0,
                legendaryWeight: 0);
            EquipmentTestUtilities.SetDropTablePools(
                table,
                normalPool: new[] { _normalPrefab },
                rarePool: new[] { _rarePrefab },
                uniquePool: new[] { _uniquePrefab },
                legendaryPool: new[] { _legendaryPrefab });

            for (int i = 0; i < 10; i++)
            {
                var result = table.RollDrop();
                Assert.AreEqual(EquipmentGrade.Normal, GetEquipmentData(result).Grade);
            }

            Object.DestroyImmediate(table);
        }

        [Test]
        public void RollDrop_OnlyLegendaryWeight_ReturnsLegendary()
        {
            var table = EquipmentTestUtilities.CreateDropTableData(
                dropChance: 1f,
                normalWeight: 0,
                rareWeight: 0,
                uniqueWeight: 0,
                legendaryWeight: 100);
            EquipmentTestUtilities.SetDropTablePools(
                table,
                normalPool: new[] { _normalPrefab },
                rarePool: new[] { _rarePrefab },
                uniquePool: new[] { _uniquePrefab },
                legendaryPool: new[] { _legendaryPrefab });

            for (int i = 0; i < 10; i++)
            {
                var result = table.RollDrop();
                Assert.AreEqual(EquipmentGrade.Legendary, GetEquipmentData(result).Grade);
            }

            Object.DestroyImmediate(table);
        }

        [Test]
        public void RollDrop_AllPoolsEmpty_ReturnsNull()
        {
            var table = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            // All pools are empty by default

            var result = table.RollDrop();

            Assert.IsNull(result);

            Object.DestroyImmediate(table);
        }

        [Test]
        public void RollDrop_SomePoolsEmpty_RedistributesToNonEmptyPools()
        {
            // Only normal and legendary pools have items
            // rare and unique are empty, so their weights should be ignored
            var table = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(
                table,
                normalPool: new[] { _normalPrefab },
                legendaryPool: new[] { _legendaryPrefab });

            int normalCount = 0;
            int legendaryCount = 0;
            const int iterations = 100;

            for (int i = 0; i < iterations; i++)
            {
                var result = table.RollDrop();
                Assert.IsNotNull(result, "Should never return null when pools have items");

                var data = GetEquipmentData(result);
                if (data.Grade == EquipmentGrade.Normal) normalCount++;
                else if (data.Grade == EquipmentGrade.Legendary) legendaryCount++;
            }

            Assert.AreEqual(iterations, normalCount + legendaryCount,
                "All results should be from normal or legendary pools");
            Assert.Greater(normalCount, 0, "Normal pool should be selected sometimes");
            Assert.Greater(legendaryCount, 0, "Legendary pool should be selected sometimes");
        }

        [Test]
        public void RollDrop_MultipleItemsInPool_ReturnsFromPool()
        {
            var weapon2Data = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, equipmentName: "NormalWeapon2");
            var weapon2Prefab = EquipmentTestUtilities.CreateDroppedEquipmentPrefab(weapon2Data);

            var table = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(table, normalPool: new[] { _normalPrefab, weapon2Prefab });

            bool foundFirst = false;
            bool foundSecond = false;

            for (int i = 0; i < 100 && (!foundFirst || !foundSecond); i++)
            {
                var result = table.RollDrop();
                if (result == _normalPrefab) foundFirst = true;
                if (result == weapon2Prefab) foundSecond = true;
            }

            Assert.IsTrue(foundFirst && foundSecond, "Both items should be possible results");

            Object.DestroyImmediate(weapon2Data);
            Object.DestroyImmediate(weapon2Prefab);
            Object.DestroyImmediate(table);
        }

        #endregion
    }
}
