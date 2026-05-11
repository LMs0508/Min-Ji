# Min-Ji
# USECASE
https://drive.google.com/file/d/1060muO2o7NqpIPSqxbGITIrgjMmIqpYq/view?usp=sharing

# Min-Ji 프로젝트 게임 개요

## 1. 게임 정체성

**장르**: 2D 탑다운 액션 RPG (마을 + 던전 클라이밍 + 보스전 구조)

**전체 흐름**:
TitleScene → GameScene → village(마을) → floor01~04(던전 4층) → Boss_Under1/2 → Boss_Room(보스전) → BossHouse / BossHouseAfter(보스 처치 후)

**차별 포인트**:
- **4원소(Fire/Water/Earth/Wind) 강화 시스템** — 같은 스킬도 현재 원소에 따라 효과가 달라짐
- **거미 보스 2페이즈 패턴** — 다리 개별 파괴 → 본체 노출 단계로 페이즈 전환
- **NPC 대화 + 컷신 + 퀘스트 자동 연동** — Timeline 자동 재생 기반

---

## 2. 플레이어 시스템

### 2.1 스탯 / 자원 ([PlayerStats.cs](Assets/Scripts/Player/PlayerStats.cs))
- `Stat` 클래스: 기본값 + 보너스 + 배수(Multiplier)로 동적 계산
- HP/MP 자동 재생 (매 프레임 최대값의 % 단위)
- 공격력 / 마력 / 방어력 / 이동속도 / 쿨다운 감소 / 공격속도 지원
- 방어력은 들어온 데미지를 %로 경감
- 피격 시 화면 흰색 플래시(0.15s) + DamageText 팝업
- 사망 처리: 스프라이트 투명화 → 콜라이더/스크립트 정지 → 사망 애니메이션 프리팹 스폰
- 속도/공격력/방어력 버프를 지속시간 기반으로 적용 가능

### 2.2 원소 시스템 ([PlayerElement.cs](Assets/Scripts/Player/PlayerElement.cs))
- `Wind / Fire / Water / Earth / None` 5종
- `OnElementChanged` 이벤트로 외부에 알림
- 던전 곳곳의 [ElementPickup](Assets/Scripts/Combat/ElementPickup.cs) 충돌 시 자동 변경

### 2.3 스킬 시스템 ([Assets/Scripts/Player/Skill/](Assets/Scripts/Player/Skill/))
- **Dash (C 키)** — 거리/시간 지정, 입력 또는 바라보는 방향, 마나 소비, 쿨다운, 대시 중 일반 컨트롤러 비활성
- **Swiftness (E 키)** — 지정 시간 동안 이동속도 배수 + 오라 이펙트
- **Judgement Smash / Weapon Charge** — 동일 구조의 추가 스킬
- **4원소 Enhancer 패턴** — 각 스킬마다 `<Skill>Fire/Water/Earth/WindEnhancer.cs`가 있어 원소별 특수 효과 프리팹 부착

### 2.4 스킬 데이터 ([SkillData.cs](Assets/Scripts/Core/SkillData.cs))
- ScriptableObject로 스킬 메타데이터 관리 (이름, 아이콘, 쿨다운, 마나, 설명)
- 플래그: `isActive` / `isBuff` / `useCharge` / `useStack`
- 원소 반응(isElementReactive) → 원소별 Enhancer 프리팹 + 텍스트 연결
- 차징 단계 최대 4단계, 단계별 시간 임계값 / 배수
- 스택 시스템 (최대 스택 수, 풀스택 시 DamageBoost/Cleanse/StatBuff 선택)

---

## 3. 전투 시스템

