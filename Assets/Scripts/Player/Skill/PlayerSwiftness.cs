using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class PlayerSwiftness : MonoBehaviour
{
    [Header("신속화 설정")]
    public float speedMultiplier;
    public float duration;
    public float cooldown;

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
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= lastUsedTime + cooldown && !isFast)
        {
            StartCoroutine(SwiftnessRoutine());
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