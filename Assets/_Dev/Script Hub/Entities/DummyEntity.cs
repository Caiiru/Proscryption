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
            bloodVFX.Stop();
        }
        public override void TakeDamage(int damage, GameObject source = null)
        {
            base.TakeDamage(damage, source);
            Debug.Log($"{this.transform.name} is taking {damage} damage");
            bloodVFX.Play();
            if (isImmortal)
            {
                health = maxHealth;
            }
        }

    }
}