- **데미지 계산** — `PlayerStats.TakeDamage()` / `EnemyHealth.TakeDamage()`가 방어력 경감 후 HP 차감
- **데미지 텍스트** — `DamageText` 프리팹을 popupPoint에서 스폰, 월드스페이스 캔버스 자동 생성
- **히트스탑(0.3s)** — `Rigidbody2D.simulated = false` + `Animator.speed = 0` + 본체 빨간 틴트(or defenceVisual)
- **원소 픽업/효과** — [ElementPickup.cs](Assets/Scripts/Combat/ElementPickup.cs) 충돌로 플레이어 원소 변경

---

## 4. 적 / 보스 시스템

### 4.1 일반 적 ([EnemyAI.cs](Assets/Scripts/Enemy/EnemyAI.cs), [EnemyMover.cs](Assets/Scripts/Enemy/EnemyMover.cs))
- DetectionRange 내 플레이어 감지 → Chase, 범위 밖 → Patrol
- StopDistance 도달 시 정지 → 공격 트리거
- 애니메이션 파라미터: `isWalking / isAttacking / isDead`
- [EnemyData.cs](Assets/Scripts/Enemy/EnemyData.cs) ScriptableObject로 능력치 외부화

### 4.2 적 체력 ([EnemyHealth.cs](Assets/Scripts/Enemy/EnemyHealth.cs))
- 월드 스페이스 HP 슬라이더 (카메라 자동 바인딩)
- 사망 시 `QuestManager.ProgressQuest(MonsterHunt, ...)` 자동 호출
- 페이드아웃 후 파괴 (보스는 시체 유지)

### 4.3 거미 보스 ([SpiderBossController.cs](Assets/Scripts/Enemy/Boss/SpiderBossController.cs))

**1페이즈 — 다리 전투**
- 앞다리 2개: 와이퍼 스윕 (부채꼴 긁기)
- 중간 다리 2개: 수직 Slam (떨림 + 긴 모션)
- 다리는 [SpiderLegHealth.cs](Assets/Scripts/Enemy/Boss/SpiderLegHealth.cs)로 개별 체력 관리
- 본체는 다리 생존 중 무적
- 모든 다리 파괴 시 → `EnterPhase2Routine()` 호출

**공통 패턴 — 화면 가르기 (Screen Slash)**
- 10초 쿨다운, 우선순위 최상위
- 전조 0.5s: 본체 숨김 + 빨간 더미 표시
- 플레이어 주변 4개 광선 프리팹 소환
- 1.5s 후 원상 복구

**2페이즈 — 본체 노출**
- 거미줄 웹 활성화, 보스가 웹 중앙으로 4배속 이동
- 웹 위에서 좌우 랜덤 이동
- 4초 쿨다운 [독침 발사](Assets/Scripts/Enemy/Boss/BossPoisonProjectile.cs)
- 화면 가르기 패턴 유지
- 페이즈 전환 중(`isTransitioning`)에도 무적

**사망 연출** — 본체 + 8개 다리 스프라이트 모두 끄고 죽음 애니메이션 오브젝트 활성화

**다리 절차적 애니메이션** — [ProceduralSpiderLeg.cs](Assets/Scripts/Enemy/Boss/ProceduralSpiderLeg.cs)의 `PerformSweep / PerformSlam`

---

## 5. 퀘스트 / 대화 시스템

### 5.1 [QuestManager.cs](Assets/Scripts/Quest/QuestManager.cs)
- 퀘스트 타입: **ItemCollect / MonsterHunt / ActivateObject / TimeLimit**
- `AddQuest()` — 수락 시 `startCutscene` 자동 재생
- `ProgressQuest()` — 외부 이벤트로 진행도 누적
- `CheckQuestCompletion()` — 목표 달성 시 `midCutscene` / `completeCutscene` 자동 재생
- `autoComplete` 플래그 — 목표 즉시 완료 처리
- `StealItem` 플래그 — 완료 시 요구 아이템 자동 회수
- `CompleteQuest()` — 보상 아이템 자동 지급

