using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector2 offset;

    private Transform target;

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning("[CameraFollow] Player 태그 오브젝트를 찾을 수 없습니다.");
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
