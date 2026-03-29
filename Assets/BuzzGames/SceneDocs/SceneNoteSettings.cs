using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BuzzGames.SceneDocs
{
    [System.Serializable]
    public class NoteTypeConfig
    {
        public string id;
        public string label;
        public Color color = Color.white;
        public Sprite iconSprite;
        public string iconName;

        public NoteTypeConfig(string id, string label, Color color, string icon, Sprite sprite = null)
        {
            this.id = id;
            this.label = label;
            this.color = color;
            this.iconName = icon;
            this.iconSprite = sprite;
        }
    }

    // --- NOVA CLASSE PARA OS CARGOS ---
    [System.Serializable]
    public class DiscordRoleConfig
    {
        public string roleName; // Ex: Programadores
        public string roleId;   // Ex: 123456789012345678
    }
    // ----------------------------------

    public class SceneNoteSettings : ScriptableObject
    {
        [Header("Configurações de Nuvem (Commitar no Git)")]
        public string notionApiKey = "";
        public string databaseId = "";
        public string discordWebhookUrl = "";

        // --- LISTA DE CARGOS NO INSPECTOR ---
        [Header("Cargos para Notificação")]
        public List<DiscordRoleConfig> discordRoles = new List<DiscordRoleConfig>();
        // ------------------------------------

        [HideInInspector]
        public List<NoteTypeConfig> noteTypes = new List<NoteTypeConfig>();

        private static SceneNoteSettings _instance;
        public static SceneNoteSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SceneNoteSettings>("SceneNoteSettings");
                    if (_instance != null) _instance.EnsureDefaults();
                }
                return _instance;
            }
        }

        public NoteTypeConfig GetConfig(string id)
        {
            EnsureDefaults();
            var config = noteTypes.Find(x => x.id == id);
            return config != null ? config : noteTypes[0];
        }

        public void EnsureDefaults()
        {
            if (noteTypes.Count >= 6) return;

            noteTypes.Clear();
            noteTypes.Add(CreateDefault("todo", "A Fazer", new Color(1f, 0.8f, 0.2f), "d_FilterSelectedOnly", "Todo"));
            noteTypes.Add(CreateDefault("bug", "Bug", new Color(1f, 0.4f, 0.4f), "d_console.erroricon.sml", "Bug"));
            noteTypes.Add(CreateDefault("design", "Design", new Color(0.4f, 0.7f, 1f), "d_SceneViewTools", "Design"));
            noteTypes.Add(CreateDefault("art", "Arte", new Color(0.7f, 0.5f, 1f), "d_Material Icon", "Art"));
            noteTypes.Add(CreateDefault("code", "Código", new Color(0.4f, 0.9f, 0.4f), "cs Script Icon", "Code"));
            noteTypes.Add(CreateDefault("praise", "Elogio", new Color(1f, 0.4f, 0.8f), "d_Favorite", "Praise"));
        }

        private NoteTypeConfig CreateDefault(string id, string label, Color color, string internalIcon, string spriteName)
        {
            Sprite loadedSprite = null;
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Replace(" ", "").Contains("SceneDocs"))
                {
                    loadedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    break;
                }
            }
#endif
            return new NoteTypeConfig(id, label, color, internalIcon, loadedSprite);
        }
    }
}