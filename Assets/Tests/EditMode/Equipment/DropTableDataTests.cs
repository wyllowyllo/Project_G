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
        private EquipmentData _normalWeapon;
        private EquipmentData _rareWeapon;
        private EquipmentData _uniqueWeapon;
        private EquipmentData _legendaryWeapon;

        [SetUp]
        public void SetUp()
        {
            _normalWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, equipmentName: "NormalWeapon");
            _rareWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare, equipmentName: "RareWeapon");
            _uniqueWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Unique, equipmentName: "UniqueWeapon");
            _legendaryWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Legendary, equipmentName: "LegendaryWeapon");

            _dropTable = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(
                _dropTable,
                normalPool: new[] { _normalWeapon },
                rarePool: new[] { _rareWeapon },
                uniquePool: new[] { _uniqueWeapon },
                legendaryPool: new[] { _legendaryWeapon });
        }

        [TearDown]
        public void TearDown()
        {
            if (_dropTable != null)
                Object.DestroyImmediate(_dropTable);
            if (_normalWeapon != null)
                Object.DestroyImmediate(_normalWeapon);
            if (_rareWeapon != null)
                Object.DestroyImmediate(_rareWeapon);
            if (_uniqueWeapon != null)
                Object.DestroyImmediate(_uniqueWeapon);
            if (_legendaryWeapon != null)
                Object.DestroyImmediate(_legendaryWeapon);
        }

        #region RollDrop Tests

        [Test]
        public void RollDrop_ZeroDropChance_ReturnsNull()
        {
            var table = EquipmentTestUtilities.CreateDropTableData(dropChance: 0f);
            EquipmentTestUtilities.SetDropTablePools(table, normalPool: new[] { _normalWeapon });

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
                normalPool: new[] { _normalWeapon },
                rarePool: new[] { _rareWeapon },
                uniquePool: new[] { _uniqueWeapon },
                legendaryPool: new[] { _legendaryWeapon });

            for (int i = 0; i < 10; i++)
            {
                var result = table.RollDrop();
                Assert.AreEqual(EquipmentGrade.Normal, result.Grade);
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
                normalPool: new[] { _normalWeapon },
                rarePool: new[] { _rareWeapon },
                uniquePool: new[] { _uniqueWeapon },
                legendaryPool: new[] { _legendaryWeapon });

            for (int i = 0; i < 10; i++)
            {
                var result = table.RollDrop();
                Assert.AreEqual(EquipmentGrade.Legendary, result.Grade);
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
                normalPool: new[] { _normalWeapon },
                legendaryPool: new[] { _legendaryWeapon });

            int normalCount = 0;
            int legendaryCount = 0;
            const int iterations = 100;

            for (int i = 0; i < iterations; i++)
            {
                var result = table.RollDrop();
                Assert.IsNotNull(result, "Should never return null when pools have items");

                if (result.Grade == EquipmentGrade.Normal) normalCount++;
                else if (result.Grade == EquipmentGrade.Legendary) legendaryCount++;
            }

            Assert.AreEqual(iterations, normalCount + legendaryCount,
                "All results should be from normal or legendary pools");
            Assert.Greater(normalCount, 0, "Normal pool should be selected sometimes");
            Assert.Greater(legendaryCount, 0, "Legendary pool should be selected sometimes");
        }

        [Test]
        public void RollDrop_MultipleItemsInPool_ReturnsFromPool()
        {
            var weapon2 = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, equipmentName: "NormalWeapon2");

            var table = EquipmentTestUtilities.CreateDropTableData(dropChance: 1f);
            EquipmentTestUtilities.SetDropTablePools(table, normalPool: new[] { _normalWeapon, weapon2 });

            bool foundFirst = false;
            bool foundSecond = false;

            for (int i = 0; i < 100 && (!foundFirst || !foundSecond); i++)
            {
                var result = table.RollDrop();
                if (result == _normalWeapon) foundFirst = true;
                if (result == weapon2) foundSecond = true;
            }

            Assert.IsTrue(foundFirst && foundSecond, "Both items should be possible results");

            Object.DestroyImmediate(weapon2);
            Object.DestroyImmediate(table);
        }

        #endregion
    }
}
