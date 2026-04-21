using UnityEditor;
using UnityEngine;

namespace proscryption
{
    public class ColorManager : MonoBehaviour
    {

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
        void OValidate()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion
    }

    [CustomEditor(typeof(ColorManager))]
    public class ColorManagerEditor : Editor
    {
        Editor cachedEditor;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ColorManager manager = (ColorManager)target;
            if (manager.colorDatabase != null)
            {
                EditorGUILayout.Space();
                if (cachedEditor == null || cachedEditor.target != manager.colorDatabase)
                {
                    cachedEditor = Editor.CreateEditor(manager.colorDatabase);

                }
                cachedEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a ColorDatabase to see preview.", MessageType.Info);
            }
        }
    }
}
