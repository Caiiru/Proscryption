using UnityEngine;

namespace proscryption
{
    public class DummyEntity : BaseEntity
    {
        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            Debug.Log($"{this.transform.name} is taking {damage} damage");
        }
    }
}