### 5.2 [QuestData.cs](Assets/Scripts/Quest/QuestData.cs)
- 목표(Objective): 타입, 대상 ID, 목표량, 현재량
- 보상(Reward): 아이템 + 수량 배열
- Timeline 참조 3종: `startCutscene / midCutscene / completeCutscene`
- 상태 플래그: `isAccepted / isCompleted / isFinished / playedMid`

### 5.3 NPC 대화 (`NPCDialogue` + `DialogueManager`)
- NPC당 여러 퀘스트(questList) 순차 진행
- 상태별 대사: `startLines / processingLines / completedLines / normalLines`
- Space 키로 상호작용, 마지막에 수락/거절 버튼 자동 표시
- NPC 머리 위 아이콘 자동 갱신:
  - 회색 물음표 (시작 가능)
  - 열린 책 (진행 중)
  - 녹색 체크 (완료 가능)
- 대화 종료 후 0.5s 재상호작용 방지

### 5.4 보조 컴포넌트
- [QuestWall.cs](Assets/Scripts/Quest/QuestWall.cs) — 퀘스트 완료 시 벽 사라짐 + 카메라 흔들림 + 추가 대사
- [NPCPresenceController.cs](Assets/Scripts/Quest/NPCPresenceController.cs) — 퀘스트 수락/완료 상태에 따라 NPC 표시 토글
- [QuizNPC.cs](Assets/Scripts/Quest/QuizNPC.cs) — 퀴즈형 상호작용
- [LockedDoor.cs](Assets/Scripts/Quest/LockedDoor.cs) — 키 아이템 필요 문
- [QuestEventTrigger.cs](Assets/Scripts/Quest/QuestEventTrigger.cs) — 트리거 진입 시 퀘스트 이벤트 발생

---

## 6. 인벤토리 / 아이템

- [InventoryManager.cs](Assets/Scripts/Inventory/InventoryManager.cs) — 싱글톤, `AddItem / RemoveItem / GetItemTotalCount`
- [ItemData.cs](Assets/Scripts/Inventory/ItemData.cs) — ScriptableObject (이름/설명/아이콘/스택 가능/최대 스택)
- [ItemInstance.cs](Assets/Scripts/Inventory/ItemInstance.cs) — 런타임 인스턴스
- [ChestInteraction.cs](Assets/Scripts/Inventory/ChestInteraction.cs) — 상자 상호작용
- UI: [InventoryPanel.cs](Assets/Scripts/UI/InventoryPanel.cs) (I 키 토글) + [QuickSlotUI.cs](Assets/Scripts/Inventory/QuickSlotUI.cs)
- 퀘스트의 `ItemCollect` 목표는 `InventoryManager.GetItemTotalCount()`를 폴링

---

## 7. 씬 / 맵 시스템

### 7.1 씬 흐름
- [TitleSceneUI.cs](Assets/Scripts/Core/TitleSceneUI.cs) — 타이틀에서 GameScene으로 전환 (non-additive)
- [SceneFlowManager.cs](Assets/Scripts/Core/SceneFlowManager.cs) — floor01~04 Additive 로드/언로드, `FloorSpawn`에서 플레이어 스폰, N 키로 다음 층(테스트용)
- [ScenePortal.cs](Assets/Scripts/Floor/ScenePortal.cs) — 씬 간 이동 포털 (트리거 진입 또는 Space 키), `PortalSpawnPoint`의 SpawnID로 도착 위치 매칭
- [Portal.cs](Assets/Scripts/Floor/Portal.cs) — 같은 씬 내 순간이동
- [VillageIntroController.cs](Assets/Scripts/Core/VillageIntroController.cs), [InHouseIntroController.cs](Assets/Scripts/Core/InHouseIntroController.cs) — 씬 진입 인트로 컷신 (1회 재생)

