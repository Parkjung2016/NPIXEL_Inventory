public class EquipmentItemDataSO : BaseItemDataSO
{
    public EquipmentItemData itemData = new();

    public override ItemDataBase GetItemData()
    {
        return itemData;
    }
}