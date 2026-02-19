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
        if (isFast) return false;

        // 쿨타임 체크
        if (Time.time < lastUsedTime + cooldown)
        {
            Debug.Log("쿨타임");
            return false;
        }

        // 마나 체크/소모
        var stats = owner.GetComponentInChildren<PlayerStats>();
        if (stats == null || !stats.SpendMP(skillManaCost))
        {
            Debug.Log("마나 부족");
            return false;
        }

        // 이동 컨트롤러
        var controller = owner.GetComponent<TopDownCharacterController>();
        if (controller == null)
        {
            Debug.LogWarning("SwiftnessSkill: owner에 TopDownCharacterController가 없어!");
            return false;
        }

        // 코루틴 실행 주체(스킬 프리팹이 비활성일 수도 있으니 owner에서)
        var runner = owner.GetComponent<CoroutineRunner>();
        if (runner == null)
        {
            Debug.LogWarning("SwiftnessSkill: owner에 CoroutineRunner가 없어!");
            return false;
        }

        lastUsedTime = Time.time;
        runner.StartCoroutine(SwiftnessRoutine(owner, controller));
        return true;
    }

    private IEnumerator SwiftnessRoutine(GameObject owner, TopDownCharacterController controller)
    {
        isFast = true;

        // 오라 켜기(선택)
        GameObject aura = FindChildByName(owner.transform, auraChildName);
        if (aura != null) aura.SetActive(true);

        originalSpeed = controller.speed;
        controller.speed = originalSpeed * speedMultiplier;

        yield return new WaitForSeconds(duration);

        controller.speed = originalSpeed;
        if (aura != null) aura.SetActive(false);

        isFast = false;
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