using UnityEngine;
using UnityEditor;
using Skill;

[CustomEditor(typeof(PlayerSkillData))]
public class PlayerSkillDataEditor : Editor
{
    private SerializedProperty _skillName;
    private SerializedProperty _slot;
    private SerializedProperty _icon;
    private SerializedProperty _areaType;
    private SerializedProperty _tiers;

    private void OnEnable()
    {
        _skillName = serializedObject.FindProperty("_skillName");
        _slot = serializedObject.FindProperty("_slot");
        _icon = serializedObject.FindProperty("_icon");
        _areaType = serializedObject.FindProperty("_areaType");
        _tiers = serializedObject.FindProperty("_tiers");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Meta", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_skillName);
        EditorGUILayout.PropertyField(_slot);
        EditorGUILayout.PropertyField(_icon);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Area", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_areaType);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tiers (0: Base, 1: Enhanced)", EditorStyles.boldLabel);

        var areaType = (SkillAreaType)_areaType.enumValueIndex;

        for (int i = 0; i < _tiers.arraySize; i++)
        {
            var tier = _tiers.GetArrayElementAtIndex(i);
            DrawTier(tier, i, areaType);
        }

        if (GUILayout.Button("Add Tier"))
        {
            _tiers.InsertArrayElementAtIndex(_tiers.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTier(SerializedProperty tier, int index, SkillAreaType areaType)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        string tierLabel = index == 0 ? "Base" : $"Enhanced {index}";
        tier.isExpanded = EditorGUILayout.Foldout(tier.isExpanded, tierLabel, true);

        if (tier.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Combat", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_damageMultiplier"));
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_range"));
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_cooldown"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Area", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_positionOffset"));

            switch (areaType)
            {
                case SkillAreaType.Box:
                    EditorGUILayout.PropertyField(tier.FindPropertyRelative("_boxWidth"));
                    EditorGUILayout.PropertyField(tier.FindPropertyRelative("_boxHeight"));
                    break;

                case SkillAreaType.Cone:
                    EditorGUILayout.PropertyField(tier.FindPropertyRelative("_angle"));
                    EditorGUILayout.PropertyField(tier.FindPropertyRelative("_coneHeight"));
                    break;

                case SkillAreaType.Sphere:
                    EditorGUILayout.HelpBox("Sphere uses Range as radius", MessageType.Info);
                    break;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Movement", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_allowMovement"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Presentation", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_effectPrefabs"), true);
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_vfxPositionOffset"));
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_vfxRotationOffset"));
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_skillSound"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Camera", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_cameraConfig"));
            EditorGUILayout.PropertyField(tier.FindPropertyRelative("_animationDuration"));

            if (GUILayout.Button("Remove Tier"))
            {
                _tiers.DeleteArrayElementAtIndex(index);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }
}
