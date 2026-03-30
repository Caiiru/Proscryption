using UnityEngine;
using UnityEditor;
using System.IO;

namespace BuzzGames.SceneDocs
{
    [InitializeOnLoad]
    public class SceneDocsWizard : EditorWindow
    {
        private Texture2D logoTexture;

        static SceneDocsWizard()
        {
            EditorApplication.delayCall += CheckFirstRun;
        }

        private static void CheckFirstRun()
        {
            if (!EditorPrefs.GetBool("SceneDocs_Installed", false))
            {
                ShowWindow();
            }
        }

        [MenuItem("Tools/SceneDocs/Re-run Setup Wizard", false, 200)]
        public static void ShowWindow()
        {
            SceneDocsWizard window = GetWindow<SceneDocsWizard>(true, "📘 SceneDocs Setup", true);
            window.minSize = new Vector2(480, 420); // Aumentei a altura para caber o diagnóstico
            window.maxSize = new Vector2(480, 420);
            window.Show();
        }

        private void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("SceneDocsLogo t:Texture2D");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }

        private void OnGUI()
        {
            // --- HEADER E BANNER ---
            if (logoTexture != null)
            {
                float bannerWidth = position.width;
                float bannerHeight = 120f;
                Rect bannerRect = new Rect(0, 0, bannerWidth, bannerHeight);
                GUI.DrawTexture(bannerRect, logoTexture, ScaleMode.ScaleAndCrop);
                GUILayout.Space(bannerHeight + 15);
            }
            else
            {
                GUILayout.Space(15);
                GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                GUILayout.Label("📘 Bem-vindo ao SceneDocs!", titleStyle);
            }

            GUILayout.Label("Ferramenta de Documentação Interna da Equipa", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // --- PAINEL DE DIAGNÓSTICO ---
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            GUILayout.Label("🔍 Verificação de Integrações (Git)", EditorStyles.boldLabel);
            GUILayout.Space(10);

            SceneNoteSettings settings = SceneNoteSettings.Instance;

            bool hasNotionKey = settings != null && !string.IsNullOrEmpty(settings.notionApiKey);
            bool hasNotionDb = settings != null && !string.IsNullOrEmpty(settings.databaseId);
            bool hasDiscord = settings != null && !string.IsNullOrEmpty(settings.discordWebhookUrl);

            DrawStatusRow("Notion API Key", hasNotionKey);
            DrawStatusRow("Notion Database ID", hasNotionDb);
            DrawStatusRow("Discord Webhook", hasDiscord);

            GUILayout.Space(10);

            if (!hasNotionKey || !hasNotionDb || !hasDiscord)
            {
                EditorGUILayout.HelpBox("Faltam algumas chaves! Se o repositório não puxou a configuração, peça as chaves ao administrador ou preencha manualmente.", MessageType.Warning);

                GUI.backgroundColor = new Color(0.7f, 0.8f, 1f);
                if (GUILayout.Button("Abrir Arquivo de Configuração", GUILayout.Height(24)))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox("Tudo verde! O SceneDocs puxou as configurações do projeto e está ligado ao Notion e Discord da equipe.", MessageType.Info);
            }

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            GUILayout.Space(15);

            // --- BOTÃO FINAL ---
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.5f);
            if (GUILayout.Button("Inicializar Workspace ✔", GUILayout.Height(40)))
            {
                FinalizeSetup(settings);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
        }

        private void DrawStatusRow(string label, bool isOk)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(label, GUILayout.Width(150));
            if (isOk)
            {
                GUI.contentColor = new Color(0.1f, 0.7f, 0.1f);
                GUILayout.Label("✔ Preenchido", EditorStyles.boldLabel);
            }
            else
            {
                GUI.contentColor = new Color(0.8f, 0.2f, 0.2f);
                GUILayout.Label("✖ Faltando", EditorStyles.boldLabel);
            }
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }

        private void FinalizeSetup(SceneNoteSettings settings)
        {
            string path = "Assets/BuzzGames/SceneDocs/Resources";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            if (settings == null)
            {
                settings = CreateInstance<SceneNoteSettings>();
                AssetDatabase.CreateAsset(settings, $"{path}/SceneNoteSettings.asset");
            }

            settings.EnsureDefaults();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            EditorPrefs.SetBool("SceneDocs_Installed", true);
            Debug.Log("📘 SceneDocs: Inicializado com Sucesso!");
            this.Close();
        }
    }
}