using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GGJ/Inventory Item Database", fileName = "InventoryItemDatabase")]
public class InventoryItemDatabase : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public ItemId id;
        public Sprite sprite;
    }

    public Sprite defaultSprite;
    public List<Entry> entries = new();

    private Dictionary<ItemId, Sprite> cache;

    private void OnEnable()
    {
        cache = new Dictionary<ItemId, Sprite>();
        foreach (var e in entries)
        {
            if (!cache.ContainsKey(e.id))
                cache.Add(e.id, e.sprite);
        }
    }

    public Sprite GetSprite(ItemId id)
    {
        if (id == ItemId.None) return defaultSprite;
        if (cache != null && cache.TryGetValue(id, out var s) && s != null) return s;
        return defaultSprite;
    }
}
