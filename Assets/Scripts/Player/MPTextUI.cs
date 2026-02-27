using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 필수
using Game.Player; // PlayerStats 접근을 위해 필요

public class MPTextUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private TextMeshProUGUI mpText; // Canvas UI용 TMP

    [Header("설정")]
    [SerializeField] private string format = "{0} / {1}"; // 예: 100 / 100

    private void Start()
    {
        // 1. 참조가 없으면 자동으로 찾기 시도
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (mpText == null)
            mpText = GetComponent<TextMeshProUGUI>();

        // 2. 이벤트 구독 (체력 변경 시 업데이트 함수 실행)
        if (playerStats != null)
        {
            playerStats.OnMPChanged += UpdateMPText;

            // 초기 텍스트 설정
            UpdateMPText(playerStats.CurrentMP, playerStats.MaxMP.Value);
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 이벤트 구독 해제
        if (playerStats != null)
            playerStats.OnHPChanged -= UpdateMPText;
    }

    private void UpdateMPText(float currentHP, float maxHP)
    {
        if (mpText == null) return;

        // 소수점 제거를 위해 정수(int)로 변환하여 표시
        mpText.text = string.Format(format, Mathf.CeilToInt(currentHP), Mathf.CeilToInt(maxHP));
    }
}