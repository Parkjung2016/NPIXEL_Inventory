using UnityEngine;

[CreateAssetMenu]
public class ItemDataSO : BaseItemDataSO
{
    public ItemData itemData = new();

    public override ItemData GetItemData()
    {
        return itemData;
    }
}