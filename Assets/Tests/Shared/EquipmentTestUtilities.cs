using Combat.Core;
using Combat.Data;
using Equipment;
using UnityEditor;
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
            var data = ScriptableObject.CreateInstance<EquipmentData>();
            var so = new SerializedObject(data);
            so.FindProperty("_equipmentName").stringValue = equipmentName;
            so.FindProperty("_slot").enumValueIndex = (int)slot;
            so.FindProperty("_grade").enumValueIndex = (int)grade;
            so.FindProperty("_attackBonus").floatValue = attackBonus;
            so.FindProperty("_defenseBonus").floatValue = defenseBonus;
            so.FindProperty("_criticalChanceBonus").floatValue = criticalChanceBonus;
            so.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        public static DropTableData CreateDropTableData(
            float dropChance = 1f,
            int normalWeight = 60,
            int rareWeight = 25,
            int uniqueWeight = 12,
            int legendaryWeight = 3)
        {
            var data = ScriptableObject.CreateInstance<DropTableData>();
            var so = new SerializedObject(data);
            so.FindProperty("_dropChance").floatValue = dropChance;
            so.FindProperty("_normalWeight").intValue = normalWeight;
            so.FindProperty("_rareWeight").intValue = rareWeight;
            so.FindProperty("_uniqueWeight").intValue = uniqueWeight;
            so.FindProperty("_legendaryWeight").intValue = legendaryWeight;
            so.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        public static void SetDropTablePools(
            DropTableData dropTable,
            EquipmentData[] normalPool = null,
            EquipmentData[] rarePool = null,
            EquipmentData[] uniquePool = null,
            EquipmentData[] legendaryPool = null)
        {
            var so = new SerializedObject(dropTable);

            SetArrayProperty(so, "_normalPool", normalPool);
            SetArrayProperty(so, "_rarePool", rarePool);
            SetArrayProperty(so, "_uniquePool", uniquePool);
            SetArrayProperty(so, "_legendaryPool", legendaryPool);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArrayProperty(SerializedObject so, string propertyName, EquipmentData[] items)
        {
            var prop = so.FindProperty(propertyName);
            if (items == null)
            {
                prop.arraySize = 0;
            }
            else
            {
                prop.arraySize = items.Length;
                for (int i = 0; i < items.Length; i++)
                {
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
                }
            }
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
            var combatantSo = new SerializedObject(combatant);
            combatantSo.FindProperty("_statsData").objectReferenceValue = statsData;
            combatantSo.ApplyModifiedPropertiesWithoutUndo();

            playerEquipment = obj.AddComponent<PlayerEquipment>();

            obj.SetActive(true);

            return obj;
        }

public static DroppedEquipment CreateDroppedEquipmentPrefab()
        {
            var obj = new GameObject("DroppedEquipmentPrefab");
            var dropped = obj.AddComponent<DroppedEquipment>();
            return dropped;
        }

        public static GameObject CreateMonsterWithEquipmentDrop(
            DropTableData dropTable,
            DroppedEquipment droppedEquipmentPrefab,
            out Health health,
            out EquipmentDropOnDeath equipmentDrop)
        {
            var obj = new GameObject("TestMonster");
            health = obj.AddComponent<Health>();
            equipmentDrop = obj.AddComponent<EquipmentDropOnDeath>();

            var so = new SerializedObject(equipmentDrop);
            so.FindProperty("_dropTable").objectReferenceValue = dropTable;
            so.FindProperty("_droppedEquipmentPrefab").objectReferenceValue = droppedEquipmentPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            return obj;
        }
    }
}
