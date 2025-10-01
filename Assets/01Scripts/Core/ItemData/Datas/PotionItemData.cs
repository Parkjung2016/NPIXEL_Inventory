using System;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial class PotionItemData : ItemDataBase, IUsable, IStackable
{
    public int StackCount { get; set; }
    [field: SerializeField] public int MaxStackCount { get; private set; }

    public PotionItemData()
    {
        itemType = ItemType.Consumable;
    }

    public void Use()
    {
    }
}