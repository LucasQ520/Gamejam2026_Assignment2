using UnityEngine;
using TMPro;

public class EndSceneTimeDisplay : MonoBehaviour
{
    public TMP_Text timeText;

    private void Start()
    {
        
        Debug.Log("time"+GameTimer.I.GetFormattedTime());
        Time.timeScale = 1f; 
        if (timeText != null && GameTimer.I != null)
            timeText.text = GameTimer.I.GetFormattedTime();
    }
}

