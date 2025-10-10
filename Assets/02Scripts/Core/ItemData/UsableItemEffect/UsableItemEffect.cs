using System;
using System.Collections.Generic;
using MemoryPack;
using PJH.Utility.Managers;

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