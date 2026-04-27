using UnityEngine;
using UnityEngine.Playables;

public class SceneOnceTrigger : MonoBehaviour
{
    [Header("이 컷신의 고유 이름 (중복 금지)")]
    public string cutsceneID; 

    public PlayableDirector director;
    public PlayableAsset timelineAsset;

    void Start()
    {
        if (CutsceneManager.Instance != null)
        {
            if (CutsceneManager.Instance.CanPlay(cutsceneID))
            {
                // 아직 재생 안 됐으면 실행
                CutsceneManager.Instance.PlayCutscene(director, timelineAsset, cutsceneID);
            }
            else
            {
                // 이미 재생됐으면 오브젝트 끄기
                gameObject.SetActive(false);
            }
        }
    }
}