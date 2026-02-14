using UnityEngine;

public enum SkillSlot { Q, W, E, R }

[CreateAssetMenu(menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillId;
    public string skillName;
    public Sprite icon;

    public float cooldown = 1f;

    // 나중에 여기에 데미지, 범위, 프리팹 등 추가
}