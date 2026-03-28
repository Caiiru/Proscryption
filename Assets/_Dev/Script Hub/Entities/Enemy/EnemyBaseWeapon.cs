using UnityEngine;

namespace proscryption
{
    public class EnemyBaseWeapon : MonoBehaviour
    {
        private EnemyEntity _owner;

        public void SetOwner(EnemyEntity owner)
        {
            this._owner = owner;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_owner.GetCanHit()) return;
            if (other.CompareTag("Player"))
            {
                other.TryGetComponent<PlayerModel>(out PlayerModel playerEntity);

                if (!playerEntity) return;

                EventManager.BroadcastHitDetected(other.transform.position, _owner.GetDamage(), other.gameObject);
                // _owner.GetDamage(), _owner.gameObject);

            }
        }
    }
}
