using UnityEngine;
using Cainos.PixelArtTopDown_Basic;
using Game.Player;

public class ApplyMoveSpeedFromStats : MonoBehaviour
{
    private TopDownCharacterController controller;
    private PlayerStats stats;

    private void Awake()
    {
        controller = GetComponent<TopDownCharacterController>();
        stats = GetComponentInChildren<PlayerStats>();
    }

    private void LateUpdate()
    {
        if (controller == null || stats == null) return;
        controller.speed = stats.MoveSpeed.Value;   //  최종값을 매 프레임 반영
    }
}