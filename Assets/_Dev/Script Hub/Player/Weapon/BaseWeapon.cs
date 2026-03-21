using UnityEngine;

namespace proscryption
{
    public class BaseWeapon : MonoBehaviour
    {
        public int minDamage = 5;
        public int maxDamage = 10;
        public int critChance = 10;
        private bool isAttacking = false;
        private bool canHit = false;
        private Collider _collider;
        void Start()
        {
            _collider = GetComponent<Collider>();

            if (minDamage > maxDamage)
            {
                int temp = minDamage;
                minDamage = maxDamage;
                maxDamage = temp;
            }
        }
        public void OnAttack()
        {
            isAttacking = true;
        }
        public void SetCanHitState(bool newState)
        {
            canHit = newState;
        }
        public void StopAttacking()
        {
            isAttacking = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!canHit) return;
            if (!isAttacking) return;
            if (other.CompareTag("Enemy"))
            {
                canHit = false;
                other.gameObject.GetComponent<BaseEntity>().TakeDamage(CalculateDamage());
            }
        }

        int CalculateDamage()
        {
            int damage = Random.Range(minDamage,maxDamage);
            if (Random.Range(0, 100) <= critChance)
            {
                damage *= 2;
            }

            return damage;
        }
    }

}
