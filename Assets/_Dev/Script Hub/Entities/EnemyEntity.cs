using UnityEngine;

namespace proscryption
{
    public class EnemyEntity : BaseEntity
    {
        [Header("Attack Settings")]

        
        public bool canAttack = false;
        public int attackTime = 6;
        private float currentAttackTime;
        public int minDamage = 3;
        public int maxDamage = 8;
        public EnemyBaseWeapon currentWeapon;
        private bool canHit; 



        public override void Start()
        {
            base.Start();
            if (currentWeapon)
            {
                currentWeapon.SetOwner(this);
            }

        }

        public void Update()
        {
            HandleAttackTimer();
        }
        private void HandleAttackTimer()
        {
            if (!canAttack) return;
            if (currentAttackTime < attackTime)
            {
                currentAttackTime += Time.deltaTime;
            }
            else
            {

                currentAttackTime = 0;
                Attack();
            }
        }

        private void Attack()
        {
            _animator.SetTrigger("Attack");
        }

        public int GetDamage()
        {
            return Random.Range(minDamage, maxDamage);
        }
        public bool GetCanHit()
        {
            return canHit; 
        }
        public void MakeCanHit()
        {
            canHit = true;
        }
        public void MakeCantHit()
        {
            canHit = false;
        }
    }
}
