using Combat.Core;
using Combat.Data;
using Progression;
using UnityEditor;
using UnityEngine;

namespace Tests.Shared
{
    public static class ProgressionTestUtilities
    {
        public static ProgressionConfig CreateProgressionConfig(
            int maxLevel = 30,
            int baseXp = 100,
            float exponent = 1.5f,
            float attackPerLevel = 5f,
            int qSkillLevel = 11,
            int eSkillLevel = 21,
            int rSkillLevel = 30)
        {
            var config = ScriptableObject.CreateInstance<ProgressionConfig>();
            var so = new SerializedObject(config);
            so.FindProperty("_maxLevel").intValue = maxLevel;
            so.FindProperty("_baseXp").intValue = baseXp;
            so.FindProperty("_exponent").floatValue = exponent;
            so.FindProperty("_attackPerLevel").floatValue = attackPerLevel;
            so.FindProperty("_qSkillLevel").intValue = qSkillLevel;
            so.FindProperty("_eSkillLevel").intValue = eSkillLevel;
            so.FindProperty("_rSkillLevel").intValue = rSkillLevel;
            so.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        public static CombatStatsData CreateStatsData(
            float baseAttackDamage = 10f,
            float criticalChance = 0.1f,
            float criticalMultiplier = 1.5f,
            float defense = 0f)
        {
            var data = ScriptableObject.CreateInstance<CombatStatsData>();
            var so = new SerializedObject(data);
            so.FindProperty("_baseAttackDamage").floatValue = baseAttackDamage;
            so.FindProperty("_criticalChance").floatValue = criticalChance;
            so.FindProperty("_criticalMultiplier").floatValue = criticalMultiplier;
            so.FindProperty("_defense").floatValue = defense;
            so.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        public static GameObject CreatePlayerWithProgression(
            ProgressionConfig config,
            CombatStatsData statsData,
            out Combatant combatant,
            out PlayerProgression progression)
        {
            // Create inactive to prevent Awake from running before configuration
            var obj = new GameObject("TestPlayer");
            obj.SetActive(false);

            obj.AddComponent<Health>();

            combatant = obj.AddComponent<Combatant>();
            var combatantSo = new SerializedObject(combatant);
            combatantSo.FindProperty("_statsData").objectReferenceValue = statsData;
            combatantSo.ApplyModifiedPropertiesWithoutUndo();

            progression = obj.AddComponent<PlayerProgression>();
            var progressionSo = new SerializedObject(progression);
            progressionSo.FindProperty("_config").objectReferenceValue = config;
            progressionSo.ApplyModifiedPropertiesWithoutUndo();

            // Activate to trigger Awake with properly configured fields
            obj.SetActive(true);

            return obj;
        }

        public static GameObject CreatePlayerWithProgression(
            ProgressionConfig config,
            CombatStatsData statsData,
            out PlayerProgression progression)
        {
            var obj = CreatePlayerWithProgression(config, statsData, out _, out progression);
            return obj;
        }

        public static GameObject CreateMonsterWithXpDrop(
            int xpReward,
            out Health health,
            out XpDropOnDeath xpDrop)
        {
            var obj = new GameObject("TestMonster");
            health = obj.AddComponent<Health>();
            xpDrop = obj.AddComponent<XpDropOnDeath>();

            var so = new SerializedObject(xpDrop);
            so.FindProperty("_xpReward").intValue = xpReward;
            so.ApplyModifiedPropertiesWithoutUndo();

            return obj;
        }
    }
}
