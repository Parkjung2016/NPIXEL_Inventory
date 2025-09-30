using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class InventoryScrollRectDataSourceSO : ScriptableObject, IOptimizeScrollRectDataSource
{
    [SerializeField] private int _dataLength;
    [field: SerializeField] public RectTransform CellPrefab { get; private set; }

    private int _currentCellCount;

    private List<ItemData> _itemDataList;

    public IList<ItemData> ItemDataList => _itemDataList;

    public InventorySortType sortType;

    private void OnValidate()
    {
        InitData();
    }

    private void InitData()
    {
        _itemDataList = new List<ItemData>(_dataLength);
        for (int i = 0; i < _dataLength; i++)
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

        for (int i = 0; i < _dataLength; i++)
        {
            _itemDataList[i] = null;
        }
    }

    public void AddData(ItemData dataToAdd)
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

    public void RemoveData(ItemData dataToRemove)
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

    private void SortData()
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
                    break;
                }
                case InventorySortType.ByType:
                {
                    return CompareByRank(a, b);
                    break;
                }
                case InventorySortType.Both:
                {
                    int typeCompare = CompareByRank(a, b);
                    if (typeCompare == 0)
                        return CompareByName(a, b);
                    return typeCompare;
                    break;
                }
            }

            return 0;
        });
    }

    int CompareByName(ItemData a, ItemData b)
    {
        return string.Compare(a.displayName, b.displayName, StringComparison.Ordinal);
    }

    int CompareByRank(ItemData a, ItemData b)
    {
        int rankCompare = b.rank.CompareTo(a.rank);
        if (rankCompare != 0) return rankCompare;
        return 0;
    }

    public int GetItemCount()
    {
        return _dataLength;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as ItemSlotUI;
        item.ConfigureCell(_itemDataList[index], index);
    }
}