using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float destroyTime = 0.5f;

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
            textMesh.text = damage.ToString();
        }
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