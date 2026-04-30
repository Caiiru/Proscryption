using UnityEngine;
using UnityEditor;

namespace proscryption
{
    public class ColorManager : MonoBehaviour
    {
        private static readonly int BloodEmissiveColor = Shader.PropertyToID("_Blood_Emissive_Color");
        private static readonly int LightEmissiveColor = Shader.PropertyToID("_Light_Emissive_Color");

        public ColorDatabase colorDatabase;

        #region Singleton

        public static ColorManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnValidate()
        {
            SetupValidate();
            if (Instance != null && Instance != this)
            {
                return;
            }

            Instance = this;
        }

        #endregion

        public void SetupValidate()
        {
            // Debug.Log("Validate Colors");
            Shader.SetGlobalColor(BloodEmissiveColor, colorDatabase.BloodColor);
            Shader.SetGlobalColor(LightEmissiveColor, colorDatabase.LightColor);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ColorManager))]
    public class ColorManagerEditor : Editor
    {
        Editor _cachedEditor;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ColorManager manager = (ColorManager)target;
            if (manager.colorDatabase != null)
            {
                EditorGUILayout.Space();
                if (_cachedEditor == null || _cachedEditor.target != manager.colorDatabase)
                {
                    _cachedEditor = Editor.CreateEditor(manager.colorDatabase);
                }

                _cachedEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a ColorDatabase to see preview.", MessageType.Info);
            }
        }
    }
#endif
}