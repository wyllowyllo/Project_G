using Combat.Core;
using UnityEditor;
using UnityEngine;

public class PlayerDeathDebugWindow : EditorWindow
{
    private float _damageAmount = 50f;
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

        var health = FindPlayerHealth();
        if (health != null)
        {
            health.TakeDamage(health.CurrentHealth + 1);
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

        _playerHealth = FindPlayerHealth();

        if (_playerHealth == null)
        {
            EditorGUILayout.HelpBox("플레이어를 찾을 수 없습니다. (Health 컴포넌트 필요)", MessageType.Error);
            return;
        }

        DrawHealthInfo();
        EditorGUILayout.Space();
        DrawDamageControls();
        EditorGUILayout.Space();
        DrawQuickActions();
    }

    private void DrawHealthInfo()
    {
        EditorGUILayout.LabelField("현재 상태", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("HP:", GUILayout.Width(30));

            float healthRatio = _playerHealth.CurrentHealth / _playerHealth.MaxHealth;
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.ProgressBar(rect, healthRatio,
                $"{_playerHealth.CurrentHealth:F0} / {_playerHealth.MaxHealth:F0}");
        }

        EditorGUILayout.LabelField($"생존 여부: {(_playerHealth.IsAlive ? "살아있음" : "사망")}");
    }

    private void DrawDamageControls()
    {
        EditorGUILayout.LabelField("데미지 컨트롤", EditorStyles.boldLabel);

        _damageAmount = EditorGUILayout.Slider("데미지 양", _damageAmount, 1f, _playerHealth.MaxHealth);

        if (GUILayout.Button($"데미지 {_damageAmount:F0} 주기"))
        {
            _playerHealth.TakeDamage(_damageAmount);
            Debug.Log($"[Debug] 플레이어에게 {_damageAmount:F0} 데미지");
        }
    }

    private void DrawQuickActions()
    {
        EditorGUILayout.LabelField("빠른 실행", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("25% 데미지"))
            {
                _playerHealth.TakeDamage(_playerHealth.MaxHealth * 0.25f);
            }
            if (GUILayout.Button("50% 데미지"))
            {
                _playerHealth.TakeDamage(_playerHealth.MaxHealth * 0.5f);
            }
            if (GUILayout.Button("75% 데미지"))
            {
                _playerHealth.TakeDamage(_playerHealth.MaxHealth * 0.75f);
            }
        }

        EditorGUILayout.Space();

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("즉시 사망 (Ctrl+Shift+J)", GUILayout.Height(30)))
        {
            _playerHealth.TakeDamage(_playerHealth.CurrentHealth + 1);
            Debug.Log("[Debug] 플레이어 즉시 사망 처리됨");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("체력 전체 회복"))
        {
            _playerHealth.Heal(_playerHealth.MaxHealth);
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

    private static Health FindPlayerHealth()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.GetComponent<Health>();
        }
        return null;
    }
}
