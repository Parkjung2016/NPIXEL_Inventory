using System;
using UnityEditor;
using UnityEngine;

public abstract class BaseItemDataSO : ScriptableObject
{
    public abstract ItemDataBase GetItemData();

    private void OnValidate()
    {
        if (EditorApplication.isUpdating) return; // 임포트 중이면 실행하지 않음
        EditorApplication.delayCall -= RenameAsset;
        EditorApplication.delayCall += RenameAsset;
    }

    private void RenameAsset()
    {
        if (this == null) return;
        string oldName = $"{GetType().Name}_{GetItemData().displayName}";
        if (oldName != name)
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(assetPath))
                AssetDatabase.RenameAsset(assetPath, oldName);
        }
    }
}