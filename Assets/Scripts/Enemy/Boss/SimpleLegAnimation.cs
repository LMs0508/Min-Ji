using UnityEngine;

public class SimpleLegAnimation : MonoBehaviour
{
    [SerializeField] private Transform[] legs;
    [SerializeField] private float waveHeight = 0.15f;  // 다리가 흔들리는 높이
    [SerializeField] private float waveSpeed = 6f;      // 흔들리는 속도

    private Vector3[] originalLocalPositions;

    private void Start()
    {
        originalLocalPositions = new Vector3[legs.Length];
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i] != null)
                originalLocalPositions[i] = legs[i].localPosition;
        }
    }

    private void Update()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i] == null) continue;

            // 다리마다 위상을 다르게 해서 교차로 움직이게 함
            float phase = (float)i / legs.Length * Mathf.PI * 2f;
            float offsetY = Mathf.Sin(Time.time * waveSpeed + phase) * waveHeight;

            legs[i].localPosition = originalLocalPositions[i] + new Vector3(0f, offsetY, 0f);
        }
    }
}
