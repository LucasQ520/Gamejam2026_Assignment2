using UnityEngine;

public enum EventSpeaker { Teacher, Student }

public class EventUIManager : MonoBehaviour
{
    [Header("Canvas (optional: will auto-find the correct one)")]
    public Canvas canvas;
    public RectTransform canvasRoot;

    [Header("Teacher Panel (fixed)")]
    public UIEventPanel teacherPanel;

    [Header("Teacher Question Mark (fixed)")]
    public GameObject teacherQuestionMarkRoot;

    [Header("Student Bubble (spawn + follow)")]
    public UIEventPanel studentBubblePrefab;
    public Vector2 studentBubbleOffsetPx = Vector2.zero;

    [Header("Student Question Mark (spawn + follow)")]
    public UIFollowMarker studentQuestionMarkPrefab;
    public Vector2 studentQuestionMarkOffsetPx = Vector2.zero;

    [Header("Clamp / Debug")]
    public float clampMarginPx = 10f;
    public bool freezeFollow = false;
    public bool debugLogPositions = false;
    
    
    [Header("Camera")]
    public Camera gameplayCamera; 
    public bool debugScreenPos = false;
    public bool disableClamp = false;


    private UIEventPanel activeStudentBubble;
    private StudentActor activeStudentBubbleTarget;

    private UIFollowMarker activeStudentMark;
    private StudentActor activeStudentMarkTarget;

    private void Awake()
    {
        EnsureCanvasRefs();
        HideAll();
    }

    private void LateUpdate()
    {
        if (freezeFollow) return;

        if (activeStudentBubble != null && activeStudentBubbleTarget != null)
        {
            var rect = activeStudentBubble.GetComponent<RectTransform>();
            UpdateRectToWorldAnchor_ScreenClamp(rect, GetAnchor(activeStudentBubbleTarget), studentBubbleOffsetPx);

            if (debugLogPositions)
                Debug.Log($"[Bubble] anchored={rect.anchoredPosition} target={activeStudentBubbleTarget.name}");
        }

        if (activeStudentMark != null && activeStudentMarkTarget != null)
        {
            var rect = activeStudentMark.GetComponent<RectTransform>();
            UpdateRectToWorldAnchor_ScreenClamp(rect, GetAnchor(activeStudentMarkTarget), studentQuestionMarkOffsetPx);

            if (debugLogPositions)
                Debug.Log($"[Mark] anchored={rect.anchoredPosition} target={activeStudentMarkTarget.name}");
        }
    }

    public void HideAll()
    {
        HideTeacher();
        HideTeacherQuestionMark();
        HideStudentBubble();
        HideStudentQuestionMark();
    }

    public void ShowTeacher(string text)
    {
        HideAll();
        teacherPanel?.Show(text);
    }

    public void SetTeacherText(string text) => teacherPanel?.SetText(text);
    public void SetTeacherTimer01(float t01) => teacherPanel?.SetTimer01(t01);
    public void HideTeacher() => teacherPanel?.Hide();

    public void ShowTeacherQuestionMark()
    {
        if (teacherQuestionMarkRoot != null)
            teacherQuestionMarkRoot.SetActive(true);
    }

    public void HideTeacherQuestionMark()
    {
        if (teacherQuestionMarkRoot != null)
            teacherQuestionMarkRoot.SetActive(false);
    }

    public void ShowStudent(StudentActor who, string text)
    {
        HideAll();
        EnsureCanvasRefs();

        if (who == null || studentBubblePrefab == null || canvasRoot == null)
        {
            Debug.LogWarning("[EventUIManager] ShowStudent missing refs.");
            return;
        }

        activeStudentBubbleTarget = who;

        activeStudentBubble = Instantiate(studentBubblePrefab, canvasRoot);
        var rect = activeStudentBubble.GetComponent<RectTransform>();
        PrepareFloatingRect(rect);

        activeStudentBubble.Show(text);

        UpdateRectToWorldAnchor_ScreenClamp(rect, GetAnchor(who), studentBubbleOffsetPx);
    }

