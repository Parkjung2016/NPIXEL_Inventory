using System;
using System.Collections.Generic;
using MemoryPack;

[MemoryPackable]
[Serializable]
public partial class PlayerStatusData
{
    public Dictionary<Define.ItemDetailType, ItemDataBase> equippedItems = new();

    public StatData[] statDatas;
}