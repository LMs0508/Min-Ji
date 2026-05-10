using UnityEngine;
using UnityEngine.UI;

public class ChargeGaugeUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image gaugeFillImage; // 게이지가 차오르는 이미지 (Image Type: Filled)
    [SerializeField] private GameObject visualRoot; // 게이지 전체를 껐다 켰다 할 부모 오브젝트

    private PlayerAttackInput playerAttackInput;

    private void Start()
{
    
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    
    if (player != null)
    {
        playerAttackInput = player.GetComponent<PlayerAttackInput>();
        if (playerAttackInput != null)
        {
            playerAttackInput.OnChargeChanged += UpdateGauge;
        }
        else
        {
            Debug.LogError("차징 UI: 플레이어 오브젝트는 찾았으나 PlayerAttackInput 컴포넌트가 없습니다!");
        }
    }
    else
    {
        Debug.LogError("차징 UI: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다! 현재 씬에 Player 태그가 있는지 확인하세요.");
    }

    if (visualRoot != null) visualRoot.SetActive(false);
}
    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (playerAttackInput != null)
        {
            playerAttackInput.OnChargeChanged -= UpdateGauge;
        }
    }

    private void UpdateGauge(float ratio)
{
    if (visualRoot == null || gaugeFillImage == null) return;

    if (ratio > 0f)
    {
        // 꺼져 있을 때만 한 번 켜기
        if (!visualRoot.activeSelf) visualRoot.SetActive(true);
        
        gaugeFillImage.fillAmount = ratio;
    }
    else
    {
        // ratio가 0이면 숨기기
        if (visualRoot.activeSelf) visualRoot.SetActive(false);
    }
}
}
