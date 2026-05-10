using UnityEngine;
using System.Collections;
using Game.Player;

public class BossScreenSlash : MonoBehaviour
{
    [Header("광선 설정")]
    public float warningTime = 0.8f; // 얇은 선으로 경고하는 시간
    public float slashTime = 0.2f;   // 굵게 변해서 실제 데미지가 들어가는 시간
    public float damage = 25f;
    
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private bool hasDamaged = false; // [추가] 다단히트(중복 데미지) 방지용 플래그

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        
        if (col != null) col.enabled = false; // 경고 중에는 데미지 판정 없음
        
        StartCoroutine(SlashSequence());
    }

    private IEnumerator SlashSequence()
    {
        // 1. 경고 페이즈 (얇고 반투명한 붉은 선)
        sr.color = new Color(1f, 0f, 0f, 0.4f);
        // 화면을 덮어야 하므로 X(길이)는 엄청 길게, Y(두께)는 얇게 설정합니다.
        transform.localScale = new Vector3(40f, 0.1f, 1f); 
        
        yield return new WaitForSeconds(warningTime);

        // 2. 발동 페이즈 (유리창 깨지는 느낌, 화면 흔들림, 굵고 선명한 선)
        sr.color = new Color(1f, 0f, 0f, 1f); // 완전한 불투명 빨간색
        transform.localScale = new Vector3(40f, 1.5f, 1f); // 두께가 0.1 -> 1.5로 확 굵어짐
        if (col != null) col.enabled = true; // 데미지 판정 ON

        // [핵심] 타격감을 위해 화면 전체를 흔듭니다.
        CameraShake.Instance?.Shake(0.3f, 0.5f);
        
        yield return new WaitForSeconds(slashTime);

        // 3. 소멸 페이즈 (판정 끄고 서서히 사라짐)
        if (col != null) col.enabled = false;
        
        float fade = 1f;
        while (fade > 0)
        {
            fade -= Time.deltaTime * 3f;
            sr.color = new Color(1f, 0f, 0f, fade);
            yield return null;
        }
        
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (hasDamaged) return; // 한 번 데미지를 입혔으면 종료

        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats == null) playerStats = other.GetComponentInParent<PlayerStats>();
            if (playerStats == null) playerStats = other.transform.root.GetComponentInChildren<PlayerStats>(); // 최상위 부모까지 안전하게 검색
            
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                hasDamaged = true; // 데미지를 주었다고 체크 (다단히트 방지)
            }
        }
    }
}