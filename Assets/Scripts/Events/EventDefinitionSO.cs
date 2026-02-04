using UnityEngine;

[CreateAssetMenu(menuName = "GGJ/Event Definition", fileName = "Event_")]
public class EventDefinitionSO : ScriptableObject
{
    [Header("Meta")]
    public string eventId;
    public string displayName;

    [Header("Duration (Early / Mid / Late)")]
    public float earlyDuration = 10f;
    public float midDuration = 8f;
    public float lateDuration = 6f;
    
    [Header("Dialogue (T0, T+N...)")]
    public TimedLine[] lines;

    [Header("Requirement")]
    public EventRequirement requirement;

    [Header("Outcomes")]
    public EventOutcome onSuccess;
    public EventOutcome onFailTimeout;
    public EventOutcome onFailWrongItem;

   
    public EventOutcome onFailNoHumanMask;

    public EventSpeaker speaker = EventSpeaker.Teacher;
    public bool flipFacingDuringEvent = false;

    public enum StudentTargetMode { Random, ById }

    [Header("Student Target")]
    public StudentTargetMode studentTargetMode = StudentTargetMode.Random;
    public string studentId;


    private void OnValidate()
    {
        onSuccess ??= new EventOutcome();
        onFailTimeout ??= new EventOutcome();
        onFailWrongItem ??= new EventOutcome();
        onFailNoHumanMask ??= new EventOutcome();
    }
}