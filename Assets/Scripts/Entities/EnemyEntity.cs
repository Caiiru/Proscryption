using UnityEngine;
using UnityEngine.VFX;

namespace proscryption
{
    public class EnemyEntity : BaseEntity
    {
        [Header("Attack Settings")]

        public bool canAttack = false;
        public int attackTime = 6;
        public int minDamage = 3;
        public int maxDamage = 8;
        [Range(0, 100)]
        public int criticalChange;


        [Header("VFX")]
        public VisualEffect takeDamageVFX;
        public VisualEffect deathVFX;

        public override void Start()
        {
            base.Start();



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

        public override void TakeDamage(int damage, GameObject source = null, bool isCritical = false)
        {
            base.TakeDamage(damage, source, isCritical);

            // Notifica ao estado machine que recebeu dano

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

            if (deathVFX)
            {
                deathVFX.Play();
            }
        }
        public int GetAttackDamage()
        {
            return Random.Range(minDamage, maxDamage);
        }
        public bool IsCritical()
        {
            return Random.value < criticalChange / 100;
        }
    }
}
