using Combat.Core;
using Combat.Data;
using Equipment;
using NUnit.Framework;
using Tests.Shared;
using UnityEngine;

namespace Tests.PlayMode
{
    [TestFixture]
    public class PlayerEquipmentTests
    {
        private GameObject _playerObject;
        private GameObject _managerObject;
        private Combatant _combatant;
        private PlayerEquipment _playerEquipment;
        private EquipmentDataManager _dataManager;
        private CombatStatsData _statsData;

        [SetUp]
        public void SetUp()
        {
            _managerObject = new GameObject("EquipmentDataManager");
            _dataManager = _managerObject.AddComponent<EquipmentDataManager>();

            _statsData = ProgressionTestUtilities.CreateStatsData(
                baseAttackDamage: 10f,
                criticalChance: 0.1f,
                criticalMultiplier: 1.5f,
                defense: 5f);
            _playerObject = EquipmentTestUtilities.CreatePlayerWithEquipment(
                _statsData, out _combatant, out _playerEquipment);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);
            if (_managerObject != null)
                Object.DestroyImmediate(_managerObject);
            if (_statsData != null)
                Object.DestroyImmediate(_statsData);
        }

        #region TryEquip Tests

        [Test]
        public void TryEquip_NullEquipment_ReturnsFalse()
        {
            bool result = _dataManager.TryEquip(null);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryEquip_FirstEquipment_ReturnsTrue()
        {
            var weapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);

            bool result = _dataManager.TryEquip(weapon);

            Assert.IsTrue(result);
            Assert.AreEqual(weapon, _dataManager.GetEquipment(EquipmentSlot.Weapon));

            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void TryEquip_HigherGrade_SameSlot_ReturnsTrue()
        {
            var normal = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);
            var rare = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare);

            _dataManager.TryEquip(normal);
            bool result = _dataManager.TryEquip(rare);

            Assert.IsTrue(result);
            Assert.AreEqual(rare, _dataManager.GetEquipment(EquipmentSlot.Weapon));

