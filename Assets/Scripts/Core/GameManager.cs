using UnityEngine;
using Game.Player;
using Game.UI;

namespace Game.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerElement playerElement;
        [SerializeField] private HUDController hud;

        private void Start()
        {
            // HUD ¿¬°á
            hud.Bind(playerStats, playerElement);
        }
    }
}