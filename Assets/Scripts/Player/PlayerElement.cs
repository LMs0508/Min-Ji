using UnityEngine;
using System;
using Game.Core;

namespace Game.Player
{
    public class PlayerElement : MonoBehaviour
    {
        [SerializeField] private ElementType currentElement = ElementType.None;
        public ElementType CurrentElement => currentElement;
        public bool HasElement => currentElement != ElementType.None;

        public event Action<ElementType, ElementType> OnElementChanged;
        // (old, new)

        public void SetElement(ElementType newElement)
        {
            if (newElement == currentElement) return;

            var old = currentElement;
            currentElement = newElement;
            OnElementChanged?.Invoke(old, currentElement);
        }
    }
}