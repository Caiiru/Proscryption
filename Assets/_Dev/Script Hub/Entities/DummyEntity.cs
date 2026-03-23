using UnityEngine;

namespace proscryption
{
    public class DummyEntity : EnemyEntity
    {
        public bool isImmortal = false;


        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            Debug.Log($"{this.transform.name} is taking {damage} damage");

            if (isImmortal)
            {
                health = maxHealth;
            }
        }

    }
}
