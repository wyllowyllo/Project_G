using Dungeon;
using Monster.AI;
using Monster.Manager;
using UnityEditor;
using UnityEngine;

public class DungeonClearDebugWindow : EditorWindow
{
    private Vector2 _scrollPosition;

    [MenuItem("Debug/Dungeon Clear Test %#d")]
    public static void ShowWindow()
    {
        GetWindow<DungeonClearDebugWindow>("Dungeon Clear Test");
    }

    // 8번 키 단축키로 전체 몬스터 처치
    [MenuItem("Debug/Kill All Monsters _8")]
    public static void KillAllMonsters()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Debug] Play 모드에서만 사용 가능합니다.");
            return;
        }

        if (MonsterTracker.Instance == null)
        {
            Debug.LogWarning("[Debug] MonsterTracker를 찾을 수 없습니다.");
            return;
        }

        var monsters = MonsterTracker.Instance.GetAliveMonsters();
        int killCount = 0;

        foreach (var monster in monsters)
        {
            if (monster != null && monster.IsAlive && monster.Combatant != null)
            {
                float overkillDamage = monster.Combatant.MaxHealth + 1f;
                monster.Combatant.TakeDamage(overkillDamage);
                killCount++;
            }
        }

        Debug.Log($"[Debug] 전체 몬스터 처치: {killCount}마리");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("던전 클리어 테스트", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서만 사용 가능합니다.", MessageType.Warning);
            return;
        }

        if (DungeonManager.Instance == null)
        {
            EditorGUILayout.HelpBox("DungeonManager를 찾을 수 없습니다.", MessageType.Error);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawCurrentStatus();
        EditorGUILayout.Space();
        DrawDungeonList();
        EditorGUILayout.Space();
        DrawActions();
        EditorGUILayout.Space();
        DrawDataManagement();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCurrentStatus()
    {
        EditorGUILayout.LabelField("현재 상태", EditorStyles.boldLabel);

        var manager = DungeonManager.Instance;
        var isInDungeon = manager.IsInDungeon;

        EditorGUILayout.LabelField($"던전 진입 여부: {(isInDungeon ? "진입 중" : "미진입")}");

        if (isInDungeon && manager.CurrentDungeon != null)
        {
            var dungeon = manager.CurrentDungeon;
            EditorGUILayout.LabelField($"현재 던전: {dungeon.DisplayName}");
            EditorGUILayout.LabelField($"권장 레벨: {dungeon.RecommendedLevel}");
            EditorGUILayout.LabelField($"클리어 보상 XP: {dungeon.ClearXpReward}");
        }

        int clearedCount = 0;
        int totalCount = manager.AllDungeons.Count;

        foreach (var dungeon in manager.AllDungeons)
        {
            if (manager.IsDungeonCleared(dungeon.DungeonId))
                clearedCount++;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("진행률:", GUILayout.Width(50));
            var rect = EditorGUILayout.GetControlRect();
            float progress = totalCount > 0 ? (float)clearedCount / totalCount : 0f;
            EditorGUI.ProgressBar(rect, progress, $"{clearedCount} / {totalCount}");
        }
    }

    private void DrawDungeonList()
    {
        EditorGUILayout.LabelField("던전 목록", EditorStyles.boldLabel);

        var manager = DungeonManager.Instance;

        foreach (var dungeon in manager.AllDungeons)
        {
            bool isCleared = manager.IsDungeonCleared(dungeon.DungeonId);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                string status = isCleared ? "[O]" : "[X]";
                EditorGUILayout.LabelField($"{status} {dungeon.DisplayName}", GUILayout.ExpandWidth(true));

                if (GUILayout.Button(isCleared ? "미클리어" : "클리어", GUILayout.Width(60)))
                {
                    ToggleDungeonClear(dungeon.DungeonId, !isCleared);
                }
            }
        }
    }

    private void DrawActions()
    {
        EditorGUILayout.LabelField("액션", EditorStyles.boldLabel);

        var manager = DungeonManager.Instance;
        bool isInDungeon = manager.IsInDungeon;

        using (new EditorGUI.DisabledGroupScope(!isInDungeon))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("현재 던전 클리어", GUILayout.Height(25)))
                {
                    manager.CompleteDungeon();
                    Debug.Log("[Debug] 던전 클리어 처리됨");
                }
                GUI.backgroundColor = Color.white;

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("현재 던전 실패", GUILayout.Height(25)))
                {
                    manager.FailDungeon();
                    Debug.Log("[Debug] 던전 실패 처리됨");
                }
                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button("마을로 복귀"))
            {
                manager.ReturnToTown();
                Debug.Log("[Debug] 마을로 복귀");
            }
        }

        if (!isInDungeon)
        {
            EditorGUILayout.HelpBox("던전에 진입해야 액션을 사용할 수 있습니다.", MessageType.Info);
        }
    }

    private void DrawDataManagement()
    {
        EditorGUILayout.LabelField("데이터 관리", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("모든 던전 클리어"))
            {
                SetAllDungeonsClear(true);
                Debug.Log("[Debug] 모든 던전 클리어 처리됨");
            }
            GUI.backgroundColor = Color.white;

            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("모든 클리어 기록 초기화"))
            {
                SetAllDungeonsClear(false);
                Debug.Log("[Debug] 모든 클리어 기록 초기화됨");
            }
            GUI.backgroundColor = Color.white;
        }
    }

    private void ToggleDungeonClear(string dungeonId, bool cleared)
    {
        if (cleared)
            PlayerPrefs.SetInt($"Dungeon_{dungeonId}_Cleared", 1);
        else
            PlayerPrefs.DeleteKey($"Dungeon_{dungeonId}_Cleared");

        DungeonManager.Instance.Editor_SetDungeonCleared(dungeonId, cleared);
        PlayerPrefs.Save();
        Debug.Log($"[Debug] 던전 '{dungeonId}' 클리어 상태: {cleared}");
    }

    private void SetAllDungeonsClear(bool cleared)
    {
        var manager = DungeonManager.Instance;

        foreach (var dungeon in manager.AllDungeons)
        {
            if (cleared)
                PlayerPrefs.SetInt($"Dungeon_{dungeon.DungeonId}_Cleared", 1);
            else
                PlayerPrefs.DeleteKey($"Dungeon_{dungeon.DungeonId}_Cleared");

            manager.Editor_SetDungeonCleared(dungeon.DungeonId, cleared);
        }
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}
