using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace proscryption
{
    public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private float _startScale = 0;
        private float _animDuration = 0.1f;

        private void Start()
        {
            _startScale = transform.localScale.x;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(_startScale + 0.1f, _animDuration).SetEase(Ease.InSine);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(_startScale, _animDuration).SetEase(Ease.InSine);
        }
    }
}