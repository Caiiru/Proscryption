using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace proscryption
{
    public class IBookUpgrade : MonoBehaviour, IInteractable
    {
        public string interactableName = "Livros de poder";

        public bool waveEnded;

        public Vector3 bookSpawnPosition;

        public AnimationCurve animationCurve;

        public float startY = -0.5f;

        public void Start()
        {
            SetupEvents();
            bookSpawnPosition = transform.position;
            DisableBook();
        }

        void SetupEvents()
        {
            ArenaEvents.OnArenaWaveEnded += HandleWaveEnded;
        }

        void OnDestroy()
        {
            ArenaEvents.OnArenaWaveEnded -= HandleWaveEnded;
        }

        void Update()
        {
            if (waveEnded)
            {
                HandleWaveEnded();
                waveEnded = false;
            }
        }

        private void HandleWaveEnded()
        {
            waveEnded = true;
            transform.DOMove(bookSpawnPosition, animationCurve.length).SetEase(animationCurve);
        }

        public void Interact()
        {
            EventManager.BroadcastBookInteract();
            waveEnded = false;
            DisableBook();
        }

        public string GetInteractName()
        {
            return interactableName;
        }

        public bool CanInteract()
        {
            return waveEnded;
        }

        private void DisableBook()
        {
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + bookSpawnPosition, 0.1f);
        }
    }
}