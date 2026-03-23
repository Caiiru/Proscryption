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
                other.TryGetComponent<BaseEntity>(out BaseEntity playerEntity);

                if (!playerEntity) return;

                playerEntity.TakeDamage(_owner.GetDamage());

            }
        }
    }
}
