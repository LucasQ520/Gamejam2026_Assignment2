using UnityEngine;

public class UIFollowMarker : MonoBehaviour
{
    public RectTransform rect;

    private void Awake()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
    }
}

