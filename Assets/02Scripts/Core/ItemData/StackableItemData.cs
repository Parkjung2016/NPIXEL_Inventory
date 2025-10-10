using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial class StackableItemData : ItemDataBase, IStackable
{
    public int StackCount { get; set; }
    [field: SerializeField] public int MaxStackCount { get; set; }

    public override ItemDataBase Clone()
    {
        StackableItemData clone = new StackableItemData
        {
            displayName = displayName,
            detailType = detailType,
            description = description,
            ItemID = ItemID,
            itemType = itemType,
            iconKey = iconKey,
            rank = rank,
            baseAttributes = new List<ItemAttribute>(baseAttributes),
            additionalAttributes = new List<AdditionalItemAttribute>(additionalAttributes),
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