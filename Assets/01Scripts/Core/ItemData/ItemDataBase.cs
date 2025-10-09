using System;
using System.Collections.Generic;
using MemoryPack;
using PJH.Attributes;
using PJH.Utility.Managers;
using Reflex.Attributes;
using UnityEngine;

[MemoryPackable]
[MemoryPackUnion(0, typeof(ItemData))]
[MemoryPackUnion(1, typeof(PotionItemData))]
[MemoryPackUnion(2, typeof(EquipmentItemData))]
[MemoryPackUnion(3, typeof(StackableItemData))]
[Serializable]
public abstract partial class ItemDataBase
{
    public string displayName;
    public string description;
    [field: SerializeField, ReadOnly] public int ItemID { get; set; }

    public Define.ItemType itemType;
    public Define.ItemDetailType detailType;

    [HideInInspector] public string iconKey;
    public Define.ItemRank rank;
    public Guid uniqueID;
    [HideInInspector] public List<ItemAttribute> baseAttributes = new();
    [HideInInspector] public List<AdditionalItemAttribute> additionalAttributes = new();
    
    public Sprite GetIcon()
    {
        return AddressableManager.Load<Sprite>(iconKey);
    }

    public virtual ItemDataBase Clone()
    {
        ItemDataBase clone = new ItemData();
        clone.displayName = displayName;
        clone.detailType = detailType;
        clone.description = description;
        clone.ItemID = ItemID;
        clone.itemType = itemType;
        clone.iconKey = iconKey;
        clone.rank = rank;
        clone.baseAttributes = new List<ItemAttribute>(baseAttributes);
        clone.additionalAttributes = new List<AdditionalItemAttribute>(additionalAttributes);
        clone.uniqueID = Guid.NewGuid();
        if (this is IStackable stackable)
        {
            if (clone is IStackable cloneStackable)
            {
                cloneStackable.StackCount = stackable.StackCount;
                cloneStackable.MaxStackCount = stackable.MaxStackCount;
            }
        }

        return clone;
    }
}