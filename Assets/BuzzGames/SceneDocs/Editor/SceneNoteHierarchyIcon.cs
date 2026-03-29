using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BuzzGames.SceneDocs
{
    [InitializeOnLoad]
    public class SceneNoteHierarchyIcon
    {
        private static Dictionary<int, string> noteCache = new Dictionary<int, string>();

        static SceneNoteHierarchyIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyChanged += RefreshCache;
            // Removido delayCall excessivo para evitar refresh desnecessário
        }

        public static void RefreshCache()
        {
            noteCache.Clear();
            // Otimização: FindObjectsByType é mais rápido que FindObjectsOfType nas versões novas (2023+), 
            // mas mantemos SortMode.None para performance máxima.
            SceneNoteBehaviour[] notes = Object.FindObjectsByType<SceneNoteBehaviour>(FindObjectsSortMode.None);
            foreach (var note in notes)
            {
                if (note != null && note.noteData != null)
                {
                    int id = note.gameObject.GetInstanceID();
                    if (!noteCache.ContainsKey(id)) noteCache.Add(id, note.noteData.noteTypeId);
                }
            }
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (!noteCache.TryGetValue(instanceID, out string typeId)) return;

#pragma warning disable 618
            // Pequena otimização: Evita chamar isso se não estiver no cache
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#pragma warning restore 618

            if (obj == null) { noteCache.Remove(instanceID); return; }

            DrawIcon(selectionRect, typeId);
        }

        private static void DrawIcon(Rect selectionRect, string typeId)
        {
            Color badgeColor = Color.gray;
            string label = "NOTE";

            // PERFORMANCE: Usa a instância cacheada do Settings
            var settings = SceneNoteSettings.Instance;
            if (settings != null)
            {
                // PERFORMANCE: Usa o método helper otimizado
                var config = settings.GetConfig(typeId);
                if (config != null)
                {
                    badgeColor = config.color;
                    label = config.label.Length > 3 ? config.label.Substring(0, 3) : config.label;
                }
            }

            float width = 60f; float rightMargin = 32f;
            Rect iconRect = new Rect(selectionRect.xMax - width - rightMargin, selectionRect.y, width, selectionRect.height);

            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = Color.white; style.alignment = TextAnchor.MiddleCenter; style.fontStyle = FontStyle.Bold; style.fontSize = 9;

            EditorGUI.DrawRect(iconRect, badgeColor);
            EditorGUI.LabelField(iconRect, label.ToUpper(), style);
        }
    }
}