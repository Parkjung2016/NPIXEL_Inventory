using System.Collections.Generic;
using PJH.Utility.Extensions;
using UnityEditor;
using UnityEngine;
using ZLinq;


[CreateAssetMenu(menuName = "SO/Item/ItemDataListSO")]
public class ItemDataListSO : ScriptableObject
{
    [SerializeField] private List<BaseItemDataSO> _itemDataList;
    public IList<BaseItemDataSO> ItemDataList => _itemDataList;

    public ItemDataBase GetRandomItemData()
    {
        if (_itemDataList.Count == 0) return null;
        return _itemDataList.Random().GetItemData();
    }

    public ItemDataBase GetRandomItemData(Define.ItemType itemType)
    {
        var filteredList = _itemDataList.AsValueEnumerable().Where(item => item.GetItemData().itemType == itemType)
            .ToList();
        if (filteredList.Count == 0) return null;
        return filteredList.Random().GetItemData();
    }

    private void OnValidate()
    {
        for (int i = 0; i < _itemDataList.Count; i++)
        {
            _itemDataList[i].GetItemData().ItemID = i;
            EditorUtility.SetDirty(_itemDataList[i]);
        }

        EditorApplication.delayCall -= OnDelayCalled;
        EditorApplication.delayCall += OnDelayCalled;
    }

    private void OnDelayCalled()
    {
        AssetDatabase.SaveAssets();
    }
}