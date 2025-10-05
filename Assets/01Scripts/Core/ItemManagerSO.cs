using System;
using PJH.Utility;
using Reflex.Attributes;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/ItemManagerSO")]
public class ItemManagerSO : ScriptableObject
{
    [Inject] private InventoryListSO _inventoryListSO;
    public event Action<ItemDataBase> OnUsedItemWithStackable;


    public void UseItem(ItemDataBase itemData)
    {
        if (itemData is IUsable usable)
        {
            usable.Use();

            if (itemData is IStackable stackable)
            {
                stackable.StackCount = Mathf.Max(0, stackable.StackCount - 1);
                bool isEmpty = stackable.StackCount <= 0;
                if (isEmpty)
                {
                    PJHDebug.LogColorPart($"Item used up! :{itemData.displayName} ", Color.yellow,
                        tag: "ItemManagerSO");
                    RemoveItem(itemData);
                }
                else
                {
                    OnUsedItemWithStackable?.Invoke(itemData);
                }
            }
            else
                RemoveItem(itemData);
        }
    }

    public void ChangeItemDataIndex(ItemDataBase itemData, int prevIndex, int newIndex)
    {
        _inventoryListSO.ChangeItemDataIndex(itemData, prevIndex, newIndex);
    }

    public void DeleteItem(ItemDataBase itemData)
    {
        PJHDebug.LogColorPart($"Item deleted! :{itemData.displayName}", Color.red, tag: "ItemManagerSO");

        RemoveItem(itemData);
    }

    public void RemoveItem(ItemDataBase itemData)
    {
        _inventoryListSO.RemoveItem(itemData);
    }
}