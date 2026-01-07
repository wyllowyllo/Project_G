using Combat.Core;
using Combat.Data;
using Progression;
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
            return ProgressionConfig.CreateForTest(maxLevel, baseXp, exponent, attackPerLevel, qSkillLevel, eSkillLevel, rSkillLevel);
        }

        public static CombatStatsData CreateStatsData(
            float baseAttackDamage = 10f,
            float criticalChance = 0.1f,
            float criticalMultiplier = 1.5f,
            float defense = 0f)
        {
            return CombatStatsData.CreateForTest(baseAttackDamage, criticalChance, criticalMultiplier, defense);
        }

        public static GameObject CreatePlayerWithProgression(
            ProgressionConfig config,
            CombatStatsData statsData,
            out Combatant combatant,
            out PlayerProgression progression)
        {
            var obj = new GameObject("TestPlayer");
            obj.SetActive(false);

            obj.AddComponent<Health>();

            combatant = obj.AddComponent<Combatant>();
            combatant.SetStatsDataForTest(statsData);

            progression = obj.AddComponent<PlayerProgression>();
            progression.SetConfigForTest(config);

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

            xpDrop.SetXpRewardForTest(xpReward);

            return obj;
        }
    }
}
