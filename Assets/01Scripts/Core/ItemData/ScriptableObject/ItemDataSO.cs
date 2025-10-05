using System;
using MemoryPack;

[MemoryPackable]
[Serializable]
public partial class ItemData : ItemDataBase
{
}

public class ItemDataSO : BaseItemDataSO
{
    public ItemData itemData = new();

    public override ItemDataBase GetItemData()
    {
        return itemData;
    }
}