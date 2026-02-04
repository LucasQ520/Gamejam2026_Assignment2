using UnityEngine;

public class SuspicionSystem : MonoBehaviour
{
    public int suspicion { get; private set; } = 0;
    public int maxSuspicion = 4;

    private void Start()
    {
        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.SetSuspicion(suspicion, maxSuspicion);
    }

    private void Update()
    {
        if (GameManager.I != null && GameManager.I.IsGameOver) return;
    }

    public void AddSuspicion(int amount)
    {
        if (GameManager.I != null && GameManager.I.IsGameOver) return;

        suspicion += amount;
        suspicion = Mathf.Clamp(suspicion, 0, maxSuspicion);

        Debug.Log("Suspicion: " + suspicion);

        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.SetSuspicion(suspicion, maxSuspicion);

        if (suspicion >= maxSuspicion)
        {
            GameManager.I.GameOver("Suspicion Maxed");
        }
    }
}