using Combat.Core;
using Combat.Data;
using Equipment;
using UnityEngine;

namespace Tests.Shared
{
    public static class EquipmentTestUtilities
    {
        public static EquipmentData CreateEquipmentData(
            EquipmentSlot slot,
            EquipmentGrade grade,
            float attackBonus = 0f,
            float defenseBonus = 0f,
            float criticalChanceBonus = 0f,
            string equipmentName = "TestEquipment")
        {
            return EquipmentData.CreateForTest(slot, grade, attackBonus, defenseBonus, criticalChanceBonus, equipmentName);
        }

        public static DropTableData CreateDropTableData(
            float dropChance = 1f,
            int normalWeight = 60,
            int rareWeight = 25,
            int uniqueWeight = 12,
            int legendaryWeight = 3)
        {
            return DropTableData.CreateForTest(dropChance, normalWeight, rareWeight, uniqueWeight, legendaryWeight);
        }

        public static void SetDropTablePools(
            DropTableData dropTable,
            GameObject[] normalPool = null,
            GameObject[] rarePool = null,
            GameObject[] uniquePool = null,
            GameObject[] legendaryPool = null)
        {
            dropTable.SetPoolsForTest(normalPool, rarePool, uniquePool, legendaryPool);
        }

        public static GameObject CreatePlayerWithEquipment(
            CombatStatsData statsData,
            out Combatant combatant,
            out PlayerEquipment playerEquipment)
        {
            var obj = new GameObject("TestPlayer");
            obj.SetActive(false);

            obj.AddComponent<Health>();

            combatant = obj.AddComponent<Combatant>();
            combatant.SetStatsDataForTest(statsData);

            playerEquipment = obj.AddComponent<PlayerEquipment>();

            obj.SetActive(true);

            return obj;
        }

        public static GameObject CreateDroppedEquipmentPrefab(EquipmentData equipmentData = null)
        {
            var obj = new GameObject("DroppedEquipmentPrefab");
            obj.AddComponent<BoxCollider>();
            var dropped = obj.AddComponent<DroppedEquipment>();
            if (equipmentData != null)
            {
                dropped.Initialize(equipmentData);
            }
            return obj;
        }

        public static GameObject CreateMonsterWithEquipmentDrop(
            DropTableData dropTable,
            out Health health,
            out EquipmentDropOnDeath equipmentDrop)
        {
            var obj = new GameObject("TestMonster");
            health = obj.AddComponent<Health>();
            equipmentDrop = obj.AddComponent<EquipmentDropOnDeath>();

            equipmentDrop.SetDropTableForTest(dropTable);

            return obj;
        }
    }
}
