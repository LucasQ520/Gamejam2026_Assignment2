using System;
using UnityEngine;

[Serializable]
public struct TimedLine
{
    public float timeOffset;
    [TextArea] public string text;
}