using UnityEngine;

public class ClickEffectHelper : MonoBehaviour
{
    [Header("설정")]
    public float fadeSpeed = 2f;      // 투명해지는 속도

    private SpriteRenderer spriteRenderer;
    private Color color;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            color = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (spriteRenderer != null)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = color;
        }
    }
}