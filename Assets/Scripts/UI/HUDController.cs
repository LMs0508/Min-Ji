using UnityEngine;
using UnityEngine.UI;
using Game.Player;
using Game.Core;

namespace Game.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Bars")]
        [SerializeField] private Image hpFill;
        [SerializeField] private Image mpFill;

        [Header("Element")]
        [SerializeField] private Image elementIcon;
        [SerializeField] private Sprite noneIcon;
        [SerializeField] private Sprite fireIcon;
        [SerializeField] private Sprite waterIcon;
        [SerializeField] private Sprite windIcon;
        [SerializeField] private Sprite earthIcon;

        private PlayerStats stats;
        private PlayerElement element;

        public void Bind(PlayerStats stats, PlayerElement element)
        {
            this.stats = stats;
            this.element = element;

            stats.OnHPChanged += UpdateHP;
            stats.OnMPChanged += UpdateMP;
            element.OnElementChanged += UpdateElement;

            UpdateHP(stats.CurrentHP, stats.MaxHP.Value);
            UpdateMP(stats.CurrentMP, stats.MaxMP.Value);
            UpdateElement(element.CurrentElement, element.CurrentElement);
        }

        private void OnDestroy()
        {
            if (stats != null)
            {
                stats.OnHPChanged -= UpdateHP;
                stats.OnMPChanged -= UpdateMP;
            }

            if (element != null)
            {
                element.OnElementChanged -= UpdateElement;
            }
        }

        void UpdateHP(float current, float max)
        {
            hpFill.fillAmount = current / max;
        }

        void UpdateMP(float current, float max)
        {
            mpFill.fillAmount = current / max;
        }

        void UpdateElement(ElementType oldE, ElementType newE)
        {
            elementIcon.sprite = newE switch
            {
                ElementType.Fire => fireIcon,
                ElementType.Water => waterIcon,
                ElementType.Wind => windIcon,
                ElementType.Earth => earthIcon,
                _ => noneIcon
            };
        }
    }
}