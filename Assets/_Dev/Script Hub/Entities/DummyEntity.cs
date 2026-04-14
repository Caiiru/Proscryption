using UnityEngine;
using UnityEngine.VFX;

namespace proscryption
{

    public class DummyEntity : EnemyEntity
    {

        public bool isImmortal = false;


        [Header("Visual Effects")]

        public VisualEffect bloodVFX;
        public override void Start()
        {
            base.Start();

            if (bloodVFX)
                bloodVFX.Stop();
        }
        public override void TakeDamage(int damage,
                                        GameObject source = null, bool isCritical = false)
        {
            base.TakeDamage(damage, source, isCritical);
            Debug.Log($"{this.transform.name} is taking {damage} damage");


            if (bloodVFX)
                bloodVFX.Play();


            if (isImmortal)
            {
                health = maxHealth;
            }
        }

    }
}
