using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BuzzGames.SceneDocs
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SceneNoteBehaviour : MonoBehaviour
    {
        public SceneNoteData noteData;

        [Header("Visual Settings")]
        public float iconScale = 0.5f;
        public float textHeight = 1.0f;

        [Range(0f, 1f)] public float idleAlpha = 0.7f;
        [Range(0f, 1f)] public float hoverAlpha = 1.0f;

        private SpriteRenderer spriteRenderer;
        private bool isHovered = false;

        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (gameObject.tag != "EditorOnly") gameObject.tag = "EditorOnly";
            UpdateVisuals();
        }

        private void OnValidate() { UpdateVisuals(); }

        private void Update()
        {
            if (Application.isPlaying)
            {
                if (spriteRenderer != null && spriteRenderer.enabled) spriteRenderer.enabled = false;
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.SceneView.lastActiveSceneView != null)
            {
                var camTrans = UnityEditor.SceneView.lastActiveSceneView.camera.transform;
                if (transform.rotation != camTrans.rotation) transform.rotation = camTrans.rotation;
            }
#endif
        }

        public void SetHoverState(bool state)
        {
            if (isHovered != state) { isHovered = state; UpdateVisuals(); }
        }

        public void UpdateVisuals()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (noteData == null) { spriteRenderer.enabled = false; return; }
            if (Application.isPlaying) { spriteRenderer.enabled = false; return; }

#if UNITY_EDITOR
            var settings = SceneNoteSettings.Instance; // Singleton Cache
            if (settings == null) return;

            var config = settings.GetConfig(noteData.noteTypeId); // Busca otimizada
            if (config == null) return;

            bool isMasterOn = SessionState.GetBool("SceneNotes_Master_Visible", true);
            if (!isMasterOn) { spriteRenderer.enabled = false; return; }

            string filterKey = $"SceneNote_Filter_{config.id}";
            bool isVisible = SessionState.GetBool(filterKey, true);
            if (!isVisible) { spriteRenderer.enabled = false; return; }

            spriteRenderer.enabled = true;

            if (config.iconSprite != null)
            {
                spriteRenderer.sprite = config.iconSprite;
            }
            else
            {
                // Fallbacks de ícone
                Sprite customIcon = null;
                string[] guids = AssetDatabase.FindAssets($"{config.iconName} t:Sprite");
                if (guids.Length > 0) customIcon = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));

                if (customIcon == null)
                {
                    string[] fallback = AssetDatabase.FindAssets("Knob t:Sprite");
                    if (fallback.Length > 0) customIcon = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(fallback[0]));
                    else customIcon = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                }
                spriteRenderer.sprite = customIcon;
            }

            Color finalColor = config.color;
            finalColor.a = isHovered ? hoverAlpha : idleAlpha;
            spriteRenderer.color = finalColor;
            transform.localScale = Vector3.one * iconScale;

            string newName = string.IsNullOrEmpty(noteData.title) ? "Note" : noteData.title;
            if (gameObject.name != newName)
            {
                gameObject.name = newName;
                EditorUtility.SetDirty(gameObject);
            }
#endif
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (noteData == null) return;
            var settings = SceneNoteSettings.Instance;
            if (settings == null) return;

            var config = settings.GetConfig(noteData.noteTypeId);
            if (config == null) return;

            if (!SessionState.GetBool("SceneNotes_Master_Visible", true)) return;
            if (!SessionState.GetBool($"SceneNote_Filter_{config.id}", true)) return;

            bool showDetails = isHovered || Selection.activeGameObject == gameObject;

            if (transform.parent != null)
            {
                Color c = config.color;
                Handles.color = new Color(c.r, c.g, c.b, 0.5f);
                if (Vector3.Distance(transform.position, transform.parent.position) > 0.5f)
                {
                    Handles.DrawDottedLine(transform.position, transform.parent.position, 2.0f);
                    Handles.SphereHandleCap(0, transform.parent.position, Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }

            if (!showDetails) return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.UpperCenter;

            GUIStyle shadow = new GUIStyle(style);
            shadow.normal.textColor = Color.black;

            Vector3 textPos = transform.position + (transform.up * textHeight);
            Handles.Label(textPos + new Vector3(0.02f, -0.02f, 0), noteData.title, shadow);
            Handles.Label(textPos, noteData.title, style);
#endif
        }
    }
}