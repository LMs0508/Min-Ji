using UnityEngine;
using Game.Core;

public class SwiftnessFireEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Fire; // 불 속성 지정
    public GameObject fireTrailPrefab; // 인스펙터에서 불길 프리팹 할당
    public float spawnInterval = 0.1f; // 불길 생성 간격
    private float timer;

    public void OnStart(GameObject owner)
    {
        Debug.Log("불의 신속화: 이동 경로에 불꽃이 일어납니다!");
        timer = 0f;
    }

    public void OnUpdate(GameObject owner)
    {
        // 일정 시간마다 발밑에 불길 생성
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            Instantiate(fireTrailPrefab, owner.transform.position, Quaternion.identity);
            timer = 0f;
        }
    }

    public void OnEnd(GameObject owner)
    {
        Debug.Log("불의 신속화 종료");
    }
}