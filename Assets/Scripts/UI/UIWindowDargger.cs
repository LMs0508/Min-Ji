using UnityEngine;
using UnityEngine.EventSystems;

public class UIWindowDragger : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private string saveKey;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // 이 오브젝트의 이름으로 저장 키를 생성 (여러 창이 있을 경우 대비)
        saveKey = "WindowPos_" + gameObject.name;

        // 게임 시작 시 저장된 위치 불러오기
        LoadPosition();
    }

    // 드래그 시작 시 (레이캐스트 타겟 확인용)
    public void OnBeginDrag(PointerEventData eventData) { }

    // 드래그 중일 때 실행
    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 이동량만큼 UI 위치 이동
        // 캔버스의 스케일에 맞춰 delta 값을 조정하여 이동시킵니다.
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        ClampToScreen();
    }

    // 창이 꺼질 때나 파괴될 때 위치 저장
    private void OnDisable()
    {
        SavePosition();
    }

    public void SavePosition()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        PlayerPrefs.SetFloat(saveKey + "_X", pos.x);
        PlayerPrefs.SetFloat(saveKey + "_Y", pos.y);
        PlayerPrefs.Save();
        Debug.Log($"{gameObject.name} 위치 저장 완료: {pos}");
    }

    public void LoadPosition()
    {
        // 저장된 기록이 있을 때만 위치 이동
        if (PlayerPrefs.HasKey(saveKey + "_X"))
        {
            float x = PlayerPrefs.GetFloat(saveKey + "_X");
            float y = PlayerPrefs.GetFloat(saveKey + "_Y");
            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
    private void ClampToScreen()
    {
        // 캔버스 크기 가져오기 (Screen 공간)
        Vector3[] canvasCorners = new Vector3[4];
        ((RectTransform)canvas.transform).GetWorldCorners(canvasCorners);

        // 내 UI 크기 가져오기
        Vector3[] windowCorners = new Vector3[4];
        rectTransform.GetWorldCorners(windowCorners);

        // 현재 위치와 화면 경계 비교
        Vector2 currentPos = rectTransform.position;
        
        // 왼쪽/오른쪽 제한
        if (windowCorners[0].x < canvasCorners[0].x) // 왼쪽 밖
            currentPos.x += canvasCorners[0].x - windowCorners[0].x;
        if (windowCorners[2].x > canvasCorners[2].x) // 오른쪽 밖
            currentPos.x -= windowCorners[2].x - canvasCorners[2].x;

        // 위/아래 제한
        if (windowCorners[0].y < canvasCorners[0].y) // 아래 밖
            currentPos.y += canvasCorners[0].y - windowCorners[0].y;
        if (windowCorners[1].y > canvasCorners[1].y) // 위 밖
            currentPos.y -= windowCorners[1].y - canvasCorners[1].y;

        rectTransform.position = currentPos;
    }
}