### 7.2 Floor 규칙 시스템
- [IFloorRule.cs](Assets/Scripts/Floor/IFloorRule.cs) 인터페이스
- [FloorRuleRunner.cs](Assets/Scripts/Floor/FloorRuleRunner.cs) — 규칙 실행기
- [SurviveTimeRule.cs](Assets/Scripts/Floor/SurviveTimeRule.cs) — 지정 시간 생존
- [FloorManager.cs](Assets/Scripts/Floor/FloorManager.cs) — 층 전체 관리

### 7.3 레이어 / 높이
- [StairLayerFollower.cs](Assets/Scripts/Floor/StairLayerFollower.cs), [LayerTrigger.cs](Assets/Scripts/Floor/LayerTrigger.cs) — 계단 오르내림 시 정렬 순서 자동 변경
- [SimpleLayerSwitcher.cs](Assets/Scripts/Floor/SimpleLayerSwitcher.cs) — 콜라이더 진입 시 레이어 변경
- [PlayerLayerSync.cs](Assets/Scripts/Core/PlayerLayerSync.cs) — 플레이어 레이어 동기화

---

## 8. UI 시스템

- [HUDController.cs](Assets/Scripts/UI/HUDController.cs) + [HUD.cs](Assets/Scripts/UI/HUD.cs) — HP/MP 바, 원소 아이콘 실시간 갱신
- [SkillBarUI.cs](Assets/Scripts/UI/SkillBarUI.cs) + [SkillGaugeUI.cs](Assets/Scripts/UI/SkillGaugeUI.cs) — 스킬 슬롯 쿨다운 시각화
- [GameManager.cs](Assets/Scripts/Core/GameManager.cs) — 패널 스택 관리:
  - **O 키** → 상태창 토글
  - **I 키** → 인벤토리 토글
  - **ESC** → 스택 기반으로 최근 패널부터 닫음, 드롭 팝업 우선 처리
  - 클릭 시 `SetAsLastSibling()`로 자동 최상단
- [RewardSelectUI.cs](Assets/Scripts/UI/RewardSelectUI.cs) — 보상 선택 UI
- [TooltipUI.cs](Assets/Scripts/UI/TooltipUI.cs) — 호버 툴팁
- [QuestLogUI.cs](Assets/Scripts/Quest/QuestLogUI.cs) — 퀘스트 로그 (완료 목표는 녹색, 미완료는 흰색)

---

## 9. 연출 / 카메라

- [CutsceneManager.cs](Assets/Scripts/Core/CutsceneManager.cs) — Timeline 재생, `playedCutsceneIDs`로 1회성 관리, Cinemachine 자동 바인딩, 컷신 중 UI 숨김 + 플레이어 정지
- [TimelineTrigger.cs](Assets/Scripts/Core/TimelineTrigger.cs) — 트리거로 Timeline 발동
- [BossIntroController.cs](Assets/Scripts/Enemy/Boss/BossIntroController.cs) — 보스 등장 시 인트로
- [FadeController.cs](Assets/Scripts/Core/FadeController.cs) — 씬 전환 페이드 인/아웃
- [CameraShake.cs](Assets/Scripts/Effect/CameraShake.cs) — 카메라 흔들림
- [CameraFollow.cs](Assets/Scripts/Camera/CameraFollow.cs) — 플레이어 추적

---

## 10. 저장 시스템

[SaveManager.cs](Assets/Scripts/Core/SaveManager.cs) — 골격만 존재, 진행 상황/인벤토리/퀘스트 상태 저장 예정

---

## 11. 게임 플레이 시나리오

### Phase 1 — 타이틀 / 마을 진입
1. TitleScene에서 시작 → GameScene 로드
2. 마을 NPC들과 Space로 대화 → 퀘스트 수락 (`startCutscene` 자동 재생)

