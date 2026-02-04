using UnityEngine;

public class MainSceneBootstrap : MonoBehaviour
{
    private void Start()
    {
        if (GameTimer.I != null)
            GameTimer.I.StartTimer(resetFirst: true); 
    }
}