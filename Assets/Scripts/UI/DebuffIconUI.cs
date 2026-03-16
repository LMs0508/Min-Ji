using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebuffIconUI : MonoBehaviour
{
    public Image iconImage;      // 디버프 아이콘
    public Image fillImage;      // 쿨타임처럼 차오르는 이미지 (Filled 타입)
    public TextMeshProUGUI timeText; // 남은 시간 텍스트

    private float maxDuration;
    private float currentTimer;

    public void Setup(Sprite icon, float duration)
    {
        iconImage.sprite = icon;
        maxDuration = duration;
        currentTimer = duration;
    }

    public void UpdateTime(float remainingTime)
    {
        currentTimer = remainingTime;
        
        // 텍스트 업데이트 (소수점 첫째자리까지)
        timeText.text = currentTimer.ToString("F1");
        
        // 게이지 업데이트 (남은 비율)
        fillImage.fillAmount = currentTimer / maxDuration;

        if (currentTimer <= 0)
        {
            Destroy(gameObject);
        }
    }
}