using UnityEngine;

public class PotionItemDataSO : BaseItemDataSO
{
    public PotionItemData itemData = new();

    public override ItemData GetItemData()
    {
        return itemData;
    }
}