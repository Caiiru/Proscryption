using UnityEngine;
using proscryption.Enemy;

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
        public EnemyBaseWeapon[] weapons;
        private bool canHit;

        // State Machine
        private EnemyController _enemyController;

        public override void Start()
        {
            base.Start();

            // Obtém o EnemyController (que contém a state machine)
            _enemyController = GetComponent<EnemyController>();

            foreach (EnemyBaseWeapon weapon in weapons)
            {
                weapon.SetOwner(this);

            }
        }

        // public void Update()
        // {
        // HandleAttackTimer();
        // }

        // private void HandleAttackTimer()
        // {
        //     if (!canAttack) return;
        //     if (currentAttackTime < attackTime)
        //     {
        //         currentAttackTime += Time.deltaTime;
        //     }
        //     else
        //     {
        //         currentAttackTime = 0;
        //         Attack();
        //     }
        // }

        // private void Attack()
        // {
        //     if (_animator)
        //     {
        //         _animator.SetTrigger("Attack");
        //     }
        // }

        /// <summary>
        /// Sobrescreve o método TakeDamage da BaseEntity para integrar com state machine
        /// </summary>
        public override void TakeDamage(int damage, GameObject source = null)
        {
            base.TakeDamage(damage, source);

            // Notifica ao estado machine que recebeu dano
            if (_enemyController && !_isDead)
            {
                _enemyController.OnDamageTaken();
            }
        }

        /// <summary>
        /// Sobrescreve OnDeath para notificar state machine
        /// </summary>
        public override void OnDeath()
        {
            base.OnDeath();

            if (_enemyController)
            {
                _enemyController.OnDeath();
            }
        }

        public int GetDamage()
        {
            return Random.Range(minDamage, maxDamage);
        }

        public bool GetCanHit()
        {
            return _enemyController.GetIsAttacking() && canHit;
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
