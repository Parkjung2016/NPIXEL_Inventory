using System;
using System.Collections.Generic;
using System.Linq;
using MemoryPack;
using PJH.Utility.Extensions;
using UnityEngine;

// 기존 InventoryData의 룩업 필드를 캡슐화하고 관리합니다.
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
        // 스택이 꽉 차지 않은 경우만 _stackableLookup에 추가 (새 스택을 찾기 위함)
        if (stackable.StackCount < stackable.MaxStackCount)
        {
            if (!_stackableLookup.TryGetValue(itemID, out var list))
            {
                list = new SortedList<int, IStackable> { { slotIndex, stackable } };
                _stackableLookup[itemID] = list;
            }

            // 안전하게 추가/갱신 (TryAdd 대신 덮어쓰기)
            list[slotIndex] = stackable;
        }
        else
        {
            // 꽉 찼다면 목록에서 제거
            RemoveFromLookup(itemID, slotIndex);
        }

        // 스택 가능 여부와 상관없이 항상 슬롯 인덱스를 매핑
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