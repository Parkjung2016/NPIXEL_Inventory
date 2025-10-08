using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEditor;
using UnityEngine;


[MemoryPackable]
[MemoryPackUnion(0, typeof(RecoveryHealthEffect))]
[MemoryPackUnion(1, typeof(RecoveryManaEffect))]
[Serializable]
public abstract partial class UsableItemEffect
{
    public List<ItemAttribute> requiredAttributes;
    public abstract void UseItem(IList<ItemAttribute> attributes, IList<AdditionalItemAttribute> additionalAttributes);
}

public abstract class UsableItemEffectSO : ScriptableObject
{
    public List<ItemAttributeSO> requiredAttributes = new();
    public abstract UsableItemEffect GetUsableItemEffect();

    private void OnValidate()
    {
        if (GetUsableItemEffect() == null) return;
        GetUsableItemEffect().requiredAttributes = new();
        foreach (var attribute in requiredAttributes)
        {
            if (attribute != null)
            {
                GetUsableItemEffect().requiredAttributes.Add(attribute.attribute);
            }
        }

        EditorUtility.SetDirty(this);
    }
}