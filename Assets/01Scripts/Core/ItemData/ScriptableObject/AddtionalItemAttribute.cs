using System;
using MemoryPack;


public enum OperationType
{
    Sum,
    Sub,
    PercentAdd,
    PercentSub
}

[Serializable]
[MemoryPackable]
public partial struct AdditionalItemAttribute
{
    public ItemAttribute additionalAttribute;
    public float value;
    public OperationType operationType;
}