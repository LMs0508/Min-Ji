using UnityEngine;
using UnityEngine.Playables; // 타임라인 제어를 위해 필수

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector director; // 타임라인이 붙은 오브젝트 연결

    void Start()
    {
        // 원할 때 재생 (예: 씬 시작 시)
        director.Play();
    }
    
    // 만약 특정 구역에 들어갈 때 재생하고 싶다면?
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            director.Play();
        }
    }
}