using System;
using UnityEditor;
using UnityEngine;

public static class InventoryEditorUtility
{
    public static T LoadAsset<T>(string relativePath) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>($"Assets/{relativePath}");
    }

    public static void AddNewItem(InventoryDataListSO targetSO, Type type)
    {
        BaseItemDataSO newItem = (BaseItemDataSO)ScriptableObject.CreateInstance(type);
        newItem.name = $"New {type.Name}";
        var itemData = newItem.GetItemData();
        itemData.displayName = type.Name;
        itemData.itemID = targetSO.InventoryDataList.Count;

        string path = $"Assets/03SO/ItemData/{newItem.name}_{Guid.NewGuid()}.asset";
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();

        targetSO.InventoryDataList.Add(newItem);
        EditorUtility.SetDirty(targetSO);
    }

    public static void DeleteItem(InventoryDataListSO targetSO, int index)
    {
        if (index < 0 || index >= targetSO.InventoryDataList.Count)
            return;

        var item = targetSO.InventoryDataList[index];
        targetSO.InventoryDataList.RemoveAt(index);

        string assetPath = AssetDatabase.GetAssetPath(item);
        if (!string.IsNullOrEmpty(assetPath))
            AssetDatabase.DeleteAsset(assetPath);

        EditorUtility.SetDirty(targetSO);
        AssetDatabase.SaveAssets();
    }
}