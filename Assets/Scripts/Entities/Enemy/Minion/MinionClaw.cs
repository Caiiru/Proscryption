using Cysharp.Threading.Tasks;
using UnityEngine;

namespace proscryption
{
    public class MinionClaw : MonoBehaviour
    {
        private MinionController _controller;
        public bool alreadyHit;
        public float hitCooldown;

        public void Setup(MinionController controller)
        {
            this._controller = controller;
            alreadyHit = false;
        }
        void OnTriggerEnter(Collider other)
        {
            if (alreadyHit) return;

            if (other.TryGetComponent(out PlayerModel model))
            {
                alreadyHit = true;
                int _damage = _controller.GetAttackDamage();
                if (_controller.IsAttackCritical())
                {
                    _damage *= 2;
                }
                EventManager.BroadcastHitDetected(transform.position, _damage, model.gameObject);

                Invoke("ResetHit", 0.01f);
            }
        }

        async UniTask ResetHit()
        {
            int _delay = Mathf.FloorToInt(hitCooldown * 1000);
            await UniTask.Delay(_delay);
            alreadyHit = false;
        }
    }
}
