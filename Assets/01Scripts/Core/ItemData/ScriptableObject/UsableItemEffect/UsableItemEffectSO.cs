using System;
using System.Collections.Generic;
using MemoryPack;
using PJH.Utility.Managers;
using UnityEditor;
using UnityEngine;


[MemoryPackable]
[MemoryPackUnion(0, typeof(RecoveryHealthEffect))]
[MemoryPackUnion(1, typeof(RecoveryManaEffect))]
[Serializable]
public abstract partial class UsableItemEffect
{
    public string useSoundKey;
    public List<ItemAttribute> requiredAttributes;
    public abstract void UseItem(IList<ItemAttribute> attributes, IList<AdditionalItemAttribute> additionalAttributes);

    public SoundDataSO GetUseSound()
    {
        if (string.IsNullOrEmpty(useSoundKey)) return null;
        return AddressableManager.Load<SoundDataSO>(useSoundKey);
    }
}

public abstract class UsableItemEffectSO : ScriptableObject
{
    [MemoryPackIgnore] public SoundDataSO useSound;
    public List<ItemAttributeSO> requiredAttributes = new();
    public abstract UsableItemEffect GetUsableItemEffect();

    private void OnValidate()
    {
        if (GetUsableItemEffect() == null) return;
        GetUsableItemEffect().useSoundKey = useSound.name;
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