using UnityEngine;
using UnityEditor;
using System.IO;

namespace BuzzGames.SceneDocs
{
    public class SceneNoteMenus : Editor
    {
        [MenuItem("GameObject/SceneDocs/Nova Nota/🟨 A Fazer", false, 0)]
        static void AddTodo(MenuCommand cmd) => CreateNoteLogic(cmd, "todo");
        [MenuItem("GameObject/SceneDocs/Nova Nota/🟥 Bug", false, 0)]
        static void AddBug(MenuCommand cmd) => CreateNoteLogic(cmd, "bug");
        [MenuItem("GameObject/SceneDocs/Nova Nota/🟦 Design", false, 0)]
        static void AddDesign(MenuCommand cmd) => CreateNoteLogic(cmd, "design");
        [MenuItem("GameObject/SceneDocs/Nova Nota/🟩 Código", false, 0)]
        static void AddCode(MenuCommand cmd) => CreateNoteLogic(cmd, "code");
        [MenuItem("GameObject/SceneDocs/Nova Nota/🟪 Arte", false, 0)]
        static void AddArt(MenuCommand cmd) => CreateNoteLogic(cmd, "art");
        [MenuItem("GameObject/SceneDocs/Nova Nota/💗 Elogio", false, 0)]
        static void AddPraise(MenuCommand cmd) => CreateNoteLogic(cmd, "praise");

        public static void CreateNoteLogic(MenuCommand menuCommand, string typeId)
        {
            GameObject selectedObj = menuCommand.context as GameObject;
            if (selectedObj == null) selectedObj = Selection.activeGameObject;

            GameObject noteObject = null;
            string initialTitle = "";

            if (selectedObj == null)
            {
                noteObject = new GameObject("Nova Nota");
                if (SceneView.lastActiveSceneView != null)
                    noteObject.transform.position = SceneView.lastActiveSceneView.pivot;
                initialTitle = "Nova Nota";
            }
            else
            {
                int existingNotesCount = 0;
                foreach (Transform child in selectedObj.transform)
                {
                    if (child.GetComponent<SceneNoteBehaviour>() != null)
                        existingNotesCount++;
                }

                noteObject = new GameObject("Nova Nota");
                Undo.RegisterCreatedObjectUndo(noteObject, "Criar SceneDoc");
                noteObject.transform.SetParent(selectedObj.transform);

                float stackOffset = existingNotesCount * 0.8f;

                Renderer r = selectedObj.GetComponent<Renderer>();
                if (r != null)
                    noteObject.transform.position = r.bounds.center + Vector3.up * (r.bounds.extents.y + 0.5f + stackOffset);
                else
                    noteObject.transform.localPosition = Vector3.up * (1.0f + stackOffset);

                initialTitle = selectedObj.name;
            }

            noteObject.tag = "EditorOnly";

            SceneNoteBehaviour behaviour = Undo.AddComponent<SceneNoteBehaviour>(noteObject);

            SceneNoteSettings settings = SceneNoteSettings.Instance;
            string iconName = "Knob";
            if (settings != null)
            {
                var config = settings.GetConfig(typeId);
                if (config != null) iconName = config.iconName;
            }

            SpriteRenderer sr = noteObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                string[] guids = AssetDatabase.FindAssets($"{iconName} t:Sprite");
                if (guids.Length > 0) sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
                else sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            }

            SceneNoteData newData = CreateNoteAsset(initialTitle, typeId);

            if (SceneView.lastActiveSceneView != null)
            {
                newData.cameraPosition = SceneView.lastActiveSceneView.pivot;
                newData.cameraRotation = SceneView.lastActiveSceneView.rotation;
                newData.isOrthographic = SceneView.lastActiveSceneView.orthographic;
                newData.cameraSize = SceneView.lastActiveSceneView.size;
            }

            behaviour.noteData = newData;
            behaviour.noteData.position = noteObject.transform.position;
            behaviour.UpdateVisuals();
            EditorUtility.SetDirty(noteObject);
            Selection.activeGameObject = noteObject;
        }

        static SceneNoteData CreateNoteAsset(string titleBase, string typeId)
        {
            SceneNoteData newData = ScriptableObject.CreateInstance<SceneNoteData>();
            newData.title = titleBase;
            newData.createdDate = System.DateTime.Now.ToString();
            newData.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            newData.noteTypeId = typeId;

            string unityUser = CloudProjectSettings.userName;
            newData.author = !string.IsNullOrEmpty(unityUser) ? unityUser : System.Environment.UserName;

            string folderPath = "Assets/BuzzGames/SceneDocs/Data";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = $"Nota_{titleBase}_{System.DateTime.Now.Ticks}.asset";
            string fullPath = Path.Combine(folderPath, fileName);

            AssetDatabase.CreateAsset(newData, fullPath);
            AssetDatabase.SaveAssets();
            return newData;
        }

        public static void CreateNoteFromOverlay(string typeId)
        {
            CreateNoteLogic(new MenuCommand(Selection.activeGameObject), typeId);
        }
    }
}