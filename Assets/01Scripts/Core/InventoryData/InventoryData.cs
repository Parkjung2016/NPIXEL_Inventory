using System;
using System.Collections.Generic;
using MemoryPack;
using Priority_Queue;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;
using PJH.Utility.Extensions;
using PJH.Utility;
using Reflex.Extensions;

public delegate void ChangedInventoryDataEvent(List<ItemDataBase> inventoryDataList, bool resetData);

[MemoryPackable]
[Serializable]
public partial class InventoryData
{
    [MemoryPackIgnore] public Action OnUpdateItemCount;
    [MemoryPackIgnore] public Action OnChangedCurrentInventorySlotCount;
    [MemoryPackIgnore] public ChangedInventoryDataEvent OnChangedInventoryData;

    [MemoryPackIgnore, HideInInspector] public bool canSettingData = true;

    public int inventorySlotCapacity = 10;
    public Define.InventorySortType sortType;
    public bool canAutoSort = true;

    [HideInInspector] public int currentInventorySlotCount;

    [MemoryPackIgnore]
    public int CurrentInventorySlotCount
    {
        get => currentInventorySlotCount;
        private set
        {
            currentInventorySlotCount = value;
            OnChangedCurrentInventorySlotCount?.Invoke();
        }
    }

    public List<ItemDataBase> currentInventoryDataList = new();

    private readonly StackableLookup _stackableLookup = new();

    private readonly SimplePriorityQueue<int> _emptySlotPriorityQueue = new();

    public ItemDataBase GetItemDataAt(int index)
    {
        if (index < 0 || index >= currentInventoryDataList.Count)
        {
            PJHDebug.LogWarning($"Index {index} is out of range. Cannot get item.", tag: "InventorySO");
            return null;
        }

        return currentInventoryDataList[index];
    }

    public int GetItemDataIndex(ItemDataBase itemData)
    {
        return currentInventoryDataList.FindIndex(x => x == itemData);
    }

    public bool IsFull()
    {
        return CurrentInventorySlotCount >= inventorySlotCapacity;
    }

    public void ClearData()
    {
        if (!canSettingData) return;
        currentInventoryDataList.Clear();
        _emptySlotPriorityQueue.Clear();
        _stackableLookup.Clear();
        CurrentInventorySlotCount = 0;

        for (int i = 0; i < inventorySlotCapacity; i++)
        {
            currentInventoryDataList.Add(null);
            _emptySlotPriorityQueue.Enqueue(i, i);
        }
    }

    public void AddInventorySlotCapacity(int countToAdd)
    {
        if (!canSettingData || countToAdd <= 0) return;

        int index = currentInventoryDataList.Count;
        for (int i = 0; i < countToAdd; i++)
        {
            currentInventoryDataList.Add(null);
            _emptySlotPriorityQueue.Enqueue(index, index);
            ++index;
        }

        inventorySlotCapacity += countToAdd;
        OnUpdateItemCount?.Invoke();
    }

    public ItemDataBase AddItem(ItemDataBase itemData)
    {
        if (!canSettingData) return null;
        return AddItemInternal(itemData);
    }

    public void AddItems(IList<ItemDataBase> items)
    {
        if (!canSettingData) return;

        int emptyAddedItemCount = 0;
        for (int j = 0; j < items.Count; j++)
        {
            ItemDataBase itemData = AddItemInternal(items[j], invokeEvent: false);
            if (itemData == null)
                emptyAddedItemCount++;
        }

        if (emptyAddedItemCount < items.Count && canAutoSort)
        {
            SortData();
        }

        OnChangedCurrentInventorySlotCount?.Invoke();
        OnChangedInventoryData?.Invoke(currentInventoryDataList, true);
    }

    private ItemDataBase AddItemInternal(ItemDataBase itemData, bool invokeEvent = true)
    {
        IStackable stackableCandidate = itemData as IStackable;

        if (stackableCandidate != null)
        {
            IStackable existingStack = _stackableLookup.FindStackable(itemData.ItemID);
            if (existingStack != null)
            {
                existingStack.StackCount++;
                if (existingStack.StackCount >= existingStack.MaxStackCount &&
                    _stackableLookup.TryGetSlotIndex(existingStack, out int slotIndex))
                {
                    _stackableLookup.RemoveFromLookup(itemData.ItemID, slotIndex);
                }

                if (invokeEvent)
                {
                    if (canAutoSort) SortData();
                    OnChangedInventoryData?.Invoke(currentInventoryDataList, false);
                }

                return null;
            }
        }

        if (IsFull())
        {
            return null;
        }

        int emptySlotIndex = FindEmptySlotIndex();
        if (emptySlotIndex < 0) return null;

        ItemDataBase dataInstance = itemData.Clone();
        if (Application.isPlaying)
            AttributeInjector.Inject(dataInstance, SceneManager.GetActiveScene().GetSceneContainer());
        dataInstance.uniqueID = Guid.NewGuid();

        if (dataInstance is IStackable newStackable)
        {
            newStackable.StackCount = 1;
            _stackableLookup.Add(dataInstance.ItemID, emptySlotIndex, newStackable);
        }

        currentInventoryDataList[emptySlotIndex] = dataInstance;
        if (invokeEvent)
            CurrentInventorySlotCount++;
        else
            currentInventorySlotCount++;

        if (invokeEvent)
        {
            if (canAutoSort) SortData();
            OnChangedInventoryData?.Invoke(currentInventoryDataList, false);
        }

        return dataInstance;
    }

