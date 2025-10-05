using System;
using MemoryPack;


public enum OperationType
{
    Sum,
    Sub
}

[Serializable]
[MemoryPackable]
public partial struct AdditionalItemAttribute
{
    public ItemAttribute additionalAttribute;
    public float value;
    public OperationType operationType;
}

