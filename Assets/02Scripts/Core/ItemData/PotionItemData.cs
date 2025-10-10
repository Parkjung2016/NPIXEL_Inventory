using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial class PotionItemData : ItemDataBase, IUsable, IStackable
{
    public int StackCount { get; set; }
    [field: SerializeField] public int MaxStackCount { get; set; }

    public List<UsableItemEffect> UsableItemEffects { get; set; }

    public PotionItemData()
    {
        itemType = Define.ItemType.Consumable;
    }

    public void Use()
    {
        for (int i = 0; i < UsableItemEffects.Count; i++)
        {
            UsableItemEffects[i].UseItem(baseAttributes, additionalAttributes);
        }
    }

    public override ItemDataBase Clone()
    {
        PotionItemData clone = new PotionItemData
        {
            displayName = displayName,
            detailType = detailType,
            description = description,
            ItemID = base.ItemID,
            itemType = itemType,
            iconKey = iconKey,
            rank = rank,
            baseAttributes = new List<ItemAttribute>(baseAttributes),
            additionalAttributes = new List<AdditionalItemAttribute>(additionalAttributes),
            UsableItemEffects = UsableItemEffects,
            uniqueID = Guid.NewGuid()
        };
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