using System.Collections.Generic;
using PJH.Utility.Extensions;
using UnityEngine;

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
}