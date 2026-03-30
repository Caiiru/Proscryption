using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace BuzzGames.SceneDocs
{
    [CustomEditor(typeof(SceneNoteBehaviour))]
    public class SceneNoteEditor : Editor
    {
        private GUIStyle headerStyle;
        private string tempNameInput = "";

        private void OnEnable()
        {
            Tools.hidden = true;
            SceneNoteBehaviour note = (SceneNoteBehaviour)target;
            if (note != null && note.noteData != null)
            {
                tempNameInput = note.noteData.title;
                if (tempNameInput == "New Note" || tempNameInput == "Nova Nota" || tempNameInput == "GameObject")
                    tempNameInput = "";
            }
        }

        private void OnDisable() { Tools.hidden = false; }

        private void OnSceneGUI()
        {
            SceneNoteBehaviour note = (SceneNoteBehaviour)target;
            if (note.noteData == null) return;
            Event e = Event.current;
            if (e.button == 1 || e.alt) return;

            if (e.type == EventType.Repaint)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                float distance = Vector3.Cross(ray.direction, note.transform.position - ray.origin).magnitude;
                note.SetHoverState(distance < 0.8f);
            }
        }

        public override void OnInspectorGUI()
        {
            SceneNoteBehaviour behaviour = (SceneNoteBehaviour)target;
            SetupStyles();

            serializedObject.Update();
            if (behaviour.noteData == null) { EditorGUILayout.HelpBox("Dados ausentes.", MessageType.Warning); return; }

            SerializedObject dataObject = new SerializedObject(behaviour.noteData);
            dataObject.Update();

            SerializedProperty isSetupProp = dataObject.FindProperty("isSetup");
            if (!isSetupProp.boolValue) DrawSetupScreen(behaviour, dataObject);
            else DrawMainInspector(behaviour, dataObject);

            dataObject.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSetupScreen(SceneNoteBehaviour behaviour, SerializedObject dataObject)
        {
            GUILayout.Space(10);
            GUIStyle sleekTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.9f, 0.9f, 0.9f) } };
            GUIStyle sleekInput = new GUIStyle(EditorStyles.textField) { fontSize = 12, alignment = TextAnchor.MiddleLeft, fixedHeight = 22 };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var icon = EditorGUIUtility.IconContent("d_CreateAddNew");
            if (icon != null && icon.image != null) GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
            GUILayout.Label("Nova Nota Criada!", sleekTitle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Dê um nome para começar a documentar.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(10);

            GUI.SetNextControlName("NameInput");
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            tempNameInput = EditorGUILayout.TextField(tempNameInput, sleekInput);
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(tempNameInput) && Event.current.type == EventType.Repaint) GUI.FocusControl("NameInput");

            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.5f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("CRIAR NOTA", GUILayout.Height(24)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                if (string.IsNullOrWhiteSpace(tempNameInput)) tempNameInput = "Nota";
                SerializedProperty titleProp = dataObject.FindProperty("title");
                titleProp.stringValue = tempNameInput;
                behaviour.gameObject.name = tempNameInput;
                EditorUtility.SetDirty(behaviour.gameObject);
                SerializedProperty isSetupProp = dataObject.FindProperty("isSetup");
                isSetupProp.boolValue = true;
                dataObject.ApplyModifiedProperties();
                behaviour.UpdateVisuals();
                GUI.FocusControl(null);
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        private void DrawMainInspector(SceneNoteBehaviour behaviour, SerializedObject dataObject)
        {
            SceneNoteSettings settings = SceneNoteSettings.Instance;
            if (settings == null) { EditorGUILayout.HelpBox("Configurações não encontradas!", MessageType.Error); return; }

            EditorGUILayout.Space(5);
            DrawDynamicCategoryButtons(dataObject.FindProperty("noteTypeId"), behaviour, settings);
            EditorGUILayout.Space(5);

            NoteTypeConfig currentConfig = settings.GetConfig(behaviour.noteData.noteTypeId);
            if (currentConfig == null) return;

            Color catColor = currentConfig.color;
            GUI.backgroundColor = catColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📌 " + currentConfig.label.ToUpper(), headerStyle, GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(dataObject.FindProperty("title"), GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                dataObject.ApplyModifiedProperties();
                behaviour.gameObject.name = behaviour.noteData.title;
                EditorUtility.SetDirty(behaviour.gameObject);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🚦 Status:", EditorStyles.miniBoldLabel, GUILayout.Width(60));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(dataObject.FindProperty("status"), GUIContent.none);
            if (EditorGUI.EndChangeCheck()) { dataObject.ApplyModifiedProperties(); behaviour.UpdateVisuals(); SceneView.RepaintAll(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🏃 Sprint:", EditorStyles.miniBoldLabel, GUILayout.Width(60));
            EditorGUILayout.PropertyField(dataObject.FindProperty("sprint"), GUIContent.none);
            EditorGUILayout.EndHorizontal();

            // --- NOVO SISTEMA DE CARGOS DO DISCORD ---
            if (settings.discordRoles != null && settings.discordRoles.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("🔔 Notificar:", EditorStyles.miniBoldLabel, GUILayout.Width(60));

                List<string> roleNames = new List<string>() { "Ninguém" };
                int selectedIndex = 0;

                for (int i = 0; i < settings.discordRoles.Count; i++)
                {
                    string rName = settings.discordRoles[i].roleName;
                    if (string.IsNullOrEmpty(rName)) rName = "Cargo Desconhecido";

                    roleNames.Add("@" + rName);

                    if (behaviour.noteData.targetRoleId == settings.discordRoles[i].roleId)
                    {
                        selectedIndex = i + 1;
                    }
                }

                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup(selectedIndex, roleNames.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    behaviour.noteData.targetRoleId = newIndex == 0 ? "" : settings.discordRoles[newIndex - 1].roleId;
                    EditorUtility.SetDirty(behaviour.noteData);
                }
                EditorGUILayout.EndHorizontal();
            }
            // -----------------------------------------

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            DrawSectionHeader("📸 Câmera & Evidência", new Color(0.85f, 0.9f, 1f));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" Visão", EditorGUIUtility.IconContent("d_SceneViewCamera").image, "Restaurar Visão"), GUILayout.Height(28))) AlignViewToNote(behaviour.noteData);
            if (GUILayout.Button(new GUIContent(" Buscar", EditorGUIUtility.IconContent("d_ViewToolZoom").image, "Achar Objeto"), GUILayout.Height(28))) { Selection.activeGameObject = behaviour.gameObject; if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected(); }
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Refresh").image, "Atualizar Posição da Câmera"), GUILayout.Height(28), GUILayout.Width(30))) { if (EditorUtility.DisplayDialog("Atualizar?", "Salvar posição atual da câmera?", "Sim", "Não")) UpdateViewData(behaviour.noteData); }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent(" Capturar Tela", EditorGUIUtility.IconContent("Camera Gizmo").image), GUILayout.Height(24))) CaptureScreenshot(behaviour.noteData);

            if (behaviour.noteData.screenshotPaths.Count > 0)
            {
                EditorGUILayout.Space(2);
                float width = EditorGUIUtility.currentViewWidth - 50;
                float height = width * 0.55f;
                for (int i = 0; i < behaviour.noteData.screenshotPaths.Count; i++)
                {
                    string path = behaviour.noteData.screenshotPaths[i];
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex != null)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        if (GUILayout.Button(tex, GUILayout.Width(width), GUILayout.Height(height))) AssetDatabase.OpenAsset(tex);
                        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                        if (GUILayout.Button(new GUIContent(" Deletar", EditorGUIUtility.IconContent("TreeEditor.Trash").image), GUILayout.Height(20))) { if (EditorUtility.DisplayDialog("Deletar?", "Deletar Captura?", "Sim", "Não")) { AssetDatabase.DeleteAsset(path); behaviour.noteData.screenshotPaths.RemoveAt(i); EditorUtility.SetDirty(behaviour.noteData); } }
                        GUI.backgroundColor = Color.white;
                        GUILayout.EndVertical();
                    }
                    else behaviour.noteData.screenshotPaths.RemoveAt(i);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            DrawSectionHeader("📝 Descrição", new Color(1f, 1f, 0.85f));
            SerializedProperty contentProp = dataObject.FindProperty("content");
            contentProp.stringValue = EditorGUILayout.TextArea(contentProp.stringValue, EditorStyles.textArea, GUILayout.MinHeight(60));
            EditorGUILayout.LabelField($"📅 {behaviour.noteData.createdDate} por {behaviour.noteData.author}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();

            // INTEGRAÇÕES
            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.35f, 0.4f, 0.85f);
            if (GUILayout.Button(new GUIContent(" Notificar Discord", EditorGUIUtility.IconContent("d_BuildSettings.Android.Small").image), GUILayout.Height(32)))
            {
                NotionSync.SendDiscordWebhook(behaviour.noteData);
                if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Enviado pro Discord!"));
            }

            string notionIcon = string.IsNullOrEmpty(behaviour.noteData.notionPageId) ? "d_BuildSettings.Web.Small" : "d_CloudConnect";
            string notionText = string.IsNullOrEmpty(behaviour.noteData.notionPageId) ? " Enviar p/ Notion" : " Atualizar Notion";

            GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
            if (GUILayout.Button(new GUIContent(notionText, EditorGUIUtility.IconContent(notionIcon).image), GUILayout.Height(32)))
            {
                NotionSync.SyncNoteToNotion(behaviour.noteData);
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDynamicCategoryButtons(SerializedProperty prop, SceneNoteBehaviour b, SceneNoteSettings settings)
        {
            GUILayout.BeginHorizontal();
            foreach (var typeConfig in settings.noteTypes)
            {
                bool isSelected = prop.stringValue == typeConfig.id;

                if (isSelected)
                {
                    GUI.backgroundColor = typeConfig.color;
                    GUI.contentColor = Color.white;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                    GUI.contentColor = typeConfig.color;
                }

                Texture icon = null;
                if (typeConfig.iconSprite != null) icon = typeConfig.iconSprite.texture;
                else
                {
                    icon = EditorGUIUtility.IconContent(typeConfig.iconName).image;
                    if (icon == null) icon = EditorGUIUtility.IconContent("d_FilterSelectedOnly").image;
                }

                if (GUILayout.Button(new GUIContent(icon, typeConfig.label), GUILayout.Width(35), GUILayout.Height(30)))
                {
                    prop.stringValue = typeConfig.id;
                    prop.serializedObject.ApplyModifiedProperties();
                    b.UpdateVisuals();
                    SceneView.RepaintAll();
                }

                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSectionHeader(string title, Color bgColor)
        {
            GUI.backgroundColor = bgColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private void AlignViewToNote(SceneNoteData data)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(data.cameraPosition, data.cameraRotation, data.cameraSize, data.isOrthographic);
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        private void UpdateViewData(SceneNoteData data)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                data.cameraPosition = SceneView.lastActiveSceneView.pivot;
                data.cameraRotation = SceneView.lastActiveSceneView.rotation;
                data.cameraSize = SceneView.lastActiveSceneView.size;
                data.isOrthographic = SceneView.lastActiveSceneView.orthographic;
                EditorUtility.SetDirty(data);
            }
        }

        private void CaptureScreenshot(SceneNoteData data)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            string folder = "Assets/BuzzGames/SceneDocs/Screenshots";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string file = $"Screen_{data.name}_{System.DateTime.Now.Ticks}.png";
            string path = Path.Combine(folder, file);

            Camera cam = view.camera;
            int w = (int)view.position.width * 2;
            int h = (int)view.position.height * 2;

            RenderTexture rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D shot = new Texture2D(w, h, TextureFormat.RGB24, false);
            shot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            shot.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);

            File.WriteAllBytes(path, shot.EncodeToPNG());
            DestroyImmediate(shot);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            data.screenshotPaths.Add(path);
            EditorUtility.SetDirty(data);
        }

        private void SetupStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 13;
                headerStyle.normal.textColor = Color.black;
                headerStyle.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}