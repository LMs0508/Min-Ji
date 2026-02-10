using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;
using Game.Player;

public class PlayerSwiftness : MonoBehaviour
{
    [Header("신속화 설정")]
    public float speedMultiplier;
    public float duration;
    public float cooldown;
    public float SkillManaCost;

    [Header("시각 효과")]
    public GameObject auraEffect;

    private TopDownCharacterController controller;
    private float originalSpeed;
    private float lastUsedTime;
    private bool isFast = false;

    void Awake()
    {
        controller = GetComponent<TopDownCharacterController>();
        if (auraEffect != null) auraEffect.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isFast)
        {
            if (Time.time >= lastUsedTime + cooldown)
            {
                PlayerStats stats = GetComponentInChildren<PlayerStats>();

                if (stats != null)
                {
                    if (stats.SpendMP(SkillManaCost))
                    {
                        StartCoroutine(SwiftnessRoutine());
                    }
                    else
                    {
                        Debug.Log("마나 부족");
                    }
                }
            }
            else
            {
                Debug.Log("쿨타임");
            }
        }
    }

    private IEnumerator SwiftnessRoutine()
    {
        isFast = true;
        lastUsedTime = Time.time;

        if (auraEffect != null) auraEffect.SetActive(true);

        originalSpeed = controller.speed;
        controller.speed = originalSpeed * speedMultiplier;

        yield return new WaitForSeconds(duration);

        if (auraEffect != null) auraEffect.SetActive(false);
        controller.speed = originalSpeed;
        isFast = false;
    }
}