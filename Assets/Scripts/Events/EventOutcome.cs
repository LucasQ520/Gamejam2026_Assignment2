using System;
using UnityEngine;

[Serializable]
public class EventOutcome
{
    public int suspicionDelta = 0;          // 失败-1
    public float oxygenPercentDelta = 0f;   // -0.2f = 20%
    
    public bool instantGameOver = false; //直接死
    
    [TextArea] public string reactionText;  
}