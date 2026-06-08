using System;
using UnityEngine;
using UnityEngine.VFX;

namespace proscryption
{
    public class EnemyEntity : BaseEntity
    {
        [Header("Attack Settings")] public bool canAttack = false;
        public int attackTime = 6;
        public int minDamage = 3;
        public int maxDamage = 8;
        [Range(0, 100)] public int criticalChange;


        public Action<Vector3?, ForceMode?> OnTakeDamage;
        public Action<Vector3?, ForceMode?> OnDeath;

        [Header("VFX")] public GameObject takeDamageVFX;
        public GameObject deathVFX;

        public override void Start()
        {
            base.Start();


            SetupVFX();
        }

        void SetupVFX()
        {
            if (takeDamageVFX)
            {
                if (takeDamageVFX.TryGetComponent<VisualEffect>(out VisualEffect takeDamageVisualEffect))
                    takeDamageVisualEffect.Stop();
                else
                {
                    if (takeDamageVFX.TryGetComponent<ParticleSystem>(out ParticleSystem takeDamageParticleSystem))
                    {
                        takeDamageParticleSystem.Stop();
                    }
                }
            }

            if (deathVFX)
            {
                if (deathVFX.TryGetComponent<VisualEffect>(out VisualEffect deathVisualEffect))
                    deathVisualEffect.Stop();
                else
                {
                    if (deathVFX.TryGetComponent<ParticleSystem>(out ParticleSystem deathParticleSystem))
                    {
                        deathParticleSystem.Stop();
                    }
                }
            }
        }

        public override void TakeDamage(int damage, GameObject source = null, bool isCritical = false,
            Vector3? dmgForce = null, ForceMode? forceMode = null)
        {
            base.TakeDamage(damage, source, isCritical, dmgForce, forceMode);

            OnTakeDamage?.Invoke(dmgForce, forceMode);
            if (takeDamageVFX)
            {
                if (takeDamageVFX.TryGetComponent<VisualEffect>(out VisualEffect takeDamageVisualEffect))
                    takeDamageVisualEffect.Play();
                else
                {
                    if (takeDamageVFX.TryGetComponent<ParticleSystem>(out ParticleSystem takeDamageParticleSystem))
                    {
                        takeDamageParticleSystem.Play();
                    }
                }
            }
        }


        /// <summary>
        /// Sobrescreve OnDeath para notificar state machine
        /// </summary>
        public override void Death(Vector3? force, ForceMode? mode)
        {
            base.Death(force, mode);
            OnDeath?.Invoke(force, mode);
            // Debug.Log("Minion Death Entity");
            if (deathVFX)
            {
                if (deathVFX.TryGetComponent<VisualEffect>(out VisualEffect deathVisualEffect))
                    deathVisualEffect.Play();
                else
                {
                    if (deathVFX.TryGetComponent<ParticleSystem>(out ParticleSystem deathParticleSystem))
                    {
                        deathParticleSystem.Play();
                    }
                }
            }
        }

        public int GetAttackDamage()
        {
            return UnityEngine.Random.Range(minDamage, maxDamage);
        }

        public bool IsCritical()
        {
            return UnityEngine.Random.value < criticalChange / 100;
        }
    }
}