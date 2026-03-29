using UnityEngine;
using System.Collections.Generic;

namespace BuzzGames.SceneDocs
{
    [CreateAssetMenu(fileName = "NewSceneNote", menuName = "SceneDocs/Scene Note Data")]
    public class SceneNoteData : ScriptableObject
    {
        public bool isSetup = false;

        public string title;
        [TextArea(3, 10)] public string content;
        public string author;
        public string createdDate;
        public string sprint = "Sprint 1";
        public string noteTypeId = "todo";

        // --- NOVA VARIÁVEL PARA O CARGO ---
        public string targetRoleId = "";
        // ----------------------------------

        public NoteStatus status;
        public Vector3 position;
        public string sceneName;

        [Header("View Settings")]
        public Vector3 cameraPosition;
        public Quaternion cameraRotation;
        public float cameraSize;
        public bool isOrthographic;

        [HideInInspector] public string notionPageId;

        [Header("Attachments")]
        public List<string> screenshotPaths = new List<string>();

        [Header("Discussion")]
        public List<string> comments = new List<string>();
    }

    public enum NoteStatus { Open, Resolved, WontFix }
}