using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace BuzzGames.SceneDocs
{
    public class SceneNoteDashboard : EditorWindow
    {
        private Vector2 scrollPos;
        private List<SceneNoteBehaviour> allNotes = new List<SceneNoteBehaviour>();
        private string searchString = "";
        private bool useCategoryFilter = false;
        private int categoryFilterIndex = 0;
        private bool useStatusFilter = false;
        private NoteStatus filterStatus = NoteStatus.Open;

        [MenuItem("Tools/SceneDocs/Dashboard", false, 0)]
        public static void ShowWindow() => GetWindow<SceneNoteDashboard>("SceneDocs Board");

        private void OnEnable() => RefreshNotes();

        private void OnGUI()
        {
            SceneNoteSettings settings = SceneNoteSettings.Instance;
            if (settings == null) { EditorGUILayout.HelpBox("Configurações não encontradas!", MessageType.Error); return; }

            GUILayout.Label("📘 Painel SceneDocs", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("🔄 Atualizar", EditorStyles.toolbarButton, GUILayout.Width(80))) RefreshNotes();

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(1f, 0.6f, 0.2f);
            if (GUILayout.Button(new GUIContent(" ☁ Sincronizar Visíveis", EditorGUIUtility.IconContent("BuildSettings.Web.Small").image), EditorStyles.toolbarButton, GUILayout.Width(140))) SyncVisibleNotes();
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Label("🔍 Buscar:", GUILayout.Width(60));
            searchString = EditorGUILayout.TextField(searchString);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            useCategoryFilter = EditorGUILayout.Toggle(useCategoryFilter, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!useCategoryFilter);

            string[] catOptions = settings.noteTypes.Select(x => x.label).ToArray();
            if (catOptions.Length > 0) categoryFilterIndex = EditorGUILayout.Popup(categoryFilterIndex, catOptions);
            else EditorGUILayout.LabelField("Sem Categorias");

            EditorGUI.EndDisabledGroup();
            GUILayout.Space(10);
            useStatusFilter = EditorGUILayout.Toggle(useStatusFilter, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!useStatusFilter);
            filterStatus = (NoteStatus)EditorGUILayout.EnumPopup(filterStatus);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            EditorGUILayout.Space();
            DrawHeader();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            int visibleCount = 0;
            foreach (var note in allNotes)
            {
                if (ShouldShow(note, settings))
                {
                    DrawNoteRow(note, settings);
                    visibleCount++;
                }
            }
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Mostrando {visibleCount} / {allNotes.Count} Notas", EditorStyles.centeredGreyMiniLabel);
        }

        private bool ShouldShow(SceneNoteBehaviour note, SceneNoteSettings settings)
        {
            if (note == null || note.noteData == null) return false;
            if (!string.IsNullOrEmpty(searchString))
            {
                bool match = note.noteData.title.ToLower().Contains(searchString.ToLower()) || note.noteData.content.ToLower().Contains(searchString.ToLower());
                if (!match) return false;
            }
            if (useCategoryFilter && settings.noteTypes.Count > categoryFilterIndex)
            {
                string selectedId = settings.noteTypes[categoryFilterIndex].id;
                if (note.noteData.noteTypeId != selectedId) return false;
            }
            if (useStatusFilter && note.noteData.status != filterStatus) return false;
            return true;
        }

        private void SyncVisibleNotes()
        {
            SceneNoteSettings settings = SceneNoteSettings.Instance;
            List<SceneNoteBehaviour> notesToSync = new List<SceneNoteBehaviour>();
            foreach (var note in allNotes) if (ShouldShow(note, settings)) notesToSync.Add(note);

            if (notesToSync.Count == 0) { EditorUtility.DisplayDialog("Sync", "Nenhuma nota visível para sincronizar.", "OK"); return; }

            if (EditorUtility.DisplayDialog("Sincronizar Tudo", $"Enviar/Atualizar {notesToSync.Count} notas no Notion?", "Sim", "Cancelar"))
            {
                NotionSync.SyncBatch(notesToSync);
            }
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.Label("Título", GUILayout.Width(140));
            GUILayout.Label("Sprint", GUILayout.Width(60));
            GUILayout.Label("Categoria", GUILayout.Width(70));
            GUILayout.Label("Status", GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        private void DrawNoteRow(SceneNoteBehaviour note, SceneNoteSettings settings)
        {
            var config = settings.GetConfig(note.noteData.noteTypeId);
            Color c = config != null ? config.color : Color.white;
            string catName = config != null ? config.label : "?";

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.color = c;
            GUILayout.Label("●", GUILayout.Width(20));
            GUI.color = Color.white;

            GUILayout.Label(note.noteData.title, EditorStyles.boldLabel, GUILayout.Width(140));
            GUILayout.Label(note.noteData.sprint, EditorStyles.miniLabel, GUILayout.Width(60));
            GUILayout.Label(catName, EditorStyles.miniLabel, GUILayout.Width(70));
            GUILayout.Label(note.noteData.status.ToString(), EditorStyles.miniLabel, GUILayout.Width(70));

            if (GUILayout.Button("IR", EditorStyles.miniButton, GUILayout.Width(40)))
            {
                Selection.activeGameObject = note.gameObject;
                SceneView.lastActiveSceneView.FrameSelected();
            }
            GUILayout.EndHorizontal();
        }

        private void RefreshNotes()
        {
            allNotes.Clear();
            SceneNoteBehaviour[] found = FindObjectsByType<SceneNoteBehaviour>(FindObjectsSortMode.None);
            allNotes.AddRange(found);
        }
    }
}