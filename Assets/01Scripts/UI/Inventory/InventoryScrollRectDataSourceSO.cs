using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class InventoryScrollRectDataSourceSO : ScriptableObject, IOptimizeScrollRectDataSource
{
    [SerializeField] private int dataLength;
    [field: SerializeField] public RectTransform CellPrefab { get; private set; }

    private int _currentCellCount;

    private List<ItemDataBase> _itemDataList;

    public IList<ItemDataBase> ItemDataList => _itemDataList;

    public InventorySortType sortType;

    private void OnValidate()
    {
        InitData();
    }

    private void InitData()
    {
        _itemDataList = new List<ItemDataBase>(dataLength);
        for (int i = 0; i < dataLength; i++)
        {
            _itemDataList.Add(null);
        }
    }

    public void ClearData()
    {
        if (_itemDataList.Count == 0)
        {
            InitData();
            return;
        }

        for (int i = 0; i < dataLength; i++)
        {
            _itemDataList[i] = null;
        }
    }

    public void AddData(ItemDataBase dataToAdd)
    {
        int emptyIndex = _itemDataList.FindIndex(x => x == null);

        if (emptyIndex >= 0)
        {
            _itemDataList[emptyIndex] = dataToAdd;
            SortData();
        }
        else
        {
            Debug.LogError("No empty slot available to add the item.");
        }
    }

    public void RemoveData(ItemDataBase dataToRemove)
    {
        int removeIndex = _itemDataList.FindIndex(data => data == dataToRemove);
        Debug.Log(removeIndex);

        RemoveData(removeIndex);
    }

    public void RemoveData(int removeIndex)
    {
        if (removeIndex >= 0)
        {
            _itemDataList[removeIndex] = null;
            SortData();
        }
        else
        {
            Debug.LogError("Item to remove not found in the list.");
        }
    }

    public void SortData()
    {
        _itemDataList.Sort((a, b) =>
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
                    int countCompare = CompareByCount(a, b);
                    return countCompare;
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
        item.ConfigureCell(_itemDataList[index], index);
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

    #endregion
}