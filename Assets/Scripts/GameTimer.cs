using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer I { get; private set; }

    public float ElapsedSeconds { get; private set; }
    public bool IsRunning { get; private set; }

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!IsRunning) return;

        ElapsedSeconds += Time.unscaledDeltaTime;
    }

    public void ResetTimer()
    {
        ElapsedSeconds = 0f;
    }

    public void StartTimer(bool resetFirst = false)
    {
        if (resetFirst) ResetTimer();
        IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    public string GetFormattedTime()
    {
        int total = Mathf.FloorToInt(ElapsedSeconds);
        int minutes = total / 60;
        int seconds = total % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}