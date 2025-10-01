public class PotionItemDataSO : BaseItemDataSO
{
    public PotionItemData itemData = new();

    public override ItemDataBase GetItemData()
    {
        return itemData;
    }
}