using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneButtons : MonoBehaviour
{
    [Header("Scene Names")]
    public string startSceneName = "StartScene";

   
    public void GoToStartScene()
    {
        Time.timeScale = 1f; 
        GameTimer.I?.ResetTimer(); 
        SceneManager.LoadScene(startSceneName);
    }

   
    public void QuitGame()
    {
        
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}