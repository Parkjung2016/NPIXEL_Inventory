using System;
using System.Collections.Generic;
using MemoryPack;
using PJH.Attributes;
using PJH.Utility.Managers;
using Reflex.Attributes;
using UnityEngine;


public enum ItemRank
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum ItemType
{
    Equipment,
    Consumable,
    Material
}

public enum ItemDetailType
{
    Armor,
    Boots,
    Gloves,
    Helmet,
    Leggings,
    MeleeWeapon,
    Pendent,
    Ring,
    Shield,
    Chest,
    Potion,
    SpellBook,
}

[MemoryPackable]
[MemoryPackUnion(0, typeof(ItemData))]
[MemoryPackUnion(1, typeof(PotionItemData))]
[MemoryPackUnion(2, typeof(EquipmentItemData))]
[Serializable]
public abstract partial class ItemDataBase
{
    public string displayName;
    public string description;
    [ReadOnly] public int itemID;

    public ItemType itemType;
    public ItemDetailType detailType;

    [HideInInspector] public string iconKey;
    public ItemRank rank;
    public Guid uniqueID;
    [HideInInspector] public List<ItemAttribute> baseAttributes = new();
    [HideInInspector] public List<AdditionalItemAttribute> additionalAttributes = new();

    [NonSerialized] [MemoryPackIgnore] [Inject]
    private EnumStringMappingSO _enumStringMappingSO;

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
        clone.itemID = itemID;
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