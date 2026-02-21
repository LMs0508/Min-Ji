using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;
using Game.Player;

public class SwiftnessSkill : MonoBehaviour, ISkill
{
    [Header("UI")]
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;


    [Header("신속화 설정")]
    public float speedMultiplier = 3f;
    public float duration = 3f;
    public float cooldown = 8f;
    public float skillManaCost = 10f;

    [Header("시각 효과(플레이어 자식 오브젝트 이름)")]
    public string auraChildName = "AuraEffect"; // 네 플레이어에 있는 이펙트 오브젝트 이름으로 맞춰도 됨

    private float lastUsedTime = -999f;
    private bool HasteOn = false;
    private bool isFast = false;

    private float originalSpeed;

    //  쿨타임 UI용
    public float Cooldown => cooldown;

    public float CooldownRemaining
    {
        get
        {
            float remain = (lastUsedTime + cooldown) - Time.time;
            return Mathf.Max(0f, remain);
        }
    }

    public bool TryUse(GameObject owner)
    {
        if (owner == null) return false;

        // 쿨타임
        if (Time.time < lastUsedTime + cooldown)
        {
            Debug.Log("쿨타임");
            return false;
        }

        var stats = owner.GetComponentInChildren<PlayerStats>();
        if (stats == null || !stats.SpendMP(skillManaCost))
        {
            Debug.Log("마나 부족");
            return false;
        }

        var runner = owner.GetComponent<CoroutineRunner>();
        if (runner == null)
        {
            Debug.LogWarning("SwiftnessSkill: owner에 CoroutineRunner가 없어!");
            return false;
        }

        lastUsedTime = Time.time;
        runner.StartCoroutine(SwiftnessRoutine(owner, stats));
        return true;
    }

    private IEnumerator SwiftnessRoutine(GameObject owner, PlayerStats stats)
    {
        // 오라 켜기
        GameObject aura = FindChildByName(owner.transform, auraChildName);
        if (aura != null) aura.SetActive(true);

        // 여기서부터 핵심: 스탯에 배수 적용
        stats.MoveSpeed.Multiply(speedMultiplier);

        yield return new WaitForSeconds(duration);

        //  끝나면 정확히 되돌리기
        stats.MoveSpeed.Divide(speedMultiplier);

        if (aura != null) aura.SetActive(false);
    }

    private GameObject FindChildByName(Transform root, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        // 루트 포함 전체 탐색(비활성 포함)
        var all = root.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (t.name == name) return t.gameObject;
        }
        return null;
    }
}