using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class InventoryDataListSO : ScriptableObject
{
    public List<BaseItemDataSO> inventoryDataList;

    public ItemData GetRandomInventoryData()
    {
        if (inventoryDataList.Count == 0) return null;
        int randomIndex = Random.Range(0, inventoryDataList.Count);
        return inventoryDataList[randomIndex].GetItemData();
    }
}