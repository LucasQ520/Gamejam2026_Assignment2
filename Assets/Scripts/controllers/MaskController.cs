using UnityEngine;

public class MaskController : MonoBehaviour
{
    public enum MaskType { BreathMask, HumanMask }
    public MaskType CurrentMask { get; private set; } = MaskType.BreathMask;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.Space;

    [Header("Visual")]
    public GameObject breathMaskObj; 
    public GameObject humanMaskObj;  

    private void Start()
    {
        ApplyVisual();
        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.SetMask(CurrentMask);
    }

    private void Update()
    {
        if (!GameGate.GameplayEnabled) return; 
        if (GameManager.I != null && GameManager.I.IsGameOver) return;

        if (Input.GetKeyDown(toggleKey))
            ToggleMask();
    }

    private void ToggleMask()
    {
        CurrentMask = (CurrentMask == MaskType.BreathMask) ? MaskType.HumanMask : MaskType.BreathMask;
        ApplyVisual();

        var ui = FindFirstObjectByType<UIController>();
        if (ui != null) ui.SetMask(CurrentMask);
    }

    private void ApplyVisual()
    {
        if (breathMaskObj != null) breathMaskObj.SetActive(CurrentMask == MaskType.BreathMask);
        if (humanMaskObj != null) humanMaskObj.SetActive(CurrentMask == MaskType.HumanMask);
    }

    public bool IsWearingBreathMask() => CurrentMask == MaskType.BreathMask;
    public bool IsWearingHumanMask()  => CurrentMask == MaskType.HumanMask;
}