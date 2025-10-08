using System;
using PJH.Utility;
using Reflex.Attributes;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/ItemManagerSO")]
public class ItemManagerSO : ScriptableObject
{
    public event Action<ItemDataBase> OnItemEquipped;
    public event Action<ItemDataBase> OnItemUnequipped;
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
                bool isEmpty = stackable.StackCount == 0;
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

        if (!RemoveItem(itemData))
        {
            if (itemData is IEquipable equipable)
            {
                EquipmentHandler.Unequip(itemData, equipable.GetBaseAttributeKey(),
                    equipable.GetAdditionalAttributeKey());
                OnItemUnequipped?.Invoke(itemData);
            }
        }
    }

    public bool RemoveItem(ItemDataBase itemData)
    {
        return _inventoryListSO.RemoveItem(itemData);
    }

    public void EquipItem(ItemDataBase itemData)
    {
        if (itemData is not IEquipable equipable || equipable.IsEquipped) return;
        _inventoryListSO.RemoveItem(itemData);
        EquipmentHandler.Equip(itemData, equipable.GetBaseAttributeKey(),
            equipable.GetAdditionalAttributeKey());
        OnItemEquipped?.Invoke(itemData);
    }

    public void EquipItemForce(ItemDataBase itemData)
    {
        if (itemData is not IEquipable equipable) return;
        EquipmentHandler.Equip(itemData, equipable.GetBaseAttributeKey(),
            equipable.GetAdditionalAttributeKey());
        OnItemEquipped?.Invoke(itemData);
    }

    public int UnequipItem(ItemDataBase itemData)
    {
        if (itemData is not IEquipable equipable || !equipable.IsEquipped) return -1;

        EquipmentHandler.Unequip(itemData, equipable.GetBaseAttributeKey(),
            equipable.GetAdditionalAttributeKey());
        OnItemUnequipped?.Invoke(itemData);
        ItemDataBase addedItem = _inventoryListSO.AddItem(itemData);
        if (addedItem != null)
        {
            return _inventoryListSO.GetItemDataIndex(addedItem);
        }

        return -1;
    }
}