using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    public float amplitude = 0.2f; // 움직임 범위
    public float frequency = 2f;   // 속도
    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    void Start() => posOffset = transform.localPosition;

    void Update()
    {
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        transform.localPosition = tempPos;
    }
}