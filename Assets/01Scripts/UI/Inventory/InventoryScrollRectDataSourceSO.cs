using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MemoryPack;
using Reflex.Attributes;
using UnityEngine;

[Serializable]
[MemoryPackable]
public partial class InventoryScrollRectDataSource : IOptimizeScrollRectDataSource
{
    [MemoryPackIgnore] public Action OnUpdateItemCount { get; set; }
    public int dataLength;

    [MemoryPackIgnore] public List<ItemDataBase> itemDataList = new();

    public InventorySortType sortType;

    public void InitData()
    {
        itemDataList = new List<ItemDataBase>(dataLength);
        for (int i = 0; i < dataLength; i++)
        {
            itemDataList.Add(null);
        }
    }

    public void ClearData()
    {
        if (itemDataList.Count == 0)
        {
            InitData();
            return;
        }

        for (int i = 0; i < dataLength; i++)
        {
            itemDataList[i] = null;
        }
    }

    public void AddDataLength(int lengthToAdd)
    {
        if (lengthToAdd <= 0)
        {
            Debug.LogError("Length to add must be greater than zero.");
            return;
        }

        dataLength += lengthToAdd;

        for (int i = 0; i < lengthToAdd; i++)
        {
            itemDataList.Add(null);
        }

        OnUpdateItemCount?.Invoke();
    }

    public void AddData(ItemDataBase dataToAdd)
    {
        int emptyIndex = itemDataList.FindIndex(x => x == null);

        if (emptyIndex >= 0)
        {
            itemDataList[emptyIndex] = dataToAdd;
            SortData();
        }
        else
        {
            Debug.LogError("No empty slot available to add the item.");
        }
    }

    public void RemoveData(ItemDataBase dataToRemove)
    {
        int removeIndex = itemDataList.FindIndex(data => data == dataToRemove);
        Debug.Log(removeIndex);

        RemoveData(removeIndex);
    }

    public void RemoveData(int removeIndex)
    {
        if (removeIndex >= 0)
        {
            itemDataList[removeIndex] = null;
            SortData();
        }
        else
        {
            Debug.LogError("Item to remove not found in the list.");
        }
    }

    public void SortData()
    {
        itemDataList.Sort((a, b) =>
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
    }

    public int GetItemCount()
    {
        return dataLength;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as ItemSlotUI;
        item.ConfigureCell(itemDataList[index], index);
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

[CreateAssetMenu]
public class InventoryScrollRectDataSourceSO : ScriptableObject, ISaveable
{
    public event Action<InventoryScrollRectDataSource> OnLoadedInventoryData;
    [field: SerializeField] public int SaveID { get; private set; }
    [Inject] public SaveManagerSO SaveManagerSO { get; private set; }
    public InventoryScrollRectDataSource dataSource = new InventoryScrollRectDataSource();

    public void Init()
    {
        SaveManagerSO?.RegisterSaveable(this);
        ;
    }

    private void OnDisable()
    {
        SaveManagerSO?.UnregisterSaveable(this);
    }

    private void OnValidate()
    {
        dataSource.InitData();
    }

    public InventoryScrollRectDataSourceSO Clone()
    {
        InventoryScrollRectDataSourceSO clone = Instantiate(this);
        clone.dataSource = clone.dataSource.DeepCopy();
        return clone;
    }

    #region saveable

    public async UniTask<byte[]> ParsingToBytes()
    {
        using var stream = new MemoryStream();
        await MemoryPackSerializer.SerializeAsync(stream, dataSource);
        return stream.ToArray();
    }

    public async UniTask ParsingFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        dataSource = await MemoryPackSerializer.DeserializeAsync<InventoryScrollRectDataSource>(stream);
        dataSource.InitData();
    }

    public void AllLoaded()
    {
        OnLoadedInventoryData?.Invoke(dataSource);
        dataSource.OnUpdateItemCount?.Invoke();
    }

    #endregion
}