### Phase 2 — 던전 클라이밍 (village~BossRoom)
1. 마을 포털 → `SceneFlowManager.LoadFloorAsync(floorIndex)`
2. 이전 floor 언로드 → 새 floor Additive 로드 → `FloorSpawn`으로 플레이어 이동
3. 던전 내부:
   - 일반 몹: `EnemyAI`가 감지/추적/공격
   - 원소 픽업으로 플레이어 원소 변경 → 스킬 효과 변화
   - 계단으로 다층 구조, `StairLayerTrigger`가 정렬 자동 처리
   - 층별 `IFloorRule` 적용 (생존 시간, 사냥 목표 등)
4. 퀘스트 진행:
   - ItemCollect: `InventoryManager`가 자동 폴링
   - MonsterHunt: `EnemyHealth.Die()`에서 자동 보고
   - autoComplete 플래그 활성 시 즉시 완료 + 보상 지급

### Phase 3 — 보스전 (Boss_Room)
1. 최종 층 클리어 → Boss_Under1/2 → Boss_Room 진입
2. `BossIntroController` 컷신 재생
3. **1페이즈** — 거미 보스의 4개 다리 개별 격파 (앞다리 스윕 / 중간 다리 Slam / 화면 가르기 패턴 대응)
4. 모든 다리 파괴 → `EnterPhase2Routine()`, 거미줄 웹 등장
5. **2페이즈** — 본체 노출, 독침 발사 + 화면 가르기 패턴
6. 본체 처치 → `HandleDeath()` 연출

### Phase 4 — 보스 처치 후
1. BossHouse 또는 BossHouseAfter 씬으로 이동
2. `RewardSelectUI`에서 보상 선택 (또는 자동 지급)
3. 마을 복귀

---

## 12. 핵심 메커니즘 요약 표

| 시스템 | 주요 기능 |
|--------|----------|
| **플레이어** | HP/MP 자동 재생, 방어력 경감, Dash/Swiftness/JudgementSmash/WeaponCharge 스킬, 4원소 강화 |
| **전투** | 데미지 계산, 히트스탑, 데미지 텍스트, 사망 연출 |
| **일반 적** | Patrol/Chase AI, 피격 시각화, 사냥 퀘스트 자동 보고 |
| **거미 보스** | 다리 4개 개별 체력, 1→2 페이즈 전환, 화면 가르기 / 스윕 / Slam / 독침 4종 패턴 |
| **퀘스트** | 4가지 목표 타입, Timeline 자동 재생(시작/중간/완료), autoComplete, 보상 자동 지급 |
| **대화** | NPC당 퀘스트 체인, 상태별 대사 4종, 머리 위 아이콘 자동 토글, 수락/거절 버튼 |
| **인벤토리** | 싱글톤 매니저, ItemData ScriptableObject, 퀘스트 진행도 연동 |
| **UI** | HUD 실시간 바인딩, 패널 스택 ESC 처리, 보상 선택, 툴팁 |
| **씬 관리** | Additive 로딩, PortalSpawnPoint ID 매칭, 인트로 1회성 |
| **컷신** | Timeline + Cinemachine 자동 바인딩, 1회 재생 캐시, 조작 차단 |
| **레벨 디자인** | 4층 던전 + 보스 다단계 + 마을 + 보스집(승리 후) |

---

## 13. 빌드 씬 구성

[EditorBuildSettings](ProjectSettings/EditorBuildSettings.asset)에 등록된 씬:

1. `TitleScene` — 타이틀
2. `GameScene` — 영구 매니저 베이스 (Additive 기준)
3. `village` — 마을
4. `floor01` ~ `floor04` — 테스트장소(디버그용)
5. `Boss` / `Boss_Under1` / `Boss_Under2` / `Boss_Room` — 보스 진입 단계
6. `BossHouse` — 보스 집
7. `InHouse` — 인 하우스

추가 미등록 씬: `BossHouseAfter` (퀘스트 완료 후 분기용)

---

*이 문서는 코드베이스 정적 분석으로 생성됐습니다. 일부 시스템(SaveManager, ElementEffect 등)은 골격만 구현된 상태입니다.*
