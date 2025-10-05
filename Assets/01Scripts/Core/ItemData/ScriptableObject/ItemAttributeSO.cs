using System;
using MemoryPack;
using UnityEditor;
using UnityEngine;

[Serializable]
[MemoryPackable]
public partial struct ItemAttribute
{
    [Delayed] public string attributeName;
    public float attributeValue;
}

[CreateAssetMenu(menuName = "SO/Item/Attribute/AttributeSO")]
public class ItemAttributeSO : ScriptableObject
{
    public ItemAttribute attribute = new();

    private void OnValidate()
    {
        RenameAsset();
    }

    private void RenameAsset()
    {
        if (this == null) return;
        string oldName = $"ItemAttribute_{attribute.attributeName}";
        if (oldName != name)
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(assetPath))
                AssetDatabase.RenameAsset(assetPath, oldName);
        }
    }
}