            Object.DestroyImmediate(normal);
            Object.DestroyImmediate(rare);
        }

        [Test]
        public void TryEquip_SameGrade_SameSlot_ReturnsFalse()
        {
            var weapon1 = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare, equipmentName: "Weapon1");
            var weapon2 = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare, equipmentName: "Weapon2");

            _dataManager.TryEquip(weapon1);
            bool result = _dataManager.TryEquip(weapon2);

            Assert.IsFalse(result);
            Assert.AreEqual(weapon1, _dataManager.GetEquipment(EquipmentSlot.Weapon));

            Object.DestroyImmediate(weapon1);
            Object.DestroyImmediate(weapon2);
        }

        [Test]
        public void TryEquip_LowerGrade_SameSlot_ReturnsFalse()
        {
            var rare = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare);
            var normal = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);

            _dataManager.TryEquip(rare);
            bool result = _dataManager.TryEquip(normal);

            Assert.IsFalse(result);
            Assert.AreEqual(rare, _dataManager.GetEquipment(EquipmentSlot.Weapon));

            Object.DestroyImmediate(rare);
            Object.DestroyImmediate(normal);
        }

        [Test]
        public void TryEquip_DifferentSlots_BothEquipped()
        {
            var weapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);
            var helmet = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Helmet, EquipmentGrade.Normal);

            _dataManager.TryEquip(weapon);
            _dataManager.TryEquip(helmet);

            Assert.AreEqual(weapon, _dataManager.GetEquipment(EquipmentSlot.Weapon));
            Assert.AreEqual(helmet, _dataManager.GetEquipment(EquipmentSlot.Helmet));

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(helmet);
        }

        [Test]
        public void TryEquip_AllSlots_AllEquipped()
        {
            var weapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);
            var helmet = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Helmet, EquipmentGrade.Normal);
            var armor = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Armor, EquipmentGrade.Normal);
            var gloves = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Gloves, EquipmentGrade.Normal);
            var boots = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Boots, EquipmentGrade.Normal);

            Assert.IsTrue(_dataManager.TryEquip(weapon));
            Assert.IsTrue(_dataManager.TryEquip(helmet));
            Assert.IsTrue(_dataManager.TryEquip(armor));
            Assert.IsTrue(_dataManager.TryEquip(gloves));
            Assert.IsTrue(_dataManager.TryEquip(boots));

            Assert.AreEqual(weapon, _dataManager.GetEquipment(EquipmentSlot.Weapon));
            Assert.AreEqual(helmet, _dataManager.GetEquipment(EquipmentSlot.Helmet));
            Assert.AreEqual(armor, _dataManager.GetEquipment(EquipmentSlot.Armor));
            Assert.AreEqual(gloves, _dataManager.GetEquipment(EquipmentSlot.Gloves));
            Assert.AreEqual(boots, _dataManager.GetEquipment(EquipmentSlot.Boots));

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(helmet);
            Object.DestroyImmediate(armor);
            Object.DestroyImmediate(gloves);
            Object.DestroyImmediate(boots);
        }

        #endregion

        #region Stat Modifier Tests

        [Test]
        public void TryEquip_AppliesAttackModifier()
        {
            float baseAttack = _combatant.Stats.AttackDamage.Value;
            var weapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 15f);

            _dataManager.TryEquip(weapon);

            Assert.AreEqual(baseAttack + 15f, _combatant.Stats.AttackDamage.Value, 0.01f);

            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void TryEquip_AppliesDefenseModifier()
        {
            float baseDefense = _combatant.Stats.Defense.Value;
            var armor = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Armor, EquipmentGrade.Normal, defenseBonus: 10f);

            _dataManager.TryEquip(armor);

            Assert.AreEqual(baseDefense + 10f, _combatant.Stats.Defense.Value, 0.01f);

            Object.DestroyImmediate(armor);
        }

        [Test]
        public void TryEquip_AppliesCriticalChanceModifier()
        {
            float baseCrit = _combatant.Stats.CriticalChance.Value;
            var gloves = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Gloves, EquipmentGrade.Normal, criticalChanceBonus: 0.05f);

            _dataManager.TryEquip(gloves);

            Assert.AreEqual(baseCrit + 0.05f, _combatant.Stats.CriticalChance.Value, 0.001f);

            Object.DestroyImmediate(gloves);
        }

        [Test]
        public void TryEquip_AppliesHealthBonus()
        {
            float baseHealth = _combatant.MaxHealth;
            var armor = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Armor, EquipmentGrade.Normal, healthBonus: 50f);

            _dataManager.TryEquip(armor);

            Assert.AreEqual(baseHealth + 50f, _combatant.MaxHealth, 0.01f);

            Object.DestroyImmediate(armor);
        }

        [Test]
        public void TryEquip_ReplaceEquipment_HealthBonusUpdated()
        {
            float baseHealth = _combatant.MaxHealth;
            var normalArmor = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Armor, EquipmentGrade.Normal, healthBonus: 30f);
            var rareArmor = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Armor, EquipmentGrade.Rare, healthBonus: 60f);

            _dataManager.TryEquip(normalArmor);
            Assert.AreEqual(baseHealth + 30f, _combatant.MaxHealth, 0.01f);

            _dataManager.TryEquip(rareArmor);
            Assert.AreEqual(baseHealth + 60f, _combatant.MaxHealth, 0.01f);

            Object.DestroyImmediate(normalArmor);
            Object.DestroyImmediate(rareArmor);
        }

        [Test]
        public void TryEquip_MultipleSlots_ModifiersStack()
        {
            float baseAttack = _combatant.Stats.AttackDamage.Value;
            var weapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 10f);
            var gloves = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Gloves, EquipmentGrade.Normal, attackBonus: 5f);

            _dataManager.TryEquip(weapon);
            _dataManager.TryEquip(gloves);

            Assert.AreEqual(baseAttack + 15f, _combatant.Stats.AttackDamage.Value, 0.01f);

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(gloves);
        }

        [Test]
        public void TryEquip_ReplaceEquipment_ModifiersUpdated()
        {
            float baseAttack = _combatant.Stats.AttackDamage.Value;
            var normalWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 10f);
            var rareWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare, attackBonus: 20f);

            _dataManager.TryEquip(normalWeapon);
            Assert.AreEqual(baseAttack + 10f, _combatant.Stats.AttackDamage.Value, 0.01f);

            _dataManager.TryEquip(rareWeapon);
            Assert.AreEqual(baseAttack + 20f, _combatant.Stats.AttackDamage.Value, 0.01f);

            Object.DestroyImmediate(normalWeapon);
            Object.DestroyImmediate(rareWeapon);
        }

        [Test]
        public void TryEquip_ReplaceOneSlot_OtherSlotModifiersRemain()
        {
            float baseAttack = _combatant.Stats.AttackDamage.Value;
            var normalWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal, attackBonus: 10f);
            var rareWeapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare, attackBonus: 20f);
            var gloves = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Gloves, EquipmentGrade.Normal, attackBonus: 5f);

            _dataManager.TryEquip(normalWeapon);
            _dataManager.TryEquip(gloves);
            Assert.AreEqual(baseAttack + 15f, _combatant.Stats.AttackDamage.Value, 0.01f);

            _dataManager.TryEquip(rareWeapon);
            Assert.AreEqual(baseAttack + 25f, _combatant.Stats.AttackDamage.Value, 0.01f);

            Object.DestroyImmediate(normalWeapon);
            Object.DestroyImmediate(rareWeapon);
            Object.DestroyImmediate(gloves);
        }

        #endregion

        #region Event Tests

        [Test]
        public void TryEquip_Success_InvokesOnEquipmentChanged()
        {
            var weapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);
            EquipmentData changedEquipment = null;
            _dataManager.OnEquipmentChanged += eq => changedEquipment = eq;

            _dataManager.TryEquip(weapon);

            Assert.AreEqual(weapon, changedEquipment);

            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void TryEquip_Failure_DoesNotInvokeOnEquipmentChanged()
        {
            var rare = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Rare);
            var normal = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);
            _dataManager.TryEquip(rare);

            bool eventInvoked = false;
            _dataManager.OnEquipmentChanged += _ => eventInvoked = true;

            _dataManager.TryEquip(normal);

            Assert.IsFalse(eventInvoked);

            Object.DestroyImmediate(rare);
            Object.DestroyImmediate(normal);
        }

        #endregion

        #region GetEquipment Tests

        [Test]
        public void GetEquipment_EmptySlot_ReturnsNull()
        {
            var result = _dataManager.GetEquipment(EquipmentSlot.Weapon);

            Assert.IsNull(result);
        }

        [Test]
        public void GetEquipment_EquippedSlot_ReturnsEquipment()
        {
            var weapon = EquipmentTestUtilities.CreateEquipmentData(
                EquipmentSlot.Weapon, EquipmentGrade.Normal);
            _dataManager.TryEquip(weapon);

            var result = _dataManager.GetEquipment(EquipmentSlot.Weapon);

            Assert.AreEqual(weapon, result);

            Object.DestroyImmediate(weapon);
        }

        #endregion
    }
}
