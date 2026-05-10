using UnityEngine;
using System.Collections;
using Game.Player;

public class BossPoisonPuddle : MonoBehaviour
{
    [Header("웅덩이 설정")]
    public float lifeTime = 5f;       // 웅덩이 유지 시간
    public float puddleDamage = 5f;   // 웅덩이 위에 서 있을 때 받는 데미지
    public float tickRate = 0.5f;     // 데미지 틱 간격
    
    private float lastTickTime;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(PuddleLifeRoutine());
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // 웅덩이 밟고 있을 때 지속 데미지 (0.5초마다)
            if (Time.time > lastTickTime + tickRate)
            {
                lastTickTime = Time.time;
                PlayerStats stats = col.GetComponent<PlayerStats>() ?? col.GetComponentInParent<PlayerStats>() ?? col.transform.root.GetComponentInChildren<PlayerStats>();
                if (stats != null) stats.TakeDamage(puddleDamage);
            }

            // 밟는 즉시 2초짜리 도트뎀 독 디버프를 지속적으로 갱신 (나가도 2초 더 달게 함)
            PlayerPoisonDebuff debuff = col.gameObject.GetComponent<PlayerPoisonDebuff>();
            if (debuff == null) debuff = col.gameObject.AddComponent<PlayerPoisonDebuff>();
            debuff.ApplyPoison(2f, 3f); 
        }
    }

    private IEnumerator PuddleLifeRoutine()
    {
        yield return new WaitForSeconds(lifeTime - 1f); // 수명 1초 전까지 유지

        // 마지막 1초 동안 서서히 투명해지며 사라지는 연출
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}