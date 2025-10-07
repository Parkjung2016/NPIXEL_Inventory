using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MemoryPack;
using PJH.Utility;
using PJH.Utility.Extensions;
using Priority_Queue;
using Reflex.Attributes;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public InventorySortType sortType;
    public bool canAutoSort = true;
    public int currentInventorySlotCount;

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

    private Dictionary<int, List<IStackable>> _stackableLookup = new Dictionary<int, List<IStackable>>();

    // nuget : OptimizedPriorityQueue
    private SimplePriorityQueue<int> _emptySlotPriorityQueue = new SimplePriorityQueue<int>();


    public ItemDataBase GetItemDataAt(int index)
    {
        if (index < 0 || index >= currentInventoryDataList.Count)
        {
            PJHDebug.LogWarning($"Index {index} is out of range. Cannot get item.", tag: "InventorySO");
            return null;
        }

        return currentInventoryDataList[index];
    }

    public void ClearData()
    {
        currentInventoryDataList.Clear();
        _emptySlotPriorityQueue.Clear();
        currentInventorySlotCount = 0;
        for (int i = 0; i < inventorySlotCapacity; i++)
        {
            currentInventoryDataList.Add(null);
            _emptySlotPriorityQueue.Enqueue(i, i);
        }

        canSettingData = true;
    }

    public void AddInventorySlotCapacity(int countToAdd)
    {
        if (!canSettingData) return;
        if (countToAdd <= 0)
        {
            PJHDebug.LogWarning("Count to add must be greater than zero.", tag: "InventorySO");
            return;
        }

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

    public bool IsFull()
    {
        return currentInventorySlotCount >= inventorySlotCapacity;
    }

    public int FindEmptySlotIndex()
    {
        if (_emptySlotPriorityQueue.Count > 0)
            return _emptySlotPriorityQueue.Dequeue();

        return -1;
    }

    public void ChangeItemDataIndex(ItemDataBase itemData, int prevIndex, int newIndex)
    {
        if (!canSettingData) return;
        ItemDataBase prevItemData = currentInventoryDataList[newIndex];
        if (prevItemData != null && itemData.itemID == prevItemData.itemID)
        {
            if (prevItemData is IStackable prevStackable)
            {
                IStackable stackable = itemData as IStackable;
                if (prevStackable.StackCount + stackable.StackCount <= prevStackable.MaxStackCount)
                {
                    prevStackable.StackCount += stackable.StackCount;
                    RemoveItem(prevIndex);
                    return;
                }
            }
        }

        currentInventoryDataList[newIndex] = itemData;
        currentInventoryDataList[prevIndex] = prevItemData;
        if (prevItemData == null)
            _emptySlotPriorityQueue.Enqueue(prevIndex, prevIndex);
        OnChangedInventoryData?.Invoke(currentInventoryDataList, false);
    }

    public int GetItemDataIndex(ItemDataBase itemData)
    {
        return currentInventoryDataList.FindIndex(x => x == itemData);
    }

    public ItemDataBase AddItem(ItemDataBase itemData)
    {
        if (!canSettingData) return null;
        return AddItemInternal(itemData);
    }

    public void AddItems(IList<ItemDataBase> items)
    {
        if (!canSettingData) return;

        for (int j = 0; j < items.Count; j++)
            AddItemInternal(items[j], invokeEvent: false);
        if (canAutoSort)
        {
            SortData();
        }

        OnChangedCurrentInventorySlotCount?.Invoke();

        OnChangedInventoryData?.Invoke(currentInventoryDataList, true);
    }

    IStackable FindStackable(int itemID)
    {
        if (_stackableLookup.TryGetValue(itemID, out var list))
        {
            return list.FirstOrDefault();
        }

        return null;
    }

    private ItemDataBase AddItemInternal(ItemDataBase itemData, bool invokeEvent = true)
    {
        IStackable stackable = itemData as IStackable;
        if (stackable != null)
        {
            stackable = FindStackable(itemData.itemID);
            if (stackable != null)
            {
                stackable.StackCount++;
                if (stackable.StackCount >= stackable.MaxStackCount)
                {
                    RemoveStackableLookup(itemData.itemID, stackable);
                }

                if (invokeEvent)
                {
                    if (canAutoSort)
                    {
                        SortData();
                    }

                    OnChangedInventoryData?.Invoke(currentInventoryDataList, false);
                }

                return null;
            }
        }

        if (IsFull())
        {
            // PJHDebug.LogWarning("No empty slot available to add the item.", tag: "InventorySO");
            return null;
        }

        ItemDataBase dataInstance = itemData.Clone();
        AttributeInjector.Inject(dataInstance, SceneManager.GetActiveScene().GetSceneContainer());
        stackable = dataInstance as IStackable;
        if (stackable != null)
        {
            stackable.StackCount = 1;
            if (_stackableLookup.ContainsKey(dataInstance.itemID))
                _stackableLookup[dataInstance.itemID].Add(stackable);
            else
            {
                _stackableLookup.Add(dataInstance.itemID, new List<IStackable> { stackable });
            }
        }

        dataInstance.uniqueID = Guid.NewGuid();
        int emptySlotIndex = FindEmptySlotIndex();
        if (invokeEvent)
            CurrentInventorySlotCount++;
        else
            currentInventorySlotCount++;
        if (emptySlotIndex >= 0)
        {
            currentInventoryDataList[emptySlotIndex] = dataInstance;
            if (invokeEvent)
            {
                if (canAutoSort)
                {
                    SortData();
                }

                OnChangedInventoryData?.Invoke(currentInventoryDataList, false);
            }
        }

        return dataInstance;
    }

    public void SplitItem(ItemDataBase itemData, int splitCount)
    {
        if (!canSettingData) return;
        if (splitCount == 0)
        {
            PJHDebug.LogWarning("Split count cannot be zero", tag: "InventorySO");
            return;
        }

        if (itemData is IStackable stackable)
        {
            if (splitCount <= 0 || splitCount >= stackable.StackCount)
            {
                PJHDebug.LogWarning("Invalid split count", tag: "InventorySO");
                return;
            }

            ItemDataBase newItem = itemData.DeepCopy();
            AttributeInjector.Inject(newItem, SceneManager.GetActiveScene().GetSceneContainer());
            IStackable newItemStackable = (newItem as IStackable);
            newItemStackable.StackCount = splitCount;
            stackable.StackCount -= splitCount;
            int emptySlotIndex = FindEmptySlotIndex();
            currentInventoryDataList[emptySlotIndex] = newItem;
            AddStackableLookup(newItem.itemID, newItemStackable);

            CurrentInventorySlotCount++;
            OnChangedInventoryData?.Invoke(currentInventoryDataList, true);
        }
        else
        {
            PJHDebug.LogWarning("Item is not stackable", tag: "InventorySO");
        }
    }

    public bool RemoveItem(ItemDataBase dataToRemove)
    {
        if (!canSettingData) return false;
        if (dataToRemove == null)
        {
            PJHDebug.LogWarning("Cannot remove a null item.", tag: "InventorySO");
            return false;
        }

        int removeIndex = currentInventoryDataList.FindIndex(x => x == dataToRemove);
        return RemoveItem(removeIndex);
    }

    public bool RemoveItem(int index)
    {
        if (!canSettingData) return false;
        if (index >= 0 && index < currentInventoryDataList.Count)
        {
            ItemDataBase prevItemData = currentInventoryDataList[index];
            if (prevItemData is IStackable stackable)
            {
                RemoveStackableLookup(prevItemData.itemID, stackable);
            }

            currentInventoryDataList[index] = null;
            _emptySlotPriorityQueue.Enqueue(index, index);
            if (canAutoSort)
                SortData();
            CurrentInventorySlotCount--;
            OnChangedInventoryData?.Invoke(currentInventoryDataList, false);

            return true;
        }

        PJHDebug.LogWarning($"Index {index} is out of range. Cannot remove item.", tag: "InventorySO");
        return false;
    }

    private void AddStackableLookup(int index, IStackable stackable)
    {
        if (_stackableLookup.ContainsKey(index))
            _stackableLookup[index].Add(stackable);
        else
        {
            _stackableLookup.Add(index, new List<IStackable> { stackable });
        }
    }

    private void RemoveStackableLookup(int index, IStackable stackable)
    {
        if (!_stackableLookup.ContainsKey(index)) return;
        _stackableLookup[index].Remove(stackable);
        if (_stackableLookup[index].Count == 0)
        {
            _stackableLookup.Remove(index);
        }
    }

    public void SortData()
    {
        if (!canSettingData) return;
        currentInventoryDataList.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1; // null은 뒤로
            if (b == null) return -1;

            switch (sortType)
            {
                case InventorySortType.ByName:
                {
                    return CompareByName(a, b);
                }
                case InventorySortType.ByRank:
                {
                    return CompareByRank(a, b);
                }
                case InventorySortType.ByCount:
                {
                    return CompareByCount(a, b);
                }
                case InventorySortType.ByType:
                {
                    return CompareByType(a, b);
                }
                case InventorySortType.ByAll:
                {
                    int rankCompare = CompareByRank(a, b);
                    if (rankCompare != 0) return rankCompare;
                    int countCompare = CompareByCount(a, b);
                    if (countCompare != 0) return countCompare;
                    return CompareByName(a, b);
                }
            }

            return 0;
        });

        UpdateEmptySlotPriorityQueue();
    }

    private void UpdateEmptySlotPriorityQueue()
    {
        for (int i = 0; i < currentInventoryDataList.Count; i++)
        {
            if (currentInventoryDataList[i] != null && _emptySlotPriorityQueue.Contains(i))
            {
                _emptySlotPriorityQueue.Remove(i);
            }
            else if (currentInventoryDataList[i] == null && !_emptySlotPriorityQueue.Contains(i))
            {
                _emptySlotPriorityQueue.Enqueue(i, i);
            }
        }
    }

    #region compare functions

    int CompareByName(ItemDataBase a, ItemDataBase b)
    {
        return string.Compare(a.displayName, b.displayName, StringComparison.Ordinal);
    }

    int CompareByRank(ItemDataBase a, ItemDataBase b)
    {
        int rankCompare = b.rank.CompareTo(a.rank);
        if (rankCompare != 0) return rankCompare;
        return 0;
    }

    int CompareByCount(ItemDataBase a, ItemDataBase b)
    {
        IStackable stackableA = a as IStackable;
        IStackable stackableB = b as IStackable;

        int itemCountA = stackableA?.StackCount ?? 0;
        int itemCountB = stackableB?.StackCount ?? 0;

        // 내림차순: 큰 수 먼저
        int countCompare = itemCountB.CompareTo(itemCountA);
        return countCompare;
    }

    int CompareByType(ItemDataBase a, ItemDataBase b)
    {
        return a.itemType.CompareTo(b.itemType);
    }

    #endregion
}

