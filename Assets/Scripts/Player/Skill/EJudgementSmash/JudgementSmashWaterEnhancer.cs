using UnityEngine;
using Game.Core;

public class JudgementSmashWaterEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Water;

    [Header("회오리 설정")]
    public GameObject whirlwindPrefab;

    public void OnStart(GameObject owner) { }
    public void OnUpdate(GameObject owner) { }

    public void OnEnd(GameObject owner)
    {
        if (whirlwindPrefab != null)
        {
            GameObject whirlwind = Instantiate(whirlwindPrefab, owner.transform.position, Quaternion.identity);

            // 3초 뒤 자동 소멸
            Destroy(whirlwind, 3.0f);
        }
    }
}