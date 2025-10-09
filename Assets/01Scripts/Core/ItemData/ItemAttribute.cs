using System;
using MemoryPack;
using UnityEngine;

[Serializable]
[MemoryPackable]
public partial struct ItemAttribute
{
    [Delayed] public string attributeName;
    public float attributeValue;
}