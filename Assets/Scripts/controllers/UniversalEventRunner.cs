using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudentTargetMode = EventDefinitionSO.StudentTargetMode;

public class UniversalEventRunner : MonoBehaviour
{
    [Header("Event Pool")]
    public List<EventDefinitionSO> events = new();

    [Header("Spawn")]
    public float intervalSeconds = 3f;
    public float startDelay = 1f;

    [Header("Windup (telegraph)")]
    public bool useWindup = true;
    public float windupSeconds = 0.6f;

    [Header("Students (for Student speaker events)")]
    public StudentActor[] students;

    [Header("Refs")]
    public EventUIManager ui;
    public MaskController mask;
    public SuspicionSystem suspicion;
    public OxygenSystem oxygen;

    [Tooltip("拖同学的 Inventory Provider")]
    public MonoBehaviour inventoryProviderBehaviour;
    private IInventoryProvider inventory;
    private InventorySystem invSystem;
    private bool usedThisEvent;
    private ItemId usedItemThisEvent;

    [Header("Hint (show item icons first time)")]
    public InventoryItemDatabase itemDb;
    private HashSet<string> hintedEventIds = new HashSet<string>();

    [Header("Facing")]
    public FacingSwitcher teacherFacing;

    [Header("Reaction Timing ")]
    public float reactionHoldSeconds = 0.35f;
    public enum DifficultyPhase { Early, Mid, Late }

    [Header("Difficulty Phase")]
    public DifficultyPhase difficulty = DifficultyPhase.Early;

    [Header("Progression")]
    public bool enableProgression = true;

    [Tooltip("每隔多少秒触发一次递进")]
    public float progressionStepSeconds = 40f;

    [Tooltip("事件间隔最小值")]
    public float minIntervalSeconds = 1f;

    [Tooltip("每次减少事件间隔多少秒")]
    public float intervalDecreaseStep = 1f;

    private int progressionStepIndex = 0; //0=减间隔1=升难度2=减间隔3=升难度

    private int sequenceIndex = 0;
    private bool sequenceActive = false;
    private bool sequenceCompleted = false;

    private int lastDiceRoll = 0;

    private void Awake()
    {
        if (ui == null) ui = FindFirstObjectByType<EventUIManager>();
        if (mask == null) mask = FindFirstObjectByType<MaskController>();
        if (suspicion == null) suspicion = FindFirstObjectByType<SuspicionSystem>();
        if (oxygen == null) oxygen = FindFirstObjectByType<OxygenSystem>();

        inventory = inventoryProviderBehaviour as IInventoryProvider;
        invSystem = inventoryProviderBehaviour as InventorySystem;

        ui?.HideAll();
    }

    private void Start()
    {
        StartCoroutine(Loop());

        //难度递进
        if (enableProgression)
            StartCoroutine(ProgressionLoop());
    }

    private IEnumerator Loop()
    {
        yield return new WaitForSeconds(startDelay);

        while (GameManager.I == null || !GameManager.I.IsGameOver)
        {
            yield return new WaitForSeconds(intervalSeconds);

            if (GameManager.I != null && GameManager.I.IsGameOver) yield break;
            if (events == null || events.Count == 0) continue;

            var ev = events[Random.Range(0, events.Count)];
            yield return RunEvent(ev);
        }
    }

