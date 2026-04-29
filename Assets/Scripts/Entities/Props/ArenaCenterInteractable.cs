using DG.Tweening;
using UnityEngine;

namespace proscryption
{
    public class ArenaCenterInteractable : MonoBehaviour, IInteractable
    {
        public string InteractableName = "Ritual";
        public bool canInteract = true;

        SphereCollider _collider;

        public int currentWave;

        float _baseRadius = 0f;
        [Header("Ritual Transforms")]
        public Transform RitualBaseTransform;
        public Transform RitualInteriorTransform;
        public Transform RitualOutsideTransform;

        void Start()
        {
            this.transform.name = InteractableName;
            _collider = this.GetComponent<SphereCollider>();
            _baseRadius = _collider.radius;

            SetupEvents();
        }
        void SetupEvents()
        {
            ArenaEvents.OnArenaWaveEnded += HandleWaveEnded;
        }
        void OnDestroy()
        {
            ArenaEvents.OnArenaWaveEnded -= HandleWaveEnded;
        }
        void HandleWaveEnded()
        {
            canInteract = true;
            _collider.radius = _baseRadius;
        }
        public string GetInteractName()
        {
            return InteractableName;
        }

        public void Interact()
        {
            if (!canInteract)
            {
                return;
            }
            HandleNewWave();
        }
        private void HandleNewWave()
        {

            ArenaEvents.BroadcastArenaStart();
            canInteract = false;
            _collider.radius = 0;
            currentWave++;

            switch (currentWave)
            {
                case 1:
                    RitualBaseTransform.DOScale(.5f, 1.2f).SetEase(Ease.OutSine);
                    break;
                case 2:
                    RitualInteriorTransform.GetComponent<MeshRenderer>().enabled = true;

                    break;
                case 3:
                    RitualOutsideTransform.GetComponent<MeshRenderer>().enabled = true;
                    break;
                case 4:
                    break;
                default:
                    break;
            }
        }

        public bool CanInteract()
        {
            return canInteract;
        }
    }
}
