using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
    }

    public void GameOver(string reason)
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Debug.Log("GAME OVER: " + reason);

        Time.timeScale = 0f;

       
        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.ShowGameOver(reason);
    }

    
    private void Update()
    {
        if (IsGameOver)
        {
            GameTimer.I?.StopTimer();
            Time.timeScale = 1f; 
            UnityEngine.SceneManagement.SceneManager.LoadScene("endScene");
            
        }
    }
}