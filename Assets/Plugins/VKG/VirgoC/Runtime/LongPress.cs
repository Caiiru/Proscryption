using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace DeveloperConsole
{
    // credits: Ian094 https://forum.unity.com/threads/how-do-i-detect-when-a-button-is-being-pressed-held-on-eventtype.352368/
    public class LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
     
        public bool buttonPressed;
         
        public void OnPointerDown(PointerEventData eventData){
            buttonPressed = true;
        }
         
        public void OnPointerUp(PointerEventData eventData){
            buttonPressed = false;
        }
    
    }
}
