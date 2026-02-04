using UnityEngine;
using TMPro;


public class TimeDisplay : MonoBehaviour
{
    
    public TMP_Text timeText;

    void Start()
    {
        
    }

    void Update()
    {
        if (timeText != null && GameTimer.I != null)
            timeText.text = GameTimer.I.GetFormattedTime();
    }
}