    private IEnumerator ProgressionLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (GameManager.I == null || !GameManager.I.IsGameOver)
        {
            yield return new WaitForSeconds(progressionStepSeconds);

            if (GameManager.I != null && GameManager.I.IsGameOver) yield break;

            //减少事件间隔
            if (progressionStepIndex % 2 == 0)
            {
                float before = intervalSeconds;
                intervalSeconds = Mathf.Max(minIntervalSeconds, intervalSeconds - intervalDecreaseStep);
                Debug.Log($"[Progression] Interval {before:0.##} -> {intervalSeconds:0.##}");
            }
            //Early-Mid-Late
            else
            {
                var before = difficulty;

                if (difficulty == DifficultyPhase.Early) difficulty = DifficultyPhase.Mid;
                else if (difficulty == DifficultyPhase.Mid) difficulty = DifficultyPhase.Late;
                // Late stays Late

                Debug.Log($"[Progression] Difficulty {before} -> {difficulty}");
            }

            progressionStepIndex++;
        }
    }

    //使用事件回调
    private void OnUseMainHand(ItemId used)
    {
        usedThisEvent = true;
        usedItemThisEvent = used;
    }

    private IEnumerator RunEvent(EventDefinitionSO ev)
    {
        if (ev == null || ui == null || mask == null) yield break;

        //根据难度选择持续时间
        float duration = GetDurationByDifficulty(ev);

        //按R使用主手
        usedThisEvent = false;
        usedItemThisEvent = ItemId.None;

        //Sequence init
        sequenceIndex = 0;
        sequenceCompleted = false;
        sequenceActive = (ev.requirement != null && ev.requirement.type == RequirementType.Sequence);

        if (invSystem != null) invSystem.OnUseMainHand += OnUseMainHand;

        try
        {
            //选同学
            StudentActor student = null;
            if (ev.speaker == EventSpeaker.Student)
            {
                student = PickStudentForEvent(ev);
                if (student == null)
                {
                    Debug.LogWarning("[EventRunner] No StudentActor available for Student event.");
                    yield break;
                }
            }

            //问号
            if (useWindup && windupSeconds > 0f)
            {
                ui.HideAll();
                ShowQuestionMark(ev.speaker, student);
                yield return new WaitForSeconds(windupSeconds);
                HideQuestionMark(ev.speaker);
            }

            //整理时间线
            TimedLine[] lines = ev.lines ?? new TimedLine[0];
            System.Array.Sort(lines, (a, b) => a.timeOffset.CompareTo(b.timeOffset));

            string initialText = !string.IsNullOrEmpty(ev.displayName) ? ev.displayName : "Event";
            if (lines.Length > 0 && lines[0].timeOffset <= 0f && !string.IsNullOrEmpty(lines[0].text))
                initialText = lines[0].text;

            //显示对话
            ui.HideAll();
            ShowDialogue(ev.speaker, student, initialText);

            bool showHintIcons = ShouldShowHintForEvent(ev);

            if (showHintIcons)
            {
                BuildHintIcons(ev, out Sprite[] hintSprites, out bool[] submitFlags);
                SetHintIcons(ev.speaker, hintSprites, submitFlags);
                MarkHintShown(ev);
            }
            else
            {
                SetHintIcons(ev.speaker, null, null);
            }

            //转身
            FacingSwitcher facing = null;
            if (ev.flipFacingDuringEvent)
            {
                facing = GetFacing(ev.speaker, student);
                if (facing != null) facing.SetFacingFront(false);
            }

            //WatchEvent
            bool isWatchEvent =
                ev.flipFacingDuringEvent &&
                ev.requirement != null &&
                ev.requirement.type == RequirementType.MaskRequired;

            var requiredMask = isWatchEvent
                ? ev.requirement.requiredMask
                : MaskController.MaskType.HumanMask;

            float t = 0f;
            int nextLineIndex = 0;
            if (lines.Length > 0 && lines[0].timeOffset <= 0f)
                nextLineIndex = 1;

            bool success = false;

            while (t < duration)
            {
                //被盯着就必须戴人类面具
                if (ev.flipFacingDuringEvent)
                {
                    if (mask.CurrentMask != MaskController.MaskType.HumanMask)
                    {
                        GameManager.I?.GameOver("Caught without human mask");
                        yield break;
                    }
                }

                if (GameManager.I != null && GameManager.I.IsGameOver) yield break;

                //时间线文字更新
                while (nextLineIndex < lines.Length && t >= lines[nextLineIndex].timeOffset)
                {
                    if (!string.IsNullOrEmpty(lines[nextLineIndex].text))
                        SetDialogueText(ev.speaker, lines[nextLineIndex].text);

                    nextLineIndex++;
                }

                SetDialogueTimer(ev.speaker, 1f - (t / duration));

                if (!isWatchEvent && sequenceActive && sequenceCompleted && ev.requirement != null)
                {
                    if (AreHandsSatisfied(ev.requirement))
                    {
                        success = true;
                        ApplyOutcome(ev.onSuccess);
                        yield return HoldReactionIfAny(ev.speaker, ev.onSuccess);

                        if (facing != null) facing.SetFacingFront(true);
                        HideDialogue(ev.speaker);
                        yield break;
                    }
                }

                //按R使用主手
                if (!isWatchEvent && usedThisEvent)
                {
                    usedThisEvent = false;
                    ItemId used = usedItemThisEvent;
                    usedItemThisEvent = ItemId.None;

                    if (sequenceActive)
                    {
                        var req = ev.requirement;
                        var seq = req.sequence;

                        if (seq == null || seq.Length == 0)
                        {
                            Debug.LogError("[Sequence] sequence is empty.");
                            ApplyOutcome(ev.onFailWrongItem);
                            yield return HoldReactionIfAny(ev.speaker, ev.onFailWrongItem);

                            if (facing != null) facing.SetFacingFront(true);
                            HideDialogue(ev.speaker);
                            yield break;
                        }

                        if (sequenceIndex < 0) sequenceIndex = 0;
                        if (sequenceIndex >= seq.Length) sequenceIndex = seq.Length - 1;

                        ItemId expected = seq[sequenceIndex];

                        bool stepCorrect =
                            (expected == ItemId.None && used == ItemId.None) ||
                            (expected == used);

                        if (!stepCorrect)
                        {
                            ApplyOutcome(ev.onFailWrongItem);
                            yield return HoldReactionIfAny(ev.speaker, ev.onFailWrongItem);

                            if (facing != null) facing.SetFacingFront(true);
                            HideDialogue(ev.speaker);
                            yield break;
                        }

                        sequenceIndex++;
                        SetDialogueText(ev.speaker, $"You've done{sequenceIndex}/{seq.Length}. ");

                        if (sequenceIndex >= seq.Length)
                        {
                            //如果这个sequence还要求两手状态则进入第二阶段等待
                            if (RequiresHandsAfterSequence(req))
                            {
                                sequenceCompleted = true;
                                SetDialogueText(ev.speaker, "Now have your both hands");
                            }
                            else
                            {
                                success = true;
                                ApplyOutcome(ev.onSuccess);
                                yield return HoldReactionIfAny(ev.speaker, ev.onSuccess);

                                if (facing != null) facing.SetFacingFront(true);
                                HideDialogue(ev.speaker);
                                yield break;
                            }
                        }

                    }
                    else
                    {
                        if (IsDiceEvent(ev))
                        {
                            if (used != ItemId.Dice)
                            {
                                ApplyOutcome(ev.onFailWrongItem);
                                yield return HoldReactionIfAny(ev.speaker, ev.onFailWrongItem);

                                if (facing != null) facing.SetFacingFront(true);
                                HideDialogue(ev.speaker);
                                yield break;
                            }

                            int target = ev.requirement.requiredDiceNumber;
                            if (target < 1 || target > 6)
                            {
                                Debug.LogError($"[Dice] requiredDiceNumber 没设置（1~6），event={ev.eventId}");
                                ApplyOutcome(ev.onFailWrongItem);
                                yield return HoldReactionIfAny(ev.speaker, ev.onFailWrongItem);

                                if (facing != null) facing.SetFacingFront(true);
                                HideDialogue(ev.speaker);
                                yield break;
                            }

                            lastDiceRoll = Random.Range(1, 7);
                            Debug.Log($"[Dice] rolled={lastDiceRoll}, target={target}, event={ev.eventId}");

                            if (lastDiceRoll == target)
                            {
                                // 掷对：成功结束
                                SetDialogueText(ev.speaker, $"掷出：{lastDiceRoll} ✓");
                                success = true;
                                ApplyOutcome(ev.onSuccess);
                                yield return HoldReactionIfAny(ev.speaker, ev.onSuccess);

                                if (facing != null) facing.SetFacingFront(true);
                                HideDialogue(ev.speaker);
                                yield break;
                            }
                            else
                            {
                                SetDialogueText(ev.speaker, $"掷出：{lastDiceRoll} ✗ 再试一次");
                            }
                        }
                        else
                        {
                            //其它事件
                            bool correct = IsUsedItemCorrect(ev, used);

                            if (correct)
                            {
                                success = true;
                                ApplyOutcome(ev.onSuccess);
                                yield return HoldReactionIfAny(ev.speaker, ev.onSuccess);

                                if (facing != null) facing.SetFacingFront(true);
                                HideDialogue(ev.speaker);
                                yield break;
                            }
                            else
                            {
                                ApplyOutcome(ev.onFailWrongItem);
                                yield return HoldReactionIfAny(ev.speaker, ev.onFailWrongItem);

                                if (facing != null) facing.SetFacingFront(true);
                                HideDialogue(ev.speaker);
                                yield break;
                            }
                        }
                    }
                }

                if (!isWatchEvent && !sequenceActive && IsRequirementMet(ev))
                {
                    success = true;
                    ApplyOutcome(ev.onSuccess);
                    yield return HoldReactionIfAny(ev.speaker, ev.onSuccess);

                    if (facing != null) facing.SetFacingFront(true);
                    HideDialogue(ev.speaker);
                    yield break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            if (isWatchEvent)
            {
                success = (mask.CurrentMask == requiredMask);

                if (success)
                {
                    ApplyOutcome(ev.onSuccess);
                    yield return HoldReactionIfAny(ev.speaker, ev.onSuccess);
                }
                else
                {
                    ApplyOutcome(ev.onFailTimeout);
                    yield return HoldReactionIfAny(ev.speaker, ev.onFailTimeout);
                }

                if (facing != null) facing.SetFacingFront(true);
                HideDialogue(ev.speaker);
                yield break;
            }

            if (!success)
            {
                ApplyOutcome(ev.onFailTimeout);
                yield return HoldReactionIfAny(ev.speaker, ev.onFailTimeout);
            }

            if (facing != null) facing.SetFacingFront(true);
            HideDialogue(ev.speaker);
        }
        finally
        {
            if (invSystem != null) invSystem.OnUseMainHand -= OnUseMainHand;
        }
    }

    private float GetDurationByDifficulty(EventDefinitionSO ev)
    {
        float d = difficulty switch
        {
            DifficultyPhase.Mid => ev.midDuration,
            DifficultyPhase.Late => ev.lateDuration,
            _ => ev.earlyDuration,
        };
        return Mathf.Max(0.05f, d);
    }

    private StudentActor PickStudentForEvent(EventDefinitionSO ev)
    {
        if (students == null || students.Length == 0) return null;

        if (ev.studentTargetMode == StudentTargetMode.ById && !string.IsNullOrEmpty(ev.studentId))
        {
            foreach (var s in students)
                if (s != null && s.studentId == ev.studentId)
                    return s;

            Debug.LogWarning($"[EventRunner] Student id '{ev.studentId}' not found, fallback random.");
        }

        for (int tries = 0; tries < 10; tries++)
        {
            var s = students[Random.Range(0, students.Length)];
            if (s != null) return s;
        }

        foreach (var s in students)
            if (s != null) return s;

        return null;
    }

    //Dialogue
    private void ShowDialogue(EventSpeaker speaker, StudentActor student, string text)
    {
        if (speaker == EventSpeaker.Teacher) ui.ShowTeacher(text);
        else ui.ShowStudent(student, text);
    }

    private void SetDialogueText(EventSpeaker speaker, string text)
    {
        if (speaker == EventSpeaker.Teacher) ui.SetTeacherText(text);
        else ui.SetStudentText(text);
    }

    private void SetDialogueTimer(EventSpeaker speaker, float t01)
    {
        if (speaker == EventSpeaker.Teacher) ui.SetTeacherTimer01(t01);
        else ui.SetStudentTimer01(t01);
    }

    private void HideDialogue(EventSpeaker speaker)
    {
        if (speaker == EventSpeaker.Teacher) ui.HideTeacher();
        else ui.HideStudentBubble();
    }

    private void ShowQuestionMark(EventSpeaker speaker, StudentActor student)
    {
        if (speaker == EventSpeaker.Teacher) ui.ShowTeacherQuestionMark();
        else ui.ShowStudentQuestionMark(student);
    }

    private void HideQuestionMark(EventSpeaker speaker)
    {
        if (speaker == EventSpeaker.Teacher) ui.HideTeacherQuestionMark();
        else ui.HideStudentQuestionMark();
    }

    private IEnumerator HoldReactionIfAny(EventSpeaker speaker, EventOutcome outcome)
    {
        if (outcome == null || string.IsNullOrEmpty(outcome.reactionText)) yield break;
        SetDialogueText(speaker, outcome.reactionText);
        yield return new WaitForSecondsRealtime(reactionHoldSeconds);
    }

    //Requirement
    private bool IsRequirementMet(EventDefinitionSO ev)
    {
        if (ev.requirement == null) return false;
        var req = ev.requirement;

        switch (req.type)
        {
            case RequirementType.MaskRequired:
                return mask.CurrentMask == req.requiredMask;

            case RequirementType.SingleItem:
                if (inventory == null) return false;
                return inventory.GetSelectedItem() == req.singleItem;

            case RequirementType.TwoHandsExact:
                if (inventory == null) return false;
                return inventory.GetLeftHandItem() == req.leftItem &&
                       inventory.GetRightHandItem() == req.rightItem;

            case RequirementType.Sequence:
                return false;

            default:
                return false;
        }
    }

    //按R使用主手判定
    private bool IsUsedItemCorrect(EventDefinitionSO ev, ItemId used)
    {
        if (ev == null || ev.requirement == null) return false;

        var req = ev.requirement;

        switch (req.type)
        {
            case RequirementType.SingleItem:
                return used == req.singleItem;

            case RequirementType.TwoHandsExact:
                if (inventory == null) return false;
                return inventory.GetLeftHandItem() == req.leftItem &&
                       inventory.GetRightHandItem() == req.rightItem;

            default:
                return false;
        }
    }

    private bool IsDiceEvent(EventDefinitionSO ev)
    {
        return ev != null &&
               ev.requirement != null &&
               ev.requirement.type == RequirementType.SingleItem &&
               ev.requirement.singleItem == ItemId.Dice;
    }

    private bool RequiresHandsAfterSequence(EventRequirement req)
    {
        if (req == null) return false;
        return req.leftItem != ItemId.None || req.rightItem != ItemId.None;
    }

    private bool AreHandsSatisfied(EventRequirement req)
    {
        if (req == null || inventory == null) return false;

        if (req.leftItem != ItemId.None && req.rightItem != ItemId.None)
        {
            return inventory.GetLeftHandItem() == req.leftItem &&
                   inventory.GetRightHandItem() == req.rightItem;
        }

        if (req.leftItem != ItemId.None)
            return inventory.GetLeftHandItem() == req.leftItem || inventory.GetRightHandItem() == req.leftItem;

        if (req.rightItem != ItemId.None)
            return inventory.GetLeftHandItem() == req.rightItem || inventory.GetRightHandItem() == req.rightItem;

        return false;
    }

    //Outcome
    private void ApplyOutcome(EventOutcome o)
    {
        if (o == null) return;

        if (o.suspicionDelta != 0 && suspicion != null)
            suspicion.AddSuspicion(o.suspicionDelta);

        if (Mathf.Abs(o.oxygenPercentDelta) > 0.0001f && oxygen != null)
            oxygen.ApplyOxygenPercentDelta(o.oxygenPercentDelta);

        if (o.instantGameOver)
            GameManager.I?.GameOver("Instant Fail");
    }

    private FacingSwitcher GetFacing(EventSpeaker speaker, StudentActor student)
    {
        if (speaker == EventSpeaker.Teacher) return teacherFacing;
        return student != null ? student.facing : null;
    }

    //Hint system
    private bool ShouldShowHintForEvent(EventDefinitionSO ev)
    {
        if (ev == null) return false;
        string key = !string.IsNullOrEmpty(ev.eventId) ? ev.eventId : ev.name;
        return !hintedEventIds.Contains(key);
    }

    private void MarkHintShown(EventDefinitionSO ev)
    {
        if (ev == null) return;
        string key = !string.IsNullOrEmpty(ev.eventId) ? ev.eventId : ev.name;
        hintedEventIds.Add(key);
    }

    private void BuildHintIcons(EventDefinitionSO ev, out Sprite[] sprites, out bool[] submitFlags)
    {
        sprites = null;
        submitFlags = null;

        if (ev == null || ev.requirement == null || itemDb == null) return;

        var req = ev.requirement;

        List<ItemId> ids = new List<ItemId>();
        List<bool> flags = new List<bool>();

        void Add(ItemId id, bool isSubmit)
        {
            if (id == ItemId.None) return;

            int idx = ids.IndexOf(id);
            if (idx >= 0)
            {
                flags[idx] = flags[idx] || isSubmit;
                return;
            }

            ids.Add(id);
            flags.Add(isSubmit);
        }

        switch (req.type)
        {
            case RequirementType.SingleItem:
               
                
                Add(req.singleItem, true);
                break;


            case RequirementType.TwoHandsExact:
                Add(req.leftItem, false);
                Add(req.rightItem, false);
                break;

            case RequirementType.Sequence:
                if (req.sequence != null)
                {
                    for (int i = 0; i < req.sequence.Length; i++)
                        Add(req.sequence[i], true);
                }

                Add(req.leftItem, false);
                Add(req.rightItem, false);
                break;

            case RequirementType.MaskRequired:
                break;
        }

        if (ids.Count == 0) return;

        sprites = new Sprite[ids.Count];
        submitFlags = new bool[ids.Count];

        for (int i = 0; i < ids.Count; i++)
        {
            sprites[i] = itemDb.GetSprite(ids[i]);
            submitFlags[i] = flags[i];
        }
    }

    private void SetHintIcons(EventSpeaker speaker, Sprite[] sprites, bool[] submitFlags)
    {
        if (ui == null) return;

      
        if (speaker == EventSpeaker.Teacher)
            InvokeSetItemIcons("SetTeacherItemIcons", sprites, submitFlags);
        else
            InvokeSetItemIcons("SetStudentItemIcons", sprites, submitFlags);
    }

    private void InvokeSetItemIcons(string methodName, Sprite[] sprites, bool[] submitFlags)
    {
        var t = ui.GetType();

        var mNew = t.GetMethod(methodName, new System.Type[] { typeof(Sprite[]), typeof(bool[]) });
        if (mNew != null)
        {
            mNew.Invoke(ui, new object[] { sprites, submitFlags });
            return;
        }

        var mOld = t.GetMethod(methodName, new System.Type[] { typeof(Sprite[]) });
        if (mOld != null)
        {
            mOld.Invoke(ui, new object[] { sprites });
            return;
        }

        Debug.LogWarning($"[UniversalEventRunner] EventUIManager missing {methodName}.");
    }
}
