using UnityEngine;
using TMPro;

public class DamageText_Player : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float destroyTime = 1.0f;
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
            // [중요] 레이어 순서를 강제로 맨 앞으로 뺍니다.
            if (canvas != null)
            {
                canvas.sortingOrder = 999;
                if (canvas.renderMode == RenderMode.WorldSpace)
                    canvas.worldCamera = Camera.main ?? FindFirstObjectByType<Camera>();
            }
        }

        // 소환된 위치를 로그로 찍어 좌표가 이상한지 확인합니다.
        Debug.Log($"[DamageText] 생성 위치: {transform.position}, 데미지: {damage}");
        Invoke(nameof(DestroySelf), destroyTime);
    }

    void Update()
    {
        // 위로 이동
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        if (textMesh != null)
        {
            alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * 3f);
            textMesh.color = alpha;
        }
    }

    private void DestroySelf() => Destroy(gameObject);
}