    public void ChangeItemDataIndex(ItemDataBase itemData, int prevIndex, int newIndex)
    {
        if (!canSettingData) return;
        ItemDataBase targetItemData = currentInventoryDataList[newIndex];

        // 1. 스택 합치기 시도
        if (targetItemData != null && itemData.ItemID == targetItemData.ItemID)
        {
            if (targetItemData is IStackable targetStackable && itemData is IStackable sourceStackable)
            {
                if (targetStackable.StackCount + sourceStackable.StackCount <= targetStackable.MaxStackCount)
                {
                    targetStackable.StackCount += sourceStackable.StackCount;

                    RemoveItemAt(prevIndex);

                    _stackableLookup.Add(targetItemData.ItemID, newIndex, targetStackable);
                    return;
                }
            }
        }

        ItemDataBase sourceItemData = itemData;

        currentInventoryDataList[newIndex] = sourceItemData;
        currentInventoryDataList[prevIndex] = targetItemData;

        UpdateStackableLookupAfterSwap(sourceItemData as IStackable, targetItemData as IStackable, prevIndex, newIndex);

        UpdateEmptySlotQueueAfterSwap(prevIndex, newIndex, sourceItemData, targetItemData);

        OnChangedInventoryData?.Invoke(currentInventoryDataList, false);
    }

    private void UpdateStackableLookupAfterSwap(IStackable sourceStackable, IStackable targetStackable, int prevIndex,
        int newIndex)
    {
        // sourceItemData (newIndex로 이동) 갱신
        if (sourceStackable == null && targetStackable == null) return;
        if (sourceStackable != null)
            _stackableLookup.Add(sourceStackable.ItemID, newIndex, sourceStackable);

        if (targetStackable != null)
            _stackableLookup.Add(targetStackable.ItemID, prevIndex, targetStackable);
        else
        {
            _stackableLookup.RemoveFromLookup(sourceStackable.ItemID, prevIndex);
        }
    }

    private void UpdateEmptySlotQueueAfterSwap(int prevIndex, int newIndex, ItemDataBase sourceItemData,
        ItemDataBase targetItemData)
    {
        if (targetItemData == null)
        {
            _emptySlotPriorityQueue.Enqueue(prevIndex, prevIndex);
        }
        else
        {
            _emptySlotPriorityQueue.TryRemove(prevIndex);
        }

        if (sourceItemData == null)
        {
            _emptySlotPriorityQueue.Enqueue(newIndex, newIndex);
        }
        else
        {
            _emptySlotPriorityQueue.TryRemove(newIndex);
        }
    }

    public bool SplitItem(ItemDataBase itemData, int splitCount)
    {
        if (!canSettingData) return false;

        if (itemData is not IStackable stackable || splitCount <= 0 || splitCount >= stackable.StackCount || IsFull())
        {
            PJHDebug.LogWarning("Split failed: Invalid item, count, or inventory full.", tag: "InventorySO");
            return false;
        }

        ItemDataBase newItem = itemData.DeepCopy();
        if (Application.isPlaying)
            AttributeInjector.Inject(newItem, SceneManager.GetActiveScene().GetSceneContainer());
        IStackable newStackable = newItem as IStackable;

        newStackable.StackCount = splitCount;
        stackable.StackCount -= splitCount;

        int emptySlotIndex = FindEmptySlotIndex();
        if (emptySlotIndex < 0) return false;

        currentInventoryDataList[emptySlotIndex] = newItem;
        _emptySlotPriorityQueue.TryRemove(emptySlotIndex);

        _stackableLookup.Add(newItem.ItemID, emptySlotIndex, newStackable);
        if (_stackableLookup.TryGetSlotIndex(stackable, out int originalIndex))
        {
            _stackableLookup.Add(itemData.ItemID, originalIndex, stackable);
        }

        CurrentInventorySlotCount++;

        if (canAutoSort) SortData();

        OnChangedInventoryData?.Invoke(currentInventoryDataList, true);
        return true;
    }

