using UnityEngine;

namespace proscryption
{
    [CreateAssetMenu(fileName = "ColorDatabase", menuName = "ScriptableObjects/ColorDatabase")]
    [System.Serializable]
    public class ColorDatabase : ScriptableObject
    {
        [Header("Color Preview")]
        [Space]
        [Header("Player")]
        [SerializeField]
        public Color StandardColor;
        [ColorUsage(true, true)]
        public Color BloodColor;
        [ColorUsage(true, true)]
        public Color LightColor;
    }
}
