using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
public class ItemAttributeOverride
{
    public ItemAttributeSO itemAttribute;
    public float value;
}

[Serializable]
public class AdditionalItemAttributeClass
{
    public ItemAttributeSO additionalAttribute;
    public float value;
    public OperationType operationType;
}

public abstract class BaseItemDataSO : ScriptableObject
{
    public abstract ItemDataBase GetItemData();
    public List<ItemAttributeOverride> attributes;
    public List<AdditionalItemAttributeClass> additionalAttributes;

#if UNITY_EDITOR
    public virtual void OnValidate()
    {
        UpdateAttributeData();
        if (EditorApplication.isUpdating) return; // 임포트 중이면 실행하지 않음
        EditorApplication.delayCall -= RenameAsset;
        EditorApplication.delayCall += RenameAsset;
    }

    private void UpdateAttributeData()
    {
        ItemDataBase itemData = GetItemData();
        itemData.additionalAttributes.Clear();
        foreach (var additionalAttribute in additionalAttributes)
        {
            if (additionalAttribute == null || additionalAttribute.additionalAttribute == null) continue;
            AdditionalItemAttribute newOverride = new AdditionalItemAttribute
            {
                additionalAttribute = additionalAttribute.additionalAttribute.attribute,
                value = additionalAttribute.value,
                operationType = additionalAttribute.operationType
            };
            itemData.additionalAttributes.Add(newOverride);
        }

        itemData.baseAttributes.Clear();
        foreach (var attribute in attributes)
        {
            if (attribute == null || attribute.itemAttribute == null) continue;

            ItemAttribute newAttribute = new ItemAttribute
            {
                attributeName = attribute.itemAttribute.attribute.attributeName,
                attributeValue = attribute.value
            };
            itemData.baseAttributes.Add(newAttribute);
        }
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
#endif
}