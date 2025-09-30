using System;
using MemoryPack;
using UnityEngine;

[MemoryPackable]
[Serializable]
public partial class PotionItemData : ItemData, IUsable, IStackable
{
    public int StackCount { get; set; }
    public int MaxStackCount { get; private set; }

    public void Use()
    {
    }
}