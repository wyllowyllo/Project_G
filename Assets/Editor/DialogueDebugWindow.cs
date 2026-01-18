using Dialogue;
using UnityEditor;
using UnityEngine;

public class DialogueDebugWindow : EditorWindow
{
    private const string INTRO_SHOWN_KEY = "IntroDialogueShown";

    private DialogueData _selectedDialogue;
    private Vector2 _scrollPosition;

    [MenuItem("Debug/Dialogue Test %#t")]
    public static void ShowWindow()
    {
        GetWindow<DialogueDebugWindow>("Dialogue Test");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("대화 테스트", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play 모드에서만 사용 가능합니다.", MessageType.Warning);
            DrawDataManagement();
            return;
        }

        if (DialogueManager.Instance == null)
        {
            EditorGUILayout.HelpBox("DialogueManager를 찾을 수 없습니다.", MessageType.Error);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawCurrentStatus();
        EditorGUILayout.Space();
        DrawDialogueSelector();
        EditorGUILayout.Space();
        DrawDataManagement();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCurrentStatus()
    {
        EditorGUILayout.LabelField("현재 상태", EditorStyles.boldLabel);

        var manager = DialogueManager.Instance;
        bool isActive = manager.IsDialogueActive;

        EditorGUILayout.LabelField($"대화 진행 중: {(isActive ? "예" : "아니오")}");

        bool introShown = PlayerPrefs.GetInt(INTRO_SHOWN_KEY, 0) == 1;
        EditorGUILayout.LabelField($"인트로 대화 표시됨: {(introShown ? "예" : "아니오")}");
    }

    private void DrawDialogueSelector()
    {
        EditorGUILayout.LabelField("대화 재생", EditorStyles.boldLabel);

        _selectedDialogue = (DialogueData)EditorGUILayout.ObjectField(
            "대화 에셋",
            _selectedDialogue,
            typeof(DialogueData),
            false
        );

        if (_selectedDialogue != null)
        {
            EditorGUILayout.LabelField($"라인 수: {_selectedDialogue.LineCount}");

            // 라인 미리보기
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("미리보기", EditorStyles.miniLabel);
                for (int i = 0; i < Mathf.Min(_selectedDialogue.LineCount, 3); i++)
                {
                    var line = _selectedDialogue.Lines[i];
                    string preview = line.Text.Length > 40 ? line.Text[..40] + "..." : line.Text;
                    EditorGUILayout.LabelField($"{line.SpeakerName}: {preview}", EditorStyles.wordWrappedMiniLabel);
                }
                if (_selectedDialogue.LineCount > 3)
                {
                    EditorGUILayout.LabelField($"... 외 {_selectedDialogue.LineCount - 3}개", EditorStyles.miniLabel);
                }
            }
        }

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledGroupScope(_selectedDialogue == null || DialogueManager.Instance.IsDialogueActive))
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("대화 재생", GUILayout.Height(30)))
            {
                DialogueManager.Instance.ShowDialogue(_selectedDialogue);
                Debug.Log($"[Debug] 대화 재생: {_selectedDialogue.name}");
            }
            GUI.backgroundColor = Color.white;
        }

        if (DialogueManager.Instance.IsDialogueActive)
        {
            EditorGUILayout.HelpBox("현재 대화가 진행 중입니다.", MessageType.Info);
        }
    }

    private void DrawDataManagement()
    {
        EditorGUILayout.LabelField("데이터 관리", EditorStyles.boldLabel);

        bool introShown = PlayerPrefs.GetInt(INTRO_SHOWN_KEY, 0) == 1;

        GUI.backgroundColor = introShown ? Color.yellow : Color.gray;
        if (GUILayout.Button(introShown ? "인트로 대화 리셋 (다시 표시)" : "인트로 이미 리셋됨"))
        {
            PlayerPrefs.DeleteKey(INTRO_SHOWN_KEY);
            PlayerPrefs.Save();
            Debug.Log("[Debug] 인트로 대화 리셋됨 - 다음 TownScene 진입 시 표시됩니다");
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
}