    public bool RemoveItem(ItemDataBase dataToRemove)
    {
        if (!canSettingData || dataToRemove == null) return false;
        int removeIndex = currentInventoryDataList.FindIndex(x => x == dataToRemove);
        return RemoveItemAt(removeIndex);
    }

    public bool RemoveItemAt(int index, bool invokeEvent = true)
    {
        if (!canSettingData || index < 0 || index >= currentInventoryDataList.Count)
        {
            PJHDebug.LogWarning($"Index {index} is out of range. Cannot remove item.", tag: "InventorySO");
            return false;
        }

        ItemDataBase dataToRemove = currentInventoryDataList[index];
        if (dataToRemove == null) return false;

        if (dataToRemove is IStackable stackable)
        {
            _stackableLookup.Remove(stackable);
        }

        currentInventoryDataList[index] = null;
        _emptySlotPriorityQueue.Enqueue(index, index);

        if (invokeEvent)
            CurrentInventorySlotCount--;
        else
            CurrentInventorySlotCount--;
        if (canAutoSort) SortData();
        if (invokeEvent)
            OnChangedInventoryData?.Invoke(currentInventoryDataList, false);

        return true;
    }

    public int FindEmptySlotIndex()
    {
        if (_emptySlotPriorityQueue.Count > 0)
            return _emptySlotPriorityQueue.Dequeue();

        return -1;
    }

    public void StackAll()
    {
        if (!canSettingData) return;

        // 스택 합치기 위한 딕셔너리(이 스택을 기준으로 병합)
        var mergedStacks = new Dictionary<int, IStackable>(32);
        bool anyMerged = false;
        bool prevAutoSort = canAutoSort;
        canAutoSort = false;
        for (int i = 0; i < currentInventoryDataList.Count; i++)
        {
            var data = currentInventoryDataList[i];
            if (data is not IStackable stackable)
                continue;
            int itemID = data.ItemID;

            if (mergedStacks.TryGetValue(itemID, out var target))
            {
                // 남은 공간만큼 합치기
                int available = target.MaxStackCount - target.StackCount;
                if (available <= 0)
                {
                    // 기준이 꽉 찼다면 새 기준 등록
                    if (stackable.StackCount > 0)
                        mergedStacks[itemID] = stackable;
                    continue;
                }

                int transfer = stackable.StackCount > available ? available : stackable.StackCount;

                target.StackCount += transfer;
                stackable.StackCount -= transfer;
                anyMerged = true;

                // 만약 다 합져질 경우 아이템 제거
                if (stackable.StackCount <= 0)
                {
                    RemoveItemAt(i, false);
                }

                // 만약 기존 스택이 꽉 찼다면 새로운 스택으로 갱신
                if (target.StackCount >= target.MaxStackCount && stackable.StackCount > 0)
                    mergedStacks[itemID] = stackable;
            }
            else
            {
                mergedStacks[itemID] = stackable;
            }
        }

        canAutoSort = prevAutoSort;
        RefreshLookupsAndQueue();

        if (canAutoSort) SortData();
        if (anyMerged)
        {
            OnChangedInventoryData?.Invoke(currentInventoryDataList, true);
            OnChangedCurrentInventorySlotCount?.Invoke();
        }
    }

    public void SortData()
    {
        if (!canSettingData) return;

        IInventoryComparer comparer = sortType switch
        {
            Define.InventorySortType.ByName => new NameComparer(),
            Define.InventorySortType.ByRank => new RankComparer(),
            Define.InventorySortType.ByCount => new CountComparer(),
            Define.InventorySortType.ByType => new TypeComparer(),
            Define.InventorySortType.ByAll => new AllComparer(),
            _ => new NameComparer()
        };

        currentInventoryDataList.Sort(comparer.Compare);

        RefreshLookupsAndQueue();

        OnChangedInventoryData?.Invoke(currentInventoryDataList, true);
    }

    public void RefreshLookupsAndQueue()
    {
        _stackableLookup.Refresh(currentInventoryDataList);
        RefreshEmptySlotPriorityQueue();
    }

    public void RefreshEmptySlotPriorityQueue()
    {
        _emptySlotPriorityQueue.Clear();

        for (int i = 0; i < currentInventoryDataList.Count; i++)
        {
            if (currentInventoryDataList[i] == null)
                _emptySlotPriorityQueue.Enqueue(i, i);
        }
    }
}