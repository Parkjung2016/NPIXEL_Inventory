using System;
using MemoryPack;
using UnityEngine;


public enum OperationType
{
    Sum,
    Sub
}

[Serializable]
[MemoryPackable]
public partial class AdditionalItemAttribute
{
    public ItemAttribute additionalAttribute;
    public float value;
    public OperationType operationType;
}

