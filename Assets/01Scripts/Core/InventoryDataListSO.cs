using System.Collections.Generic;
using PJH.Utility.Extensions;
using UnityEditor;
using UnityEngine;
using ZLinq;


[CreateAssetMenu(menuName = "SO/Inventory/InventoryDataListSO")]
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

    private void OnValidate()
    {
        for (int i = 0; i < _inventoryDataList.Count; i++)
        {
            _inventoryDataList[i].GetItemData().itemID = i;
            EditorUtility.SetDirty(_inventoryDataList[i]);
        }

        EditorApplication.delayCall -= OnDelayCalled;
        EditorApplication.delayCall += OnDelayCalled;
    }

    private void OnDelayCalled()
    {
        AssetDatabase.SaveAssets();
    }
}