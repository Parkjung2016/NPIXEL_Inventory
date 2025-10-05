using System.Collections.Generic;
using PJH.Utility.Extensions;
using UnityEngine;
using ZLinq;

[CreateAssetMenu]
public class InventoryDataListSO : ScriptableObject
{
    [SerializeField] private List<BaseItemDataSO> _inventoryDataList;
    public IList<BaseItemDataSO> InventoryDataList => _inventoryDataList;

    public ItemDataBase GetRandomInventoryData()
    {
        if (_inventoryDataList.Count == 0) return null;
        return _inventoryDataList.Random().GetItemData();
    }

    public ItemDataBase GetRandomInventoryData(ItemType itemType)
    {
        var filteredList = _inventoryDataList.AsValueEnumerable().Where(item => item.GetItemData().itemType == itemType)
            .ToList();
        if (filteredList.Count == 0) return null;
        return filteredList.Random().GetItemData();
    }
}