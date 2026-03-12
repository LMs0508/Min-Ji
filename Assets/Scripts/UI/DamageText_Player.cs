using UnityEngine;
using TMPro;

public class DamageText_Player : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float destroyTime = 1.0f;

    [Header("스태킹 설정")]
    public float stackOffset = 0.6f; // [추가] 데미지가 쌓일 때 밀어올릴 세로 간격

    private TextMeshProUGUI textMesh;
    private Color alpha;
    private Canvas canvas;

    void Awake()
    {
        // 스크립트가 붙은 오브젝트에서 바로 찾기
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();

        // Canvas는 부모(PlayerUI)에게 있으므로 InParent로 찾습니다.
        canvas = GetComponentInParent<Canvas>();
        if (textMesh != null) alpha = textMesh.color;
    }

    public void Setup(float damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            // 레이어 순서를 강제로 맨 앞으로 뺍니다.
            if (canvas != null)
            {
                canvas.sortingOrder = 999;
                if (canvas.renderMode == RenderMode.WorldSpace)
                    canvas.worldCamera = Camera.main ?? FindFirstObjectByType<Camera>();
            }
        }

        if (transform.parent != null)
        {
            // 같은 부모(popupPoint) 아래에 있는 모든 데미지 텍스트를 찾습니다.
            DamageText_Player[] existingTexts = transform.parent.GetComponentsInChildren<DamageText_Player>();
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

        Invoke(nameof(DestroySelf), destroyTime);
    }

    void Update()
    {
        // 서서히 위로 이동 (기존 로직 유지)
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        if (textMesh != null)
        {
            // 서서히 투명해짐
            alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * 3f);
            textMesh.color = alpha;
        }
    }

    private void DestroySelf() => Destroy(gameObject);
}