using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDesc;
    public CanvasGroup canvasGroup;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 유지하고 싶다면 DontDestroyOnLoad(gameObject); 추가 가능
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    public void ShowTooltip(ItemData item, Vector3 spawnPosition)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        itemIcon.sprite = item.icon;
        itemName.text = item.itemName;
        itemDesc.text = item.description;

        // 슬롯 위치 기준으로 위쪽에 배치
        transform.position = spawnPosition + new Vector3(0, 150, 0);

        fadeRoutine = StartCoroutine(FadeIn());
    }

    public void HideTooltip()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        canvasGroup.alpha = 0;
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(0.5f);
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * 5f;
            yield return null;
        }
    }
}