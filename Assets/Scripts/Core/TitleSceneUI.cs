using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneUI : MonoBehaviour
{
    [Header("로드할 씬")]
    public string gameSceneName = "GameScene";

    public void OnClickStart()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
