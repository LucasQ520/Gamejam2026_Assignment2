using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour, IInventoryProvider
{
    public event Action<ItemId> OnUseMainHand;

    [Header("Item Pool (single deck / list)")]
    public List<ItemId> pool = new();
    public bool shuffleOnStart = false;

    [Header("Hands (UI)")]
    [SerializeField] private ItemId mainHand = ItemId.None;
    [SerializeField] private ItemId offHand  = ItemId.None;

    [Header("Use")]
    public KeyCode useKey = KeyCode.R;

    private ItemId selectedItem = ItemId.None;
    private float selectedItemClearTimer = 0f;
    public float selectedHoldSeconds = 0.8f;

    [Header("UI Sprites")]
    public Image mainHandImage;
    public Image offHandImage;
    public InventoryItemDatabase itemDb;

    [Header("Consume rules")]
    public bool consumeMostItemsOnUse = true;
    public bool doNotConsumeBookLikeItems = true;

    private int offIndex = -1;

    private void Start()
    {
        if (shuffleOnStart) Shuffle(pool);

        if (pool != null && pool.Count > 0)
        {
            offIndex = 0;
            offHand = pool[offIndex];
        }

        RefreshUI();
    }

    private void Update()
    {
        if (GameManager.I != null && GameManager.I.IsGameOver) return;

        if (selectedItem != ItemId.None)
        {
            selectedItemClearTimer -= Time.deltaTime;
            if (selectedItemClearTimer <= 0f)
                selectedItem = ItemId.None;
        }

        if (Input.GetMouseButtonDown(0))
            CycleOffHand();

        if (Input.GetMouseButtonDown(1))
            EquipOffToMain();

        if (Input.GetKeyDown(useKey))
            UseMainHand();
    }

    public ItemId GetLeftHandItem()  => offHand;
    public ItemId GetRightHandItem() => mainHand;
    public ItemId GetSelectedItem()  => selectedItem;

    private void CycleOffHand()
    {
        if (pool == null || pool.Count == 0) return;

        offIndex++;
        if (offIndex >= pool.Count) offIndex = 0;

        offHand = pool[offIndex];
        RefreshUI();
    }

    private void EquipOffToMain()
    {
        mainHand = offHand;
        RefreshUI();
    }

    private void UseMainHand()
    {
        if (mainHand == ItemId.None) return;

        ItemId used = mainHand;

        OnUseMainHand?.Invoke(used);

        selectedItem = used;
        selectedItemClearTimer = selectedHoldSeconds;

        if (ShouldConsumeOnUse(used))
        {
            mainHand = ItemId.None;
            RefreshUI();
        }
        else
        {
            RefreshUI();
        }
    }

    private bool ShouldConsumeOnUse(ItemId id)
    {
        if (!consumeMostItemsOnUse) return false;

        if (doNotConsumeBookLikeItems)
        {
            if (id == ItemId.Book || id == ItemId.HomeworkBook)
                return false;
        }

        return true;
    }

    private void RefreshUI()
    {
        if (itemDb == null) return;

        if (mainHandImage != null)
            mainHandImage.sprite = itemDb.GetSprite(mainHand);

        if (offHandImage != null)
            offHandImage.sprite = itemDb.GetSprite(offHand);
    }

    private void Shuffle(List<ItemId> list)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
