using UnityEngine;
using UnityEngine.UI;

public class SkillGaugeUI : MonoBehaviour
{
    [SerializeField] private Image gaugeFill;
    [SerializeField] private GameObject rootObject; // UI 전체 부모 오브젝트

    private void Awake()
    {
        // 시작할 때 게이지를 숨깁니다.
        Hide();
    }

    public void SetGauge(float current, float max)
    {
        if (gaugeFill == null) return;

        float ratio = Mathf.Clamp01(current / max);
        gaugeFill.fillAmount = ratio;

        // 게이지가 0보다 크면 보이고, 0이면 숨깁니다.
        if (rootObject != null)
        {
            if (ratio > 0 && !rootObject.activeSelf) rootObject.SetActive(true);
            else if (ratio <= 0 && rootObject.activeSelf) rootObject.SetActive(false);
        }
    }

    public void Show()
    {
        if (rootObject != null) rootObject.SetActive(true);
    }

    public void Hide()
    {
        if (rootObject != null) rootObject.SetActive(false);
    }
}