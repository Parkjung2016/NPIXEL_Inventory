using System;
using UnityEditor;
using UnityEngine;

public static class ItemDataListWindowUtility
{
    public static T LoadAsset<T>(string relativePath) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>($"Assets/{relativePath}");
    }

    public static BaseItemDataSO AddNewItem(ItemDataListSO targetSO, Type type)
    {
        BaseItemDataSO newItem = (BaseItemDataSO)ScriptableObject.CreateInstance(type);
        newItem.name = $"New {type.Name}";
        var itemData = newItem.GetItemData();
        itemData.displayName = type.Name;
        itemData.ItemID = targetSO.ItemDataList.Count;

        string path = $"Assets/03SO/ItemData/{newItem.name}_{Guid.NewGuid()}.asset";
        AssetDatabase.CreateAsset(newItem, path);
        AssetDatabase.SaveAssets();

        targetSO.ItemDataList.Add(newItem);
        EditorUtility.SetDirty(targetSO);
        return newItem;
    }

    public static void DeleteItem(ItemDataListSO targetSO, int index)
    {
        if (index < 0 || index >= targetSO.ItemDataList.Count)
            return;

        var item = targetSO.ItemDataList[index];
        targetSO.ItemDataList.RemoveAt(index);

        string assetPath = AssetDatabase.GetAssetPath(item);
        if (!string.IsNullOrEmpty(assetPath))
            AssetDatabase.DeleteAsset(assetPath);

        EditorUtility.SetDirty(targetSO);
        AssetDatabase.SaveAssets();
    }
}