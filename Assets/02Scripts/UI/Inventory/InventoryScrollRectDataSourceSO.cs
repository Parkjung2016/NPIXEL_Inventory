using UnityEngine;

[CreateAssetMenu(menuName = "SO/UI/InventoryScrollRectDataSourceSO")]
public class InventoryScrollRectDataSourceSO : ScriptableObject, IOptimizeScrollRectDataSource
{
    private InventorySO _inventorySO;

    public int GetItemCount()
    {
        return _inventorySO.inventoryData.inventorySlotCapacity;
    }

    public void SetInventorySO(InventorySO inventorySO)
    {
        _inventorySO = inventorySO;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as ItemSlotUI;
        item.ConfigureCell(_inventorySO.inventoryData.GetItemDataAt(index), index);
    }
}