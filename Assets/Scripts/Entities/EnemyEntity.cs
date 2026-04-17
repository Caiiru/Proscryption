using UnityEngine;
using proscryption.Enemy;
using UnityEngine.VFX;

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

        [Header("VFX")]
        public VisualEffect takeDamageVFX;
        public VisualEffect deathVFX;

        public override void Start()
        {
            base.Start();

            // Obtém o EnemyController (que contém a state machine)
            _enemyController = GetComponent<EnemyController>();

            foreach (EnemyBaseWeapon weapon in weapons)
            {
                weapon.SetOwner(this);

            }

            SetupVFX();
        }
        void SetupVFX()
        {
            if (takeDamageVFX)
            {
                takeDamageVFX.Stop();
            }
            if (deathVFX)
            {
                deathVFX.Stop();
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
        public override void TakeDamage(int damage, GameObject source = null, bool isCritical = false)
        {
            base.TakeDamage(damage, source, isCritical);

            // Notifica ao estado machine que recebeu dano
            if (_enemyController && !_isDead)
            {
                _enemyController.OnDamageTaken();
            }
            if (takeDamageVFX)
            {
                takeDamageVFX.Play();
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
            if (deathVFX)
            {
                deathVFX.Play();
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
