using UnityEditor;
using UnityEngine;

namespace proscryption
{
    public static class RewardsListIconUtility
    {
        /// <summary>
        /// Returns the corresponding icon for a given SimpleRewardType from the RewardsListIcon ScriptableObject.
        /// Get the Scriptable Object from HUDManager
        /// </summary>
        /// <param name="rewardsListIcon"></param>
        /// <param name="simpleType"></param>
        /// <returns></returns>
        public static Sprite GetIconForSimpleRewardType(RewardsListIcon rewardsListIcon, SimpleRewardType simpleType)
        {
            foreach (SimpleRewardIcon rewardTypeIcon in rewardsListIcon.rewardsIcons)
            {
                if (rewardTypeIcon.type == simpleType)
                {
                    return rewardTypeIcon.icon;
                }
            }
            Debug.LogError("Not Find Icon");
            return null; // Return null if no matching type is found
        }
    }


    [CreateAssetMenu(fileName = "New Rewards List Icon", menuName = "Rewards/RewardsListIcon")]
    public class RewardsListIcon : ScriptableObject
    {
        public SimpleRewardIcon[] rewardsIcons;
    }
    [System.Serializable]
    public struct SimpleRewardIcon
    {
        public SimpleRewardType type;
        [SpritePreview]
        public Sprite icon;
    }



#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SimpleRewardIcon))]
    public class SimpleRewardTypeIconDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Pega o campo 'type' da struct
            SerializedProperty typeProp = property.FindPropertyRelative("type");

            // Define o label como o nome do enum selecionado
            string enumName = typeProp.enumDisplayNames[typeProp.enumValueIndex];
            label.text = enumName;

            // Desenha o resto do inspector padrão
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
#endif
}
