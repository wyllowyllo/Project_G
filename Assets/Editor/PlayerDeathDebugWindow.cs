using Combat.Core;
using UnityEditor;
using UnityEngine;

public class PlayerDeathDebugWindow : EditorWindow
{
    private float _damageAmount = 50f;
    private float _healAmount = 50f;
    private Combatant _playerCombatant;
    private Health _playerHealth;

    [MenuItem("Debug/Player Death Test %#k")] // Ctrl+Shift+K
    public static void ShowWindow()
    {
        GetWindow<PlayerDeathDebugWindow>("Player Death Test");
    }

    [MenuItem("Debug/Kill Player %#j")] // Ctrl+Shift+J
    public static void KillPlayerShortcut()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Debug] Play 모드에서만 사용 가능합니다.");
            return;
        }

        var combatant = FindPlayerCombatant();
        if (combatant != null)
        {
            combatant.TakeDamage(combatant.CurrentHealth + 1);
            Debug.Log("[Debug] 플레이어 즉시 사망 처리됨");
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("플레이어 사망 테스트", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서만 사용 가능합니다.", MessageType.Warning);
            return;
        }

        _playerCombatant = FindPlayerCombatant();
        _playerHealth = _playerCombatant?.GetComponent<Health>();

        if (_playerCombatant == null || _playerHealth == null)
        {
            EditorGUILayout.HelpBox("플레이어를 찾을 수 없습니다. (Combatant, Health 컴포넌트 필요)", MessageType.Error);
            return;
        }

        DrawHealthInfo();
        EditorGUILayout.Space();
        DrawDamageControls();
        EditorGUILayout.Space();
        DrawHealControls();
        EditorGUILayout.Space();
        DrawQuickActions();
    }

    private void DrawHealthInfo()
    {
        EditorGUILayout.LabelField("현재 상태", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("HP:", GUILayout.Width(30));

            float healthRatio = _playerCombatant.CurrentHealth / _playerCombatant.MaxHealth;
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(rect, healthRatio,
                $"{_playerCombatant.CurrentHealth:F0} / {_playerCombatant.MaxHealth:F0}");
        }

        EditorGUILayout.LabelField($"생존 여부: {(_playerCombatant.IsAlive ? "살아있음" : "사망")}");
    }

    private void DrawDamageControls()
    {
        EditorGUILayout.LabelField("데미지 컨트롤", EditorStyles.boldLabel);

        _damageAmount = EditorGUILayout.Slider("데미지 양", _damageAmount, 1f, _playerCombatant.MaxHealth);

        if (GUILayout.Button($"데미지 {_damageAmount:F0} 주기"))
        {
            _playerCombatant.TakeDamage(_damageAmount);
            Debug.Log($"[Debug] 플레이어에게 {_damageAmount:F0} 데미지");
        }
    }

    private void DrawHealControls()
    {
        EditorGUILayout.LabelField("힐 컨트롤", EditorStyles.boldLabel);

        _healAmount = EditorGUILayout.Slider("힐 양", _healAmount, 1f, _playerCombatant.MaxHealth);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button($"힐 {_healAmount:F0} 주기"))
        {
            _playerHealth.Heal(_healAmount);
            Debug.Log($"[Debug] 플레이어에게 {_healAmount:F0} 힐");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("25% 힐"))
            {
                _playerHealth.Heal(_playerCombatant.MaxHealth * 0.25f);
            }
            if (GUILayout.Button("50% 힐"))
            {
                _playerHealth.Heal(_playerCombatant.MaxHealth * 0.5f);
            }
            if (GUILayout.Button("75% 힐"))
            {
                _playerHealth.Heal(_playerCombatant.MaxHealth * 0.75f);
            }
            GUI.backgroundColor = Color.white;
        }
    }

    private void DrawQuickActions()
    {
        EditorGUILayout.LabelField("빠른 실행", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("25% 데미지"))
            {
                _playerCombatant.TakeDamage(_playerCombatant.MaxHealth * 0.25f);
            }
            if (GUILayout.Button("50% 데미지"))
            {
                _playerCombatant.TakeDamage(_playerCombatant.MaxHealth * 0.5f);
            }
            if (GUILayout.Button("75% 데미지"))
            {
                _playerCombatant.TakeDamage(_playerCombatant.MaxHealth * 0.75f);
            }
        }

        EditorGUILayout.Space();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("즉시 사망 (Ctrl+Shift+J)", GUILayout.Height(30)))
        {
            _playerCombatant.TakeDamage(_playerCombatant.CurrentHealth + 1);
            Debug.Log("[Debug] 플레이어 즉시 사망 처리됨");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("체력 전체 회복"))
        {
            _playerHealth.Heal(_playerCombatant.MaxHealth);
            Debug.Log("[Debug] 플레이어 체력 전체 회복");
        }
        GUI.backgroundColor = Color.white;
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private static Combatant FindPlayerCombatant()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.GetComponent<Combatant>();
        }
        return null;
    }
}
