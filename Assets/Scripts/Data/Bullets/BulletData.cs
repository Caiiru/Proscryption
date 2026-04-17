using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "BulletData", menuName = "ScriptableObjects/BulletData", order = 1)]
    public class BulletData : ScriptableObject
    {
        public int minDamage;
        public int maxDamage;
        [Range(0, 100)]
        public int critChance;

        public float speed;
        public float duration;

        public GameObject bulletPrefab;

        [Header("EFFECTS")]
        public GameObject impactEffect;

    }

}
