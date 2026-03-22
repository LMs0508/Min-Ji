using UnityEngine;
using Game.Core;

public class JudgementSmashEarthEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Earth;

    [Header("베리어 설정")]
    public GameObject barrierPrefab;

    public void OnStart(GameObject owner)
    {
    }

    public void OnUpdate(GameObject owner) { }

    public void OnEnd(GameObject owner)
    {
        if (barrierPrefab != null)
        {
            GameObject currentBarrier = Instantiate(barrierPrefab, owner.transform.position, Quaternion.identity);

            currentBarrier.transform.SetParent(owner.transform);

            Destroy(currentBarrier, 3.0f);
        }
    }
}