using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace BuzzGames.SceneDocs
{
    public class NotionSync : EditorWindow
    {
        private const string API_URL_PAGES = "https://api.notion.com/v1/pages";
        private static Queue<SceneNoteData> uploadQueue = new Queue<SceneNoteData>();
        private static bool isUploadingBatch = false;

        [MenuItem("Tools/SceneDocs/Integrations Setup", false, 50)]
        public static void ShowWindow() => GetWindow<NotionSync>("SceneDocs Setup");

        private void OnGUI()
        {
            GUILayout.Label("🔌 Configuração SceneDocs", EditorStyles.boldLabel);
            SceneNoteSettings settings = SceneNoteSettings.Instance;
            if (settings == null) { if (GUILayout.Button("Criar Ficheiro de Configuração")) CreateSettingsAsset(); return; }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Notion", EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("API Key:");
            settings.notionApiKey = EditorGUILayout.TextField(settings.notionApiKey);
            GUILayout.Label("Database ID:");
            settings.databaseId = EditorGUILayout.TextField(settings.databaseId);
            EditorGUILayout.HelpBox("1. Crie Integração (notion.so/my-integrations).\n2. Partilhe o seu Database com a integração.\n3. Copie o ID do Database do URL.", MessageType.Info);
            if (GUILayout.Button("Testar Conexão Notion")) Debug.Log($"Key: {settings.notionApiKey} | DB: {settings.databaseId}");
            GUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Discord", EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Webhook URL:");
            settings.discordWebhookUrl = EditorGUILayout.TextField(settings.discordWebhookUrl);
            EditorGUILayout.HelpBox("Vá ao Canal do Discord > Editar Canal > Integrações > Webhooks.", MessageType.Info);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets(); }
        }

        private static SceneNoteSettings CreateSettingsAsset()
        {
            string path = "Assets/BuzzGames/SceneDocs/Resources";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var instance = CreateInstance<SceneNoteSettings>();
            AssetDatabase.CreateAsset(instance, $"{path}/SceneNoteSettings.asset");
            AssetDatabase.SaveAssets();
            return instance;
        }

        public static void SendDiscordWebhook(SceneNoteData note)
        {
            SceneNoteSettings settings = SceneNoteSettings.Instance;
            if (settings == null || string.IsNullOrEmpty(settings.discordWebhookUrl)) { Debug.LogError("❌ Discord Webhook em falta."); return; }

            Color catColor = Color.white;
            string catLabel = note.noteTypeId;

            var config = settings.GetConfig(note.noteTypeId);
            if (config != null) { catColor = config.color; catLabel = config.label; }

            int colorInt = ((int)(catColor.r * 255) << 16) + ((int)(catColor.g * 255) << 8) + (int)(catColor.b * 255);

            // Se a nota tiver um Cargo selecionado, nós injetamos o código de "Ping" na variável content do Webhook
            string contentPing = "";
            if (!string.IsNullOrEmpty(note.targetRoleId))
            {
                contentPing = $"\"content\": \"<@&{note.targetRoleId}>\",";
            }

            string json = $@"
            {{
                ""username"": ""SceneDocs Bot"",
                {contentPing}
                ""embeds"": [
                    {{
                        ""title"": ""{catLabel.ToUpper()}: {Sanitize(note.title)}"",
                        ""description"": ""{Sanitize(note.content)}"",
                        ""color"": {colorInt},
                        ""fields"": [
                            {{ ""name"": ""Status"", ""value"": ""{note.status}"", ""inline"": true }},
                            {{ ""name"": ""Autor"", ""value"": ""{note.author}"", ""inline"": true }},
                            {{ ""name"": ""Cena"", ""value"": ""{note.sceneName}"", ""inline"": true }}
                        ],
                        ""footer"": {{ ""text"": ""SceneDocs • {System.DateTime.Now}"" }}
                    }}
                ]
            }}";
            SendRequestRaw(settings.discordWebhookUrl, json, "Discord");
        }

        public static void SyncNoteToNotion(SceneNoteData note, bool isBatch = false)
        {
            SceneNoteSettings settings = SceneNoteSettings.Instance;
            if (settings == null || string.IsNullOrEmpty(settings.notionApiKey) || string.IsNullOrEmpty(settings.databaseId)) return;

            string dateISO = System.DateTime.Now.ToString("yyyy-MM-dd");
            bool isUpdate = !string.IsNullOrEmpty(note.notionPageId);
            string url = isUpdate ? $"{API_URL_PAGES}/{note.notionPageId}" : API_URL_PAGES;
            string method = isUpdate ? "PATCH" : "POST";

            string catLabel = note.noteTypeId;
            var config = settings.GetConfig(note.noteTypeId);
            if (config != null) catLabel = config.label;

            // Busca o nome do cargo para escrever no Notion
            string targetRoleName = "";
            if (!string.IsNullOrEmpty(note.targetRoleId) && settings.discordRoles != null)
            {
                var roleInfo = settings.discordRoles.Find(r => r.roleId == note.targetRoleId);
                if (roleInfo != null) targetRoleName = roleInfo.roleName;
            }

            // Anexa a tag de marcação na primeira linha da descrição
            string descPrefix = string.IsNullOrEmpty(targetRoleName) ? "" : $"[Atenção: @{targetRoleName}]\\n\\n";
            string finalDescription = descPrefix + Sanitize(note.content);

            string jsonBody = $@"
                {{
                    {(isUpdate ? "" : $"\"parent\": {{ \"database_id\": \"{settings.databaseId}\" }},")}
                    ""properties"": {{
                        ""Name"": {{ ""title"": [ {{ ""text"": {{ ""content"": ""{Sanitize(note.title)}"" }} }} ] }},
                        ""Category"": {{ ""select"": {{ ""name"": ""{catLabel}"" }} }},
                        ""Status"": {{ ""select"": {{ ""name"": ""{note.status}"" }} }},
                        ""Sprint"": {{ ""rich_text"": [ {{ ""text"": {{ ""content"": ""{Sanitize(note.sprint)}"" }} }} ] }},
                        ""Date"": {{ ""date"": {{ ""start"": ""{dateISO}"" }} }},
                        ""Author"": {{ ""rich_text"": [ {{ ""text"": {{ ""content"": ""{Sanitize(note.author)}"" }} }} ] }},
                        ""Description"": {{ ""rich_text"": [ {{ ""text"": {{ ""content"": ""{finalDescription}"" }} }} ] }}
                    }}
                }}";

            SendRequestNotion(url, method, jsonBody, settings.notionApiKey, note, isBatch);
        }

        private static void SendRequestRaw(string url, string json, string serviceName)
        {
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            var operation = request.SendWebRequest();
            operation.completed += (op) => { if (request.result == UnityWebRequest.Result.Success) Debug.Log($"✅ {serviceName} Enviado!"); else Debug.LogError($"❌ {serviceName}: {request.error}"); request.Dispose(); };
        }

        private static void SendRequestNotion(string url, string method, string json, string apiKey, SceneNoteData note, bool isBatch)
        {
            var request = new UnityWebRequest(url, method);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Notion-Version", "2022-06-28");
            request.SetRequestHeader("Content-Type", "application/json");
            var operation = request.SendWebRequest();
            operation.completed += (op) =>
            {
                if (request.result == UnityWebRequest.Result.Success && method == "POST")
                {
                    string newId = ExtractIdFromJson(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(newId)) { note.notionPageId = newId; EditorUtility.SetDirty(note); AssetDatabase.SaveAssets(); }
                    Debug.Log($"✅ Notion: {note.title} salvo!");
                }
                else if (request.result != UnityWebRequest.Result.Success) Debug.LogError($"❌ Erro Notion: {request.error}");
                request.Dispose();
                if (isBatch) ProcessNextInQueue();
            };
        }

        public static void SyncBatch(List<SceneNoteBehaviour> notes) { uploadQueue.Clear(); foreach (var n in notes) if (n != null && n.noteData != null) uploadQueue.Enqueue(n.noteData); if (uploadQueue.Count > 0 && !isUploadingBatch) { isUploadingBatch = true; EditorUtility.DisplayProgressBar("Sincronizando", "A enviar...", 0f); ProcessNextInQueue(); } }
        private static void ProcessNextInQueue() { if (uploadQueue.Count > 0) { var note = uploadQueue.Dequeue(); float p = 1f - ((float)uploadQueue.Count / 100f); EditorUtility.DisplayProgressBar("Sincronizando", $"A enviar: {note.title}", p); SyncNoteToNotion(note, true); } else { isUploadingBatch = false; EditorUtility.ClearProgressBar(); Debug.Log("Sincronização em Lote Concluída!"); } }
        private static string ExtractIdFromJson(string json) { int i = json.IndexOf("\"id\":"); if (i == -1) return ""; int s = json.IndexOf("\"", i + 5) + 1; int e = json.IndexOf("\"", s); return json.Substring(s, e - s); }
        private static string Sanitize(string input) { if (string.IsNullOrEmpty(input)) return ""; return input.Replace("\"", "'").Replace("\n", "\\n").Replace("\r", ""); }
    }
}