[CreateAssetMenu(menuName = "SO/Inventory/InventorySO")]
public class InventorySO : ScriptableObject, ISaveable
{
    public event Action OnLoadedInventoryData;
    [field: SerializeField] public int SaveID { get; private set; }
    [Inject] public SaveManagerSO SaveManagerSO { get; private set; }

    public InventoryData inventoryData = new();

    public void Init()
    {
        ClearData();
        SaveManagerSO?.RegisterSaveable(this);
    }

    private void OnDisable()
    {
        SaveManagerSO?.UnregisterSaveable(this);
    }

    [ContextMenu("Clear Data")]
    public void ClearData()
    {
        inventoryData.ClearData();
    }

    #region saveable

    public async UniTask<byte[]> ParsingToBytes()
    {
        using var stream = new MemoryStream();
        await MemoryPackSerializer.SerializeAsync(stream, inventoryData);

        return stream.ToArray();
    }

    public async UniTask ParsingFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        inventoryData = await MemoryPackSerializer.DeserializeAsync<InventoryData>(stream);
        for (int i = 0; i < inventoryData.currentInventoryDataList.Count; i++)
        {
            ItemDataBase itemData = inventoryData.currentInventoryDataList[i];
            if (itemData == null) continue;
            AttributeInjector.Inject(itemData, SceneManager.GetActiveScene().GetSceneContainer());
        }
    }

    public void AllLoaded()
    {
        OnLoadedInventoryData?.Invoke();
        inventoryData.OnUpdateItemCount?.Invoke();
    }

    #endregion
}