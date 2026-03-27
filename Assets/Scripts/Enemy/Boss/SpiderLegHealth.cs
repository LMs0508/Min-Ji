using UnityEngine;

// EnemyHealth를 상속받아 플레이어의 공격 로직이 이 스크립트를 찾아내게 만듭니다.
public class SpiderLegHealth : EnemyHealth
{
    private ProceduralSpiderLeg leg;

    protected override void Awake()
    {
        // 부모(EnemyHealth)의 Awake()를 부르지 않아서, 불필요한 몸통 스탯이나 애니메이터를 찾는 것을 막습니다.
        leg = GetComponent<ProceduralSpiderLeg>();

        // [추가] 몸통(본체)의 EnemyHealth에서 데미지 텍스트 프리팹을 자동으로 복사해옵니다.
        if (leg != null && leg.body != null)
        {
            // 부모(최상단)에 EnemyHealth가 있어도 찾을 수 있도록 GetComponentInParent 사용
            EnemyHealth bodyHealth = leg.body.GetComponentInParent<EnemyHealth>();
            if (bodyHealth != null) this.damageTextPrefab = bodyHealth.damageTextPrefab;
        }
    }

    protected override void Start()
    {
        // 부모의 Start()를 부르지 않아서 UI 슬라이더 초기화 등 불필요한 로직을 막습니다.
    }

    // 플레이어의 공격 스크립트가 TakeDamage를 호출하면, 부모의 몸통 데미지 로직 대신 아래 코드가 덮어쓰기(Override)되어 실행됩니다.
    public override void TakeDamage(float damage)
    {
        if (leg != null)
        {
            leg.TakeDamage(damage); // 데미지를 다리(ProceduralSpiderLeg) 쪽으로 전달
            ShowDamageText(damage); // [추가] 맞은 다리 위치에 데미지 텍스트 띄우기
        }
    }

    public override void TakeDamage(float damage, Vector2 knockbackDir)
    {
        if (leg != null)
        {
            leg.TakeDamage(damage); // 다리는 넉백을 받지 않으므로 데미지만 전달
            ShowDamageText(damage); // [추가] 맞은 다리 위치에 데미지 텍스트 띄우기
        }
    }

    // [추가] 다리 전용 데미지 텍스트 출력 로직 (몸통의 캔버스를 빌려 씁니다)
    protected override void ShowDamageText(float damage)
    {
        if (damageTextPrefab == null || leg == null || leg.body == null) return;

        // 최상단 부모(몸통)에 있는 EnemyHealth를 가져와 데미지 텍스트 전용 캔버스를 요청합니다.
        EnemyHealth rootHealth = leg.body.GetComponentInParent<EnemyHealth>();
        Canvas bodyCanvas = rootHealth != null ? rootHealth.GetDamageTextCanvas() : null;
        if (bodyCanvas != null)
        {
            // 몸통의 캔버스를 공유하여 생성 (연타 스태킹이 같이 적용됨)
            GameObject textObj = Instantiate(damageTextPrefab, bodyCanvas.transform, false);
            textObj.SetActive(true);
            
            // [수정] 하드코딩 제거: 맞은 다리 위치로 이동 후, 프리팹에서 설정한 로컬 오프셋 값을 더해줍니다.
            textObj.transform.position = transform.position;
            textObj.transform.localPosition += damageTextPrefab.transform.localPosition;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            RectTransform prefabRect = damageTextPrefab.GetComponent<RectTransform>();
            if (rect != null && prefabRect != null) {
                rect.sizeDelta = prefabRect.sizeDelta; // 텍스트 상자 크기 동기화
                
                // [핵심 해결] 보스 캔버스의 거대한 스케일 뻥튀기를 역산하여 텍스트 크기를 정상화
                Vector3 canvasWorldScale = bodyCanvas.transform.lossyScale;
                Vector3 baseScale = prefabRect.localScale * 0.01f;

                float finalX = canvasWorldScale.x != 0 ? baseScale.x / canvasWorldScale.x : prefabRect.localScale.x;
                float finalY = canvasWorldScale.y != 0 ? baseScale.y / canvasWorldScale.y : prefabRect.localScale.y;
                float finalZ = canvasWorldScale.z != 0 ? baseScale.z / canvasWorldScale.z : prefabRect.localScale.z;

                rect.localScale = new Vector3(finalX, finalY, finalZ);
            }

            textObj.GetComponent<DamageText>()?.Setup(damage);
        }
    }
}