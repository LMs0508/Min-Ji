using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic; // 리스트 사용을 위해 추가

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("중앙 저장소 (재생 완료된 ID들)")]
    // 실행된 컷신 ID들이 여기에 자동으로 기록됩니다.
    [SerializeField] private List<string> playedCutsceneIDs = new List<string>();

    [Header("Settings")]
    public GameObject gameplayUIPanel;
    public MonoBehaviour playerMoveScript;
    public Rigidbody2D playerRigidbody;
    public float defaultCameraSize = 8f; // 복구할 기본 카메라 사이즈

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 데이터 유지를 위해 추가
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 컷신이 이미 재생되었는지 확인하는 함수
    public bool CanPlay(string id)
    {
        return !playedCutsceneIDs.Contains(id);
    }

    // [통합] 퀘스트 및 일반 연출 공용 재생 함수
    public void PlayCutscene(PlayableDirector director, PlayableAsset asset, string id)
    {
        if (director == null || asset == null) return;
        
        // 이미 실행된 ID라면 재생하지 않음
        if (!string.IsNullOrEmpty(id) && !CanPlay(id)) return;

        // 실행 기록 추가
        if (!string.IsNullOrEmpty(id)) playedCutsceneIDs.Add(id);

        // 1. UI 및 조작 차단
        SetCutsceneMode(true);

        director.playableAsset = asset;
        
        // 시네머신 트랙 자동 바인딩 로직
        foreach (var output in asset.outputs)
        {
            if (output.sourceObject != null && output.sourceObject.GetType().Name == "CinemachineTrack")
            {
                GameObject mainCam = GameObject.FindWithTag("MainCamera");
                if (mainCam != null) director.SetGenericBinding(output.sourceObject, mainCam);
            }
        }

        director.stopped -= OnCutsceneStopped;
        director.stopped += OnCutsceneStopped;

        director.Play();
    }

    private void OnCutsceneStopped(PlayableDirector director)
    {
        // 2. UI 및 조작 복구
        SetCutsceneMode(false);
        
        // 카메라 복구
        GameObject mainCam = GameObject.FindWithTag("MainCamera");
        if (mainCam != null)
        {
            var brain = mainCam.GetComponent("CinemachineBrain") as MonoBehaviour;
            if (brain != null) { brain.enabled = false; brain.enabled = true; }
            
            var cam = mainCam.GetComponent<Camera>();
            if (cam != null) cam.orthographicSize = defaultCameraSize;
        }

        director.stopped -= OnCutsceneStopped;
    }

    private void SetCutsceneMode(bool isStarting)
    {
        if (gameplayUIPanel != null) gameplayUIPanel.SetActive(!isStarting); //
        
        if (playerMoveScript != null) playerMoveScript.enabled = !isStarting; //
        
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero; //
            playerRigidbody.bodyType = isStarting ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic; //
        }
    }
}