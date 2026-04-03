using System;
using DG.Tweening;
using UnityEngine;

namespace proscryption
{
    public class PauseManager : MonoBehaviour
    {


        private void PauseGame()
        {
            Time.timeScale = 0;

        }
        private void UnpauseGame()
        {
            Time.timeScale = 1;
        }

        public void HidePauseScreen()
        {
            this.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
            // UnpauseGame();

        }

        public void ShowPauseScreen()
        { 
            this.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            // PauseGame();
        }
    }
}