    public void SetStudentText(string text) => activeStudentBubble?.SetText(text);
    public void SetStudentTimer01(float t01) => activeStudentBubble?.SetTimer01(t01);

    public void SetTeacherItemIcons(Sprite[] sprites)
    {
        teacherPanel?.SetItemIcons(sprites);
    }

    public void SetStudentItemIcons(Sprite[] sprites)
    {
        activeStudentBubble?.SetItemIcons(sprites);
    }
    
    public void SetTeacherItemIcons(Sprite[] sprites, bool[] submitFlags)
    {
        teacherPanel?.SetItemIcons(sprites, submitFlags);
    }

    public void SetStudentItemIcons(Sprite[] sprites, bool[] submitFlags)
    {
        activeStudentBubble?.SetItemIcons(sprites, submitFlags);
    }

    public void HideStudentBubble()
    {
        if (activeStudentBubble != null)
        {
            Destroy(activeStudentBubble.gameObject);
            activeStudentBubble = null;
        }
        activeStudentBubbleTarget = null;
    }

    public void ShowStudentQuestionMark(StudentActor who)
    {
        HideStudentQuestionMark();
        EnsureCanvasRefs();

        if (who == null || studentQuestionMarkPrefab == null || canvasRoot == null)
        {
            Debug.LogWarning("[EventUIManager] ShowStudentQuestionMark missing refs.");
            return;
        }

        activeStudentMarkTarget = who;

        activeStudentMark = Instantiate(studentQuestionMarkPrefab, canvasRoot);
        var rect = activeStudentMark.GetComponent<RectTransform>();
        PrepareFloatingRect(rect);

        UpdateRectToWorldAnchor_ScreenClamp(rect, GetAnchor(who), studentQuestionMarkOffsetPx);
    }

    public void HideStudentQuestionMark()
    {
        if (activeStudentMark != null)
        {
            Destroy(activeStudentMark.gameObject);
            activeStudentMark = null;
        }
        activeStudentMarkTarget = null;
    }

    private void EnsureCanvasRefs()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvasRoot == null && canvas != null)
            canvasRoot = canvas.GetComponent<RectTransform>();
    }

    private Transform GetAnchor(StudentActor who)
    {
        if (who == null) return null;
        return (who.bubbleAnchor != null) ? who.bubbleAnchor : who.transform;
    }

    private void PrepareFloatingRect(RectTransform rect)
    {
        if (rect == null) return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
    }

    private void UpdateRectToWorldAnchor_ScreenClamp(RectTransform rect, Transform worldAnchor, Vector2 screenOffsetPx)
    {
        if (rect == null || worldAnchor == null) return;
        if (canvasRoot == null) return;

        Camera worldCam = gameplayCamera != null ? gameplayCamera : Camera.main;

        Vector3 sp3 = RectTransformUtility.WorldToScreenPoint(worldCam, worldAnchor.position);
        if (sp3.z < 0f)
        {
            rect.anchoredPosition = new Vector2(99999, 99999);
            return;
        }

        Vector2 sp = new Vector2(sp3.x, sp3.y) + screenOffsetPx;

        if (debugScreenPos)
            Debug.Log($"[UIFollow] {worldAnchor.name} screen={sp} (raw={sp3}) size={Screen.width}x{Screen.height}");

        if (!disableClamp)
        {
            float scaleFactor = (canvas != null) ? canvas.scaleFactor : 1f;
            float halfWpx = rect.rect.width * 0.5f * scaleFactor;
            float halfHpx = rect.rect.height * 0.5f * scaleFactor;
            float margin = Mathf.Max(0f, clampMarginPx);

            sp.x = Mathf.Clamp(sp.x, margin + halfWpx, Screen.width  - margin - halfWpx);
            sp.y = Mathf.Clamp(sp.y, margin + halfHpx, Screen.height - margin - halfHpx);
        }

        Camera uiCam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCam = canvas.worldCamera;
            if (uiCam == null) uiCam = worldCam;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRoot, sp, uiCam, out Vector2 localPoint);
        rect.anchoredPosition = localPoint;
    }


}
