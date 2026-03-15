using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float destroyTime = 0.5f;

    [Header("스태킹 설정")]
    public float stackOffset = 0.6f; // [추가] 데미지가 쌓일 때 밀어올릴 세로 간격

    // [수정] 캔버스 자식이라면 UGUI 타입이 맞습니다.
    private TextMeshProUGUI textMesh;
    private Color alpha;

    void Awake()
    {
        // 자기 자신 혹은 자식에게서 TextMeshProUGUI를 찾습니다.
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();

        if (textMesh != null)
        {
            alpha = textMesh.color;
        }
    }

    public void Setup(float damage)
    {
        if (textMesh != null)
        {
            textMesh.text = Mathf.RoundToInt(damage).ToString();
        }

        // =========================================================
        // [핵심 추가] 메이플스토리 스타일 연타 스태킹 로직
        // =========================================================
        if (transform.parent != null)
        {
            // 같은 부모(적 UI 캔버스 등) 아래에 있는 모든 데미지 텍스트를 찾습니다.
            DamageText[] existingTexts = transform.parent.GetComponentsInChildren<DamageText>();
            foreach (var txt in existingTexts)
            {
                // 새로 생성된 나 자신(this)은 제외하고, 
                // 아직 화면에 떠있는 '이전 데미지'들만 강제로 위로 밀어 올립니다.
                if (txt != this && txt.gameObject.activeInHierarchy)
                {
                    txt.transform.position += Vector3.up * stackOffset;
                }
            }
        }
        // =========================================================

        Invoke("DestroySelf", destroyTime);
    }

    void Update()
    {
        // 1. 위로 이동 (이동 수식: $y = y_0 + v \cdot t$)
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // 2. 투명도 조절
        if (textMesh != null)
        {
            alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * 5f);
            textMesh.color = alpha;
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}