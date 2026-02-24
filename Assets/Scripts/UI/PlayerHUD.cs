using UnityEngine;
using UnityEngine.UI;
using Game.Player;

public class PlayerHUD : MonoBehaviour
{
    [Header("ÄÄÆ÷³ÍÆ®")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image mpFillImage;

    private void Start()
    {
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerStats = player.GetComponent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.OnHPChanged += UpdateHPUI;
            playerStats.OnMPChanged += UpdateMPUI;

            UpdateHPUI(playerStats.CurrentHP, playerStats.MaxHP.Value);
            UpdateMPUI(playerStats.CurrentMP, playerStats.MaxMP.Value);
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHPChanged -= UpdateHPUI;
            playerStats.OnMPChanged -= UpdateMPUI;
        }
    }

    private void UpdateHPUI(float current, float max)
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = current / max;
        }
    }

    private void UpdateMPUI(float current, float max)
    {
        if (mpFillImage != null)
        {
            mpFillImage.fillAmount = current / max;
        }
    }
}
