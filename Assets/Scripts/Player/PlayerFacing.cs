using UnityEngine;

public class PlayerFacing : MonoBehaviour
{
    public Vector2 LastFacingDir { get; private set; } = Vector2.right;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(x, y);

        // 입력이 있으면만 갱신
        if (input.sqrMagnitude > 0.0001f)
        {
            input.Normalize();
            LastFacingDir = input;
        }
    }
}