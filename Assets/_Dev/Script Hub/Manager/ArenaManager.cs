using UnityEngine;

namespace proscryption
{
    public class ArenaManager : MonoBehaviour
    {
        public static ArenaManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        void OnEnable()
        {
            EventManager.OnArenaStart += StartArena;
        }

        void OnDisable()
        {
            EventManager.OnArenaStart -= StartArena;
        }

        public void StartArena()
        {
            Debug.Log("[Arena Manager] Arena started!");
        }
    }
}
