public class StackableItemDataSO : BaseItemDataSO
{
    public StackableItemData itemData = new();

    public override ItemDataBase GetItemData()
    {
        return itemData;
    }
}