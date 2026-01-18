using UnityEngine;
using UnityEditor;
using System.IO;

namespace Equipment.Editor
{
    /// <summary>
    /// 프리팹을 자동으로 스크린샷 찍어서 Sprite로 변환하는 에디터 도구
    /// 투명 배경을 위해 임시 레이어를 사용합니다
    /// </summary>
    public class PrefabToSpriteConverter : EditorWindow
    {
        private GameObject _targetPrefab;
        private int _imageSize = 512;
        private float _cameraDistance = 3f;
        private Vector3 _rotation = new Vector3(15f, -30f, 0f);
        private Vector3 _modelOffset = new Vector3(0f, 0f, 0f); // 모델 위치 조정
        private Vector3 _lookAtOffset = new Vector3(0f, 0f, 0f); // 카메라가 보는 지점 조정
        private Color _backgroundColor = new Color(0, 0, 0, 0); // 투명
        private string _savePath = "Assets/Equipment/Icons/";
        
        // 프리셋
        private bool _showPresets = false;
        
        // 임시 렌더링용 레이어 (31번 사용)
        private const int TEMP_RENDER_LAYER = 31;

        [MenuItem("Tools/Equipment/Prefab to Sprite Converter")]
        public static void ShowWindow()
        {
            GetWindow<PrefabToSpriteConverter>("Prefab → Sprite");
        }

        private void OnGUI()
        {
            GUILayout.Label("프리팹을 Sprite로 변환", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _targetPrefab = (GameObject)EditorGUILayout.ObjectField(
                "장비 프리팹", 
                _targetPrefab, 
                typeof(GameObject), 
                false
            );

            GUILayout.Space(10);
            
            // 프리셋 버튼
            _showPresets = EditorGUILayout.Foldout(_showPresets, "프리셋", true);
            if (_showPresets)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("무기 (세로)"))
                {
                    _rotation = new Vector3(0f, 0f, 0f);
                    _modelOffset = new Vector3(0f, 0f, 0f);
                    _lookAtOffset = new Vector3(0f, -0.3f, 0f); // 약간 아래 보기
                    _cameraDistance = 3f;
                }
                if (GUILayout.Button("무기 (대각선)"))
                {
                    _rotation = new Vector3(15f, -30f, 0f);
                    _modelOffset = new Vector3(0f, 0f, 0f);
                    _lookAtOffset = new Vector3(0f, -0.2f, 0f);
                    _cameraDistance = 3f;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("방어구"))
                {
                    _rotation = new Vector3(15f, -30f, 0f);
                    _modelOffset = new Vector3(0f, 0f, 0f);
                    _lookAtOffset = new Vector3(0f, 0f, 0f);
                    _cameraDistance = 2.5f;
                }
                if (GUILayout.Button("초기화"))
                {
                    _rotation = new Vector3(15f, -30f, 0f);
                    _modelOffset = Vector3.zero;
                    _lookAtOffset = Vector3.zero;
                    _cameraDistance = 3f;
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("기본 설정", EditorStyles.boldLabel);

            _imageSize = EditorGUILayout.IntSlider("이미지 크기", _imageSize, 128, 2048);
            _cameraDistance = EditorGUILayout.Slider("카메라 거리", _cameraDistance, 1f, 10f);
            _backgroundColor = EditorGUILayout.ColorField("배경색", _backgroundColor);

            GUILayout.Space(10);
            GUILayout.Label("위치 조정", EditorStyles.boldLabel);
            
            _rotation = EditorGUILayout.Vector3Field("회전 각도", _rotation);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("모델 위치 조정", GUILayout.Width(120));
            _modelOffset = EditorGUILayout.Vector3Field("", _modelOffset);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("무기가 위로 치우쳐 있으면 Y값을 낮춰보세요 (예: -0.5)", MessageType.None);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("카메라 보는 지점", GUILayout.Width(120));
            _lookAtOffset = EditorGUILayout.Vector3Field("", _lookAtOffset);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("카메라를 아래로 향하게 하려면 Y값을 낮춰보세요 (예: -0.3)", MessageType.None);

            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("저장 경로:", GUILayout.Width(80));
            _savePath = EditorGUILayout.TextField(_savePath);
            if (GUILayout.Button("찾기", GUILayout.Width(50)))
            {
                string path = EditorUtility.SaveFolderPanel("저장 폴더 선택", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _savePath = "Assets" + path.Substring(Application.dataPath.Length) + "/";
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUI.enabled = _targetPrefab != null;
            if (GUILayout.Button("이미지로 변환", GUILayout.Height(40)))
            {
                ConvertPrefabToSprite();
            }
            GUI.enabled = true;

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "💡 빠른 팁:\n" +
                "• 무기가 위로 치우쳐 있다면: '카메라 보는 지점' Y값을 -0.3 정도로\n" +
                "• 또는 '모델 위치 조정' Y값을 -0.5 정도로\n" +
                "• 프리셋 버튼으로 빠르게 시작하세요!",
                MessageType.Info
            );
        }

        private void ConvertPrefabToSprite()
        {
            if (_targetPrefab == null)
            {
                EditorUtility.DisplayDialog("오류", "프리팹을 선택해주세요!", "확인");
                return;
            }

            // 저장 경로 확인/생성
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            // 임시 씬에 프리팹 생성
            GameObject instance = Instantiate(_targetPrefab);
            instance.transform.position = _modelOffset; // 모델 위치 조정 적용
            instance.transform.rotation = Quaternion.Euler(_rotation);

            // ⭐ 중요: 프리팹과 모든 자식의 레이어를 임시 레이어로 변경
            SetLayerRecursively(instance, TEMP_RENDER_LAYER);

            // 카메라가 바라볼 지점 계산
            Vector3 lookAtPoint = instance.transform.position + _lookAtOffset;

            // 임시 카메라 생성
            GameObject cameraObj = new GameObject("TempIconCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.transform.position = lookAtPoint + new Vector3(0, 0, -_cameraDistance);
            camera.transform.LookAt(lookAtPoint); // 조정된 지점을 바라봄
            
            // 카메라 설정 - 투명 배경을 위한 설정
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = _backgroundColor;
            camera.cullingMask = 1 << TEMP_RENDER_LAYER; // ⭐ 임시 레이어만 렌더링
            camera.orthographic = false;
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;

            // 조명 추가
            GameObject lightObj = new GameObject("TempIconLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1f;
            light.color = Color.white;
            light.cullingMask = 1 << TEMP_RENDER_LAYER; // 임시 레이어만 비춤

            // RenderTexture 생성
            RenderTexture renderTexture = new RenderTexture(_imageSize, _imageSize, 24, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 4; // 안티앨리어싱
            camera.targetTexture = renderTexture;

            // 렌더링
            camera.Render();

            // Texture2D로 변환 (알파 채널 포함)
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(_imageSize, _imageSize, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, _imageSize, _imageSize), 0, 0);
            texture.Apply();

            // PNG로 저장 (알파 채널 보존)
            byte[] bytes = texture.EncodeToPNG();
            string fileName = $"{_targetPrefab.name}_Icon.png";
            string fullPath = Path.Combine(_savePath, fileName);
            File.WriteAllBytes(fullPath, bytes);

            // 정리
            RenderTexture.active = null;
            renderTexture.Release();
            DestroyImmediate(renderTexture);
            DestroyImmediate(cameraObj);
            DestroyImmediate(lightObj);
            DestroyImmediate(instance);
            DestroyImmediate(texture);

            // 에셋 새로고침
            AssetDatabase.Refresh();

            // Texture Importer 설정을 Sprite로 변경
            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; // ⭐ 알파 투명도 활성화
                importer.alphaSource = TextureImporterAlphaSource.FromInput; // 알파 채널 사용
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog(
                "완료!", 
                $"투명 배경 이미지가 생성되었습니다!\n경로: {fullPath}\n\n이제 EquipmentData의 Icon 필드에 할당하세요.", 
                "확인"
            );

            // 생성된 파일 선택
            Object obj = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        /// <summary>
        /// GameObject와 모든 자식의 레이어를 재귀적으로 설정
        /// </summary>
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
