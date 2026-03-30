using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace BuzzGames.SceneDocs
{
    [Overlay(typeof(SceneView), "SceneDocs", true)]
    public class SceneNoteOverlay : IMGUIOverlay
    {
        public static string GetSessionKey(string typeId) => $"SceneNote_Filter_{typeId}";
        public static string MasterKey => "SceneNotes_Master_Visible";

        public SceneNoteOverlay() : base() { }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();

            // --- BRANDING ---
            var labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

            GUILayout.Label("📘 SceneDocs", labelStyle, GUILayout.Width(85));
            GUILayout.Space(5);
            // ----------------

            GUIStyle btnStyle = EditorStyles.toolbarButton;

            // 1. MASTER TOGGLE
            bool isMasterOn = SessionState.GetBool(MasterKey, true);
            var eyeIcon = EditorGUIUtility.IconContent(isMasterOn ? "d_scenevis_visible" : "d_scenevis_hidden");
            eyeIcon.tooltip = "Mostrar/Esconder Todas as Notas";

            Color defaultColor = GUI.backgroundColor;
            if (!isMasterOn) GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);

            if (GUILayout.Button(eyeIcon, btnStyle, GUILayout.Width(28), GUILayout.Height(20)))
            {
                SessionState.SetBool(MasterKey, !isMasterOn);
                UpdateScene();
            }
            GUI.backgroundColor = defaultColor;

            // 2. ADD BUTTON
            var addIcon = EditorGUIUtility.IconContent("d_CreateAddNew");
            addIcon.tooltip = "Adicionar Nota";
            if (GUILayout.Button(addIcon, btnStyle, GUILayout.Width(28), GUILayout.Height(20))) ShowAddMenu();

            // 3. FILTER BUTTON
            bool hasFilterOff = HasAnyFilterOff();
            var filterIcon = EditorGUIUtility.IconContent(hasFilterOff ? "d_FilterByLabel" : "d_FilterSelectedOnly");
            filterIcon.tooltip = "Filtrar Categorias";

            if (hasFilterOff) GUI.backgroundColor = new Color(0.7f, 0.8f, 1f);

            if (GUILayout.Button(filterIcon, btnStyle, GUILayout.Width(28), GUILayout.Height(20)))
            {
                Rect btnRect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(btnRect, new SceneNoteFilterPopup());
            }
            GUI.backgroundColor = defaultColor;

            // 4. GIZMOS
            GUIContent gizmoIcon = EditorGUIUtility.IconContent("d_Settings");
            gizmoIcon.tooltip = "Ligar/Desligar Gizmos";

            if (SceneView.lastActiveSceneView != null)
            {
                bool current = SceneView.lastActiveSceneView.drawGizmos;
                bool newValue = GUILayout.Toggle(current, gizmoIcon, btnStyle, GUILayout.Width(28), GUILayout.Height(20));
                if (newValue != current) SceneView.lastActiveSceneView.drawGizmos = newValue;
            }

            GUILayout.EndHorizontal();
        }

        bool HasAnyFilterOff()
        {
            var settings = SceneNoteSettings.Instance;
            if (settings == null) return false;
            foreach (var type in settings.noteTypes)
            {
                if (!SessionState.GetBool(GetSessionKey(type.id), true)) return true;
            }
            return false;
        }

        static void ShowAddMenu()
        {
            GenericMenu menu = new GenericMenu();
            var settings = SceneNoteSettings.Instance;
            if (settings == null) return;

            foreach (var type in settings.noteTypes)
            {
                menu.AddItem(new GUIContent(type.label), false, () => SceneNoteMenus.CreateNoteFromOverlay(type.id));
            }
            menu.ShowAsContext();
        }

        public static void UpdateScene()
        {
            SceneNoteBehaviour[] notes = Object.FindObjectsByType<SceneNoteBehaviour>(FindObjectsSortMode.None);
            foreach (var note in notes) note.UpdateVisuals();
            SceneView.RepaintAll();
        }
    }

    // --- CLASSE DO POPUP (JANELA QUE NÃO FECHA) ---
    public class SceneNoteFilterPopup : PopupWindowContent
    {
        public override Vector2 GetWindowSize()
        {
            var settings = SceneNoteSettings.Instance;
            int count = settings != null ? settings.noteTypes.Count : 0;
            return new Vector2(200, 30 + (count * 22) + 10);
        }

        public override void OnGUI(Rect rect)
        {
            var settings = SceneNoteSettings.Instance;
            if (settings == null) { EditorGUILayout.LabelField("Sem Configurações"); return; }

            GUILayout.BeginArea(new Rect(5, 5, rect.width - 10, rect.height - 10));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Tudo", EditorStyles.miniButtonLeft)) SetAll(true);
            if (GUILayout.Button("Nenhum", EditorStyles.miniButtonRight)) SetAll(false);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            foreach (var type in settings.noteTypes)
            {
                string key = SceneNoteOverlay.GetSessionKey(type.id);
                bool isVisible = SessionState.GetBool(key, true);

                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal();
                bool newState = EditorGUILayout.ToggleLeft(type.label, isVisible);

                var originalColor = GUI.color;
                GUI.color = type.color;
                GUILayout.Label(EditorGUIUtility.IconContent("d_FilterSelectedOnly"), GUILayout.Width(20));
                GUI.color = originalColor;

                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    SessionState.SetBool(key, newState);
                    SceneNoteOverlay.UpdateScene();
                }
            }

            GUILayout.EndArea();
        }

        void SetAll(bool state)
        {
            var settings = SceneNoteSettings.Instance;
            foreach (var type in settings.noteTypes)
            {
                SessionState.SetBool(SceneNoteOverlay.GetSessionKey(type.id), state);
            }
            SceneNoteOverlay.UpdateScene();
        }
    }
}