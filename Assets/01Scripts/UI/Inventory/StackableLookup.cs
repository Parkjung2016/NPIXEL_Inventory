using System;
using System.Collections.Generic;
using System.Linq;
using MemoryPack;
using PJH.Utility.Extensions;
using UnityEngine;

// ���� InventoryData�� ��� �ʵ带 ĸ��ȭ�ϰ� �����մϴ�.
[MemoryPackable]
[Serializable]
public partial class StackableLookup
{
    // itemID -> slotIndex -> IStackable
    private Dictionary<int, SortedList<int, IStackable>> _stackableLookup = new();

    // IStackable -> slotIndex
    private Dictionary<IStackable, int> _stackableToSlotIndex = new();

    public IStackable FindStackable(int itemID)
    {
        if (_stackableLookup.TryGetValue(itemID, out var list))
        {
            return list.FirstOrDefault().Value;
        }

        return null;
    }

    public void Add(int itemID, int slotIndex, IStackable stackable)
    {
        // ������ �� ���� ���� ��츸 _stackableLookup�� �߰� (�� ������ ã�� ����)
        if (stackable.StackCount < stackable.MaxStackCount)
        {
            if (!_stackableLookup.TryGetValue(itemID, out var list))
            {
                list = new SortedList<int, IStackable> { { slotIndex, stackable } };
                _stackableLookup[itemID] = list;
            }

            // �����ϰ� �߰�/���� (TryAdd ��� �����)
            list[slotIndex] = stackable;
        }
        else
        {
            // �� á�ٸ� ��Ͽ��� ����
            RemoveFromLookup(itemID, slotIndex);
        }

        // ���� ���� ���ο� ������� �׻� ���� �ε����� ����
        _stackableToSlotIndex[stackable] = slotIndex;
    }

    public void Remove(IStackable stackable)
    {
        if (_stackableToSlotIndex.TryGetValue(stackable, out int slotIndex))
        {
            RemoveFromLookup(stackable.ItemID, slotIndex);
            _stackableToSlotIndex.Remove(stackable);
        }
    }

    public void RemoveFromLookup(int itemID, int slotIndex)
    {
        if (!_stackableLookup.ContainsKey(itemID)) return;
        _stackableLookup[itemID].Remove(slotIndex);
        if (_stackableLookup[itemID].Count == 0)
        {
            _stackableLookup.Remove(itemID);
        }
    }

    public void Clear()
    {
        _stackableLookup.Clear();
        _stackableToSlotIndex.Clear();
    }

    public bool TryGetSlotIndex(IStackable stackable, out int index)
    {
        return _stackableToSlotIndex.TryGetValue(stackable, out index);
    }

    public void Refresh(List<ItemDataBase> currentInventoryDataList)
    {
        Clear();
        for (int i = 0; i < currentInventoryDataList.Count; i++)
        {
            if (currentInventoryDataList[i] is IStackable stackable)
            {
                Add(currentInventoryDataList[i].ItemID, i, stackable);
            }
        }
    }
}