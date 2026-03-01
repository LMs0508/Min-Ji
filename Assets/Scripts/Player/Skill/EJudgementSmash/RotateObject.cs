using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("회전 속도")]
    public float rotationSpeed = 50f;

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}