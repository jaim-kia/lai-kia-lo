using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    [SerializeField] private ItemData[] allItems; // every item in the game, for lookup by id
    [SerializeField] private int charmSlots = 3;

    private readonly List<ItemData> order = new List<ItemData>(); // pickup order
    private readonly Dictionary<ItemData, int> counts = new Dictionary<ItemData, int>();
    private readonly HashSet<CharmData> equipped = new HashSet<CharmData>();
    private readonly Dictionary<string, ItemData> byId = new Dictionary<string, ItemData>();

    public static event Action OnChanged;                    // for a coin/incense HUD
    public static event Action<CharmData, bool> OnCharmEquipChanged;

    public int CharmSlots => charmSlots;
    public int EquippedCount => equipped.Count;

    private void Awake()
    {
        Instance = this;

        foreach (var item in allItems)
        {
            if (item == null) continue;
            if (!byId.TryAdd(item.Id, item))
                Debug.LogError($"Duplicate item id '{item.Id}' in Inventory.allItems", this);
        }
    }

    public ItemData GetItem(string id) => byId.TryGetValue(id, out var item) ? item : null;

    public int Count(ItemData item) => item != null && counts.TryGetValue(item, out int c) ? c : 0;

    public bool Has(ItemData item, int amount = 1) => Count(item) >= amount;

    // returns false if nothing could be added (unique item already owned, or stack full); excess is clamped
    public bool Add(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int current = Count(item);
        int space = item.MaxStack - current;
        if (space <= 0) return false;

        if (current == 0) order.Add(item);
        counts[item] = current + Mathf.Min(amount, space);

        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(ItemData item, int amount = 1)
    {
        if (!Has(item, amount)) return false;

        int left = Count(item) - amount;
        if (left == 0)
        {
            counts.Remove(item);
            order.Remove(item);

            if (item is CharmData charm && equipped.Remove(charm))
                OnCharmEquipChanged?.Invoke(charm, false);
        }
        else
        {
            counts[item] = left;
        }

        OnChanged?.Invoke();
        return true;
    }

    public List<ItemData> GetItems(ItemCategory category)
    {
        var list = new List<ItemData>();
        foreach (var item in order)
            if (item.Category == category) list.Add(item);
        return list;
    }

    public bool IsEquipped(CharmData charm) => equipped.Contains(charm);

    // equips, or unequips if already equipped. Returns false if it couldn't (not owned, or slots full)
    public bool TryToggleEquip(CharmData charm)
    {
        if (!Has(charm)) return false;

        if (equipped.Remove(charm))
        {
            OnCharmEquipChanged?.Invoke(charm, false);
            return true;
        }

        if (equipped.Count >= charmSlots) return false;

        equipped.Add(charm);
        OnCharmEquipChanged?.Invoke(charm, true);
        return true;
    }
}