using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Data")]
public class EnemyData : ScriptableObject
{
    [Header("몬스터 기본 정보")] // 이 파일은 몬스터 스탯같은 것을 받아올 수 있도록 하는 기반이 되는 코드들임
    public string enemyName; // 몬스터 이름

    [Header("전투 스탯")]
    public int maxHealth; // 몬스터 체력
    public float moveSpeed; // 플레이어 추격할 때 속도
    public float damage; // 플레이어에게 입히는 데미지
    public float knockbackForce; // 이건 몬스터가 타격을 입었을 때 타격감을 위해 뒤로 밀려나도록 하는 코드

    [Header("인식 및 사거리 설정")]
    public float detectionRange; // 플레이어를 발견하는 거리
    public float stopDistance; // 추격을 멈추고 공격할 거리

    [Header("배회 설정")]
    public float wanderSpeed; // 배회할 때 속도
    public float wanderDuration; // 배회 시 한 방향으로 이동할 시간
    public float waitDuration; // 이동 후 멈춰 쉬는 시간

    [Header("스폰 설정")]
    public float spawnInterval; // 스폰 간격 시간 설정

    [Header("공격 설정")]
    public int attackDamage; // 공격력
    public float attackRange; // 공격 사거리
    public float attackCooldown; // 공격 쿨탐
}
