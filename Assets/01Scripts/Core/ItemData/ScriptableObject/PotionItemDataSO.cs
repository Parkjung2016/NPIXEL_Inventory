using System;
using System.Collections.Generic;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial class PotionItemData : ItemDataBase, IUsable, IStackable
{
    [field: SerializeField] public int StackCount { get; set; }
    [field: SerializeField] public int MaxStackCount { get; set; }

    public PotionItemData()
    {
        itemType = ItemType.Consumable;
    }

    public void Use()
    {
    }

    public override ItemDataBase Clone()
    {
        PotionItemData clone = new PotionItemData
        {
            displayName = displayName,
            typeName = typeName,
            description = description,
            itemID = itemID,
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

public class PotionItemDataSO : BaseItemDataSO
{
    public PotionItemData itemData = new();

    public override ItemDataBase GetItemData()
    {
        return itemData;